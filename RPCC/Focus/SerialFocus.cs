using System;
using System.Collections;
using System.IO;
using System.IO.Ports;
using System.Timers;
using RPCC.Utils;

namespace RPCC.Focus
{
    public static class SerialFocus
    {
        private static readonly object Sync = new object();

        private static readonly Timer ComTimer = new Timer();       // ожидание ответа на вопрос
        private static readonly Timer ReconnectTimer = new Timer(); // попытки реконнекта

        private static readonly SerialPort SerialPort = new SerialPort();

        private static volatile bool _initialized;
        private static volatile bool _connected;
        private static volatile bool _txEnabled;

        private static int _noReplyStreak;       // подряд таймаутов/нет ответа
        private static int _ioFailStreak;        // подряд I/O ошибок
        private static DateTime _lastRxUtc;      // когда последний раз что-то приняли
        private static DateTime _lastTxUtc;      // когда последний раз что-то отправили

        private const int ReplyTimeoutMs = 1000;
        private const int NoReplyLimit = 3;      // после N таймаутов считаем, что связи нет
        private const int IoFailLimit = 2;       // после N I/O ошибок сразу в дисконнект
        private const int ReconnectIntervalMs = 2000;
        
        private const int SerialReadTimeoutMs = 500;
        private const int SerialWriteTimeoutMs = 500;

        static SerialFocus()
        {
            Switches = new BitArray(8);
        }

        public static int CurrentPosition { get; private set; }
        public static int SpeedFast { get; private set; }
        public static int SpeedSlow { get; private set; }
        public static BitArray Switches { get; private set; }

        public static bool TransmissionEnabled => _txEnabled;
        public static bool IsConnected => _connected;

        // === Таймаут ожидания ответа ===
        private static void OnTimedEvent_Com(object sender, ElapsedEventArgs e)
        {
            ComTimer.Stop();

            // если мы ждали ответ и не получили — это сигнал проблемы
            lock (Sync)
            {
                _noReplyStreak++;
                _txEnabled = true; // разблокируем, но фиксируем деградацию
                if (_noReplyStreak >= NoReplyLimit)
                {
                    MarkDisconnected($"No reply for {_noReplyStreak} consecutive questions (timeout {ReplyTimeoutMs}ms).");
                }
            }
        }

        // === Периодический реконнект ===
        private static void OnTimedEvent_Reconnect(object sender, ElapsedEventArgs e)
        {
            // без lock на весь обработчик, чтобы не висеть
            if (_connected) return;
            TryReconnect();
        }

        // === Приём данных ===
        private static void SerialPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            // Важно: DataReceived может приходить пачками/внезапно; читаем аккуратно
            try
            {
                ComTimer.Stop();

                string indata;
                lock (Sync)
                {
                    // если уже в дисконнекте — не парсим
                    if (!_connected || !SerialPort.IsOpen)
                        return;
                }

                // ReadLine с NewLine = "\0"; с таймаутом он не должен висеть вечно
                // (настройки таймаута выставляются до открытия порта в Init/Open_Port).
                if (!SerialPort.IsOpen) return;
                indata = SerialPort.ReadLine();

                lock (Sync)
                {
                    _lastRxUtc = DateTime.UtcNow;
                    _noReplyStreak = 0;
                    _ioFailStreak = 0;
                }

                ParseReply(indata);

                // дочистка буфера (не обязательна, но оставим)
                try { SerialPort.ReadExisting(); } catch { /*ignore*/ }

                lock (Sync) { _txEnabled = true; }
            }
            catch (TimeoutException)
            {
                // ReadLine мог таймаутнуть (если NewLine не пришёл)
                lock (Sync)
                {
                    _noReplyStreak++;
                    _txEnabled = true;
                    if (_noReplyStreak >= NoReplyLimit)
                        MarkDisconnected($"ReadLine timeout. NoReplyStreak={_noReplyStreak}.");
                }
            }
            catch (IOException ex)
            {
                lock (Sync)
                {
                    _ioFailStreak++;
                    _txEnabled = false;
                    Logger.AddLogEntry("SERIAL FOCUS: IO error in DataReceived: " + ex.Message);
                    if (_ioFailStreak >= IoFailLimit)
                        MarkDisconnected($"IO errors in DataReceived: {_ioFailStreak}.");
                }
            }
            catch (InvalidOperationException ex)
            {
                lock (Sync)
                {
                    _txEnabled = false;
                    Logger.AddLogEntry("SERIAL FOCUS: InvalidOperation in DataReceived: " + ex.Message);
                    MarkDisconnected("SerialPort invalid operation (likely closed/disposed).");
                }
            }
            catch (Exception ex)
            {
                lock (Sync)
                {
                    _txEnabled = true; // не блокируем навсегда из-за разовой ошибки парсинга
                    Logger.AddLogEntry("SERIAL FOCUS: Unexpected error in DataReceived: " + ex);
                }
            }
        }

        private static void ParseReply(string indata)
        {
            try
            {
                // ожидаем формат типа: [1acp=1234] или похожее
                // у тебя reply = indata.Substring(1,3)
                if (string.IsNullOrEmpty(indata) || indata.Length < 6)
                    throw new FormatException("Reply too short: '" + indata + "'");

                var reply = indata.Substring(1, 3);

                byte value;
                var bits = new BitArray(8);
                int dig;

                if (reply == "aes")
                {
                    // твой код был странный: Convert.ToByte(indata.Substring(5)) и одновременно int.Parse(indata.Substring(5,3))
                    // здесь сделаем безопасно: сначала число после '='
                    var payload = ExtractPayload(indata);
                    if (!byte.TryParse(payload, out value))
                        throw new FormatException("aes payload is not byte: '" + payload + "'");

                    var gar = BitConverter.GetBytes(value);
                    bits = new BitArray(new[] { gar[0] });

                    // Reverse bits order
                    for (var i = 0; i < bits.Count / 2; i++)
                    {
                        var buf = bits[i];
                        bits[i] = bits[bits.Count - i - 1];
                        bits[bits.Count - i - 1] = buf;
                    }

                    dig = value; // если нужно числом тоже
                }
                else
                {
                    var payload = ExtractPayload(indata);
                    if (!int.TryParse(payload, out dig))
                        throw new FormatException("payload is not int: '" + payload + "'");
                }

                lock (Sync)
                {
                    switch (reply)
                    {
                        case "acp":
                            CurrentPosition = dig;
                            break;
                        case "asf":
                            SpeedFast = dig;
                            break;
                        case "ass":
                            SpeedSlow = dig;
                            break;
                        case "aes":
                            Switches = bits;
                            break;
                    }
                }
            }
            catch (Exception exception)
            {
                Logger.AddLogEntry("SERIAL FOCUS: Can't parse focus answer: '" + indata + "'");
                Logger.AddLogEntry(exception.Message);
            }
        }

        private static string ExtractPayload(string indata)
        {
            // ожидаем, что после '=' идёт число; если '=' нет — fallback к старому "Substring(5)"
            var eq = indata.IndexOf('=');
            if (eq >= 0 && eq + 1 < indata.Length)
                return indata.Substring(eq + 1).Trim('\0', '\r', '\n', ']', ' ');

            // старый стиль
            if (indata.Length > 5)
                return indata.Substring(5).Trim('\0', '\r', '\n', ']', ' ');

            return "";
        }

        public static void Set_Speed_Fast(int steps) => Write2Serial("2ssf=" + steps);
        public static void Set_Speed_Slow(int steps) => Write2Serial("2sss=" + steps);
        public static void Set_Zero() => Write2Serial("2szp");
        public static void FRun_To(int steps) => Write2Serial("2rfa=" + steps);
        public static void SRun_To(int steps) => Write2Serial("2rsl=" + steps);

        public static void Stop()
        {
            Logger.AddLogEntry("SERIAL FOCUS: stop");
            Write2Serial("2rst");
        }

        private static void Open_Port()
        {
            lock (Sync)
            {
                try
                {
                    SerialPort.PortName = "COM" + Settings.FocusComPort;
                    SerialPort.BaudRate = 9600;
                    SerialPort.DataBits = 8;

                    // желательно явно:
                    // SerialPort.Parity = Parity.None;
                    // SerialPort.StopBits = StopBits.One;
                    // SerialPort.Handshake = Handshake.None;

                    SerialPort.ReadTimeout = 500;
                    SerialPort.NewLine = "\0";
                    SerialPort.ReceivedBytesThreshold = 6;
                    SerialPort.WriteTimeout = SerialWriteTimeoutMs;

                    if (!SerialPort.IsOpen)
                        SerialPort.Open();

                    SerialPort.DiscardInBuffer();

                    _connected = SerialPort.IsOpen;
                    _txEnabled = _connected;
                    _noReplyStreak = 0;
                    _ioFailStreak = 0;
                    _lastRxUtc = DateTime.UtcNow;
                    _lastTxUtc = DateTime.UtcNow;

                    if (_connected)
                        Logger.AddLogEntry("SERIAL FOCUS: opened " + SerialPort.PortName);
                }
                catch (Exception ex)
                {
                    _connected = false;
                    _txEnabled = false;
                    Logger.AddLogEntry("SERIAL FOCUS: opening fails");
                    Logger.AddLogEntry(ex.ToString());
                }
            }
        }

        public static void Close_Port()
        {
            lock (Sync)
            {
                try
                {
                    ComTimer.Stop();
                    _txEnabled = false;
                    _connected = false;

                    if (SerialPort.IsOpen)
                        SerialPort.Close();

                    Logger.AddLogEntry("SERIAL FOCUS: closed");
                }
                catch (Exception ex)
                {
                    Logger.AddLogEntry("SERIAL FOCUS: closing fails");
                    Logger.AddLogEntry(ex.ToString());
                }
            }
        }

        private static void MarkDisconnected(string reason)
        {
            // вызывается только под lock(Sync)
            if (!_connected) return;

            _connected = false;
            _txEnabled = false;

            Logger.AddLogEntry("SERIAL FOCUS: DISCONNECTED: " + reason);

            try
            {
                ComTimer.Stop();
                if (SerialPort.IsOpen)
                    SerialPort.Close();
            }
            catch (Exception ex)
            {
                Logger.AddLogEntry("SERIAL FOCUS: error while closing on disconnect: " + ex.Message);
            }

            // запускаем реконнект-таймер
            try { ReconnectTimer.Start(); } catch { /*ignore*/ }
        }

        private static void TryReconnect()
        {
            lock (Sync)
            {
                if (_connected) return;

                // на всякий случай закрыть
                try { if (SerialPort.IsOpen) SerialPort.Close(); } catch { /*ignore*/ }

                Logger.AddLogEntry("SERIAL FOCUS: trying reconnect...");
                Open_Port();

                if (_connected)
                {
                    Logger.AddLogEntry("SERIAL FOCUS: reconnected");
                    try { ReconnectTimer.Stop(); } catch { /*ignore*/ }
                }
            }
        }

        public static bool Init()
        {
            lock (Sync)
            {
                if (_initialized) return _connected;

                _initialized = true;

                // таймеры
                ComTimer.Interval = ReplyTimeoutMs;
                ComTimer.AutoReset = false;
                ComTimer.Elapsed += OnTimedEvent_Com;

                ReconnectTimer.Interval = ReconnectIntervalMs;
                ReconnectTimer.AutoReset = true;
                ReconnectTimer.Elapsed += OnTimedEvent_Reconnect;

                // события порта
                SerialPort.DataReceived += SerialPort_DataReceived;
                // Важно: выставить таймауты/параметры чтения ДО открытия порта.
                // Иначе DataReceived может сработать сразу после Open(), а ReadLine() окажется без таймаута.
                SerialPort.ReadTimeout = SerialReadTimeoutMs;
                SerialPort.WriteTimeout = SerialWriteTimeoutMs;
                SerialPort.NewLine = "\0";
                SerialPort.ReceivedBytesThreshold = 6;
                
                
                Open_Port();

                if (!_connected)
                    ReconnectTimer.Start();

                return _connected;
            }
        }

        // === Отправка ===
        public static void Write2Serial(string command)
        {
            if (string.IsNullOrEmpty(command) || command.Length < 2) return;

            lock (Sync)
            {
                if (!_connected || !SerialPort.IsOpen) return;
                if (!_txEnabled) return;

                try
                {
                    _lastTxUtc = DateTime.UtcNow;

                    // run команды (r/s) — без ожидания ответа
                    if (command[1] == 'r' || command[1] == 's')
                    {
                        _txEnabled = false;
                        SerialPort.WriteLine(command);
                        _txEnabled = true;
                        return;
                    }

                    // question команды (g) — ждём ответ, блокируем следующее
                    if (command[1] == 'g')
                    {
                        SerialPort.DiscardInBuffer();
                        SerialPort.WriteLine(command);

                        _txEnabled = false;
                        ComTimer.Stop();
                        ComTimer.Start();
                        return;
                    }

                    // прочие — просто отправим
                    SerialPort.WriteLine(command);
                }
                catch (IOException ex)
                {
                    _ioFailStreak++;
                    Logger.AddLogEntry("SERIAL FOCUS: IO error on write: " + ex.Message);
                    if (_ioFailStreak >= IoFailLimit)
                        MarkDisconnected($"IO errors on write: {_ioFailStreak}.");
                    else
                        _txEnabled = false; // временно блокируем
                }
                catch (InvalidOperationException ex)
                {
                    Logger.AddLogEntry("SERIAL FOCUS: InvalidOperation on write: " + ex.Message);
                    MarkDisconnected("SerialPort invalid operation on write.");
                }
                catch (UnauthorizedAccessException ex)
                {
                    Logger.AddLogEntry("SERIAL FOCUS: UnauthorizedAccess on write: " + ex.Message);
                    MarkDisconnected("Unauthorized access to COM port.");
                }
                catch (Exception ex)
                {
                    Logger.AddLogEntry("SERIAL FOCUS: Unexpected error on write: " + ex);
                    // не всегда стоит сразу дисконнектить, но если хочешь жёстко — раскомментируй:
                    // MarkDisconnected("Unexpected write error.");
                }
            }
        }

        public static void UpdateData()
        {
            // Если нет связи — не долбим порт
            if (!_connected) return;

            Write2Serial("2gcp");
            Write2Serial("2gsf");
            Write2Serial("2gss");
            Write2Serial("2ges");
        }
    }
}
