using System;
using System.Collections;
using System.IO.Ports;
using System.Timers;
using RPCC.Utils;

namespace RPCC.Focus

{
    public class SerialFocus
    {
        private static readonly Timer ComTimer = new Timer(); //timer for Serial port communication delay
        private readonly SerialPort _serialPort = new SerialPort();
        private readonly Logger _logger;
        private readonly Settings _settings;

        internal SerialFocus(Logger logger, Settings settings)
        {
            _logger = logger;
            _settings = settings;
            Switches = new BitArray(8);
        }

        public int CurrentPosition { get; private set; }

        public int SpeedFast { get; private set; }

        public int SpeedSlow { get; private set; }

        public BitArray Switches { get; private set; }

        public bool TransmissionEnabled { get; private set; }

        //timer for waiting of reply from mc
        private void OnTimedEvent_Com(object sender, ElapsedEventArgs e)
        {
            ComTimer.Stop();
            TransmissionEnabled = true;
        }

        //serial port reader
        private void SerialPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            ComTimer.Stop(); //stop timer

            var indata = "#";
            try
            {
                indata = _serialPort.ReadLine(); //read answer [1ap=1234]
                // _logger.AddLogEntry("get msg "+indata);
            }
            catch
            {
                // ignored
            }

            var reply = "___";
            byte value;
            var bits = new BitArray(8);

            try
            {
                reply = indata.Substring(1, 3);
                var dig = 0;
                if (reply == "aes")
                {
                    value = Convert.ToByte(indata.Substring(5));
                    var gar = BitConverter.GetBytes(value);
                    bits = new BitArray(new[] {gar[0]});
                    bool buf;
                    for (var i = 0; i < bits.Count / 2; i++) // HACK Reverse order of bits variable
                    {
                        buf = bits[i];
                        bits[i] = bits[bits.Count - i - 1];
                        bits[bits.Count - i - 1] = buf;
                    }
                    dig = int.Parse(indata.Substring(5, 3));
                }
                else
                {
                    dig = int.Parse(indata.Substring(5));
                }

                // var msg = "";
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
            catch (Exception exception)
            {
                _logger.AddLogEntry("SERIAL FOCUS: Can't read the focus answer " + indata);
                _logger.AddLogEntry(exception.Message);
            }

            try
            {
                indata = _serialPort.ReadExisting(); //cleaning
            }
            catch
            {
                // ignored
            }

            TransmissionEnabled = true; //enable transmission
        }

        public void Set_Speed_Fast(int steps)
        {
            var s = "2ssf=" + steps;
            Write2Serial(s);
        }

        public void Set_Speed_Slow(int steps)
        {
            var s = "2sss=" + steps;
            Write2Serial(s);
        }

        public void Set_Zero()
        {
            var s = "2szp";
            Write2Serial(s);
        }

        public void FRun_To(int steps)
        {
            var s = "2rfa=" + steps;
            Write2Serial(s);
        }

        public void SRun_To(int steps)
        {
            var s = "2rsl=" + steps;
            Write2Serial(s);
        }

        public void Stop()
        {
            _logger.AddLogEntry("SERIAL FOCUS: stop");
            Write2Serial("2rst");
        }

        private void Open_Port()
        {
            _serialPort.PortName = "COM" + _settings.FocusComId.ToString();
            _serialPort.BaudRate = 9600;
            _serialPort.DataBits = 8;
            try
            {
                _serialPort.Open();
                if (!_serialPort.IsOpen) return;
                _serialPort.ReadTimeout = 500;
                _serialPort.NewLine = "\0";
                _serialPort.ReceivedBytesThreshold = 6;
                _serialPort.DiscardInBuffer(); // чистить порт после открытия
                // _logger.AddLogEntry("SerialPort Focus opened");
                TransmissionEnabled = true;
            }
            catch (Exception ex)
            {
                _logger.AddLogEntry("SERIAL FOCUS: opening fails");
                _logger.AddLogEntry(ex.ToString());
            }
        }

        public void Close_Port()
        {
            try
            {
                ComTimer.Stop();
                TransmissionEnabled = false;
                _serialPort.Close();
                _logger.AddLogEntry("SERIAL FOCUS: closed");
            }
            catch (Exception ex)
            {
                _logger.AddLogEntry("SERIAL FOCUS: closing fails");
                _logger.AddLogEntry(ex.ToString());
            }
        }

        public bool Init()
        {
            ComTimer.Elapsed += OnTimedEvent_Com;
            ComTimer.Interval = 1000; // ожидание ответа микроконтроллера 1000мс
            _serialPort.DataReceived += SerialPort_DataReceived;
            // ComTimer.Start();
            Open_Port();
            return TransmissionEnabled;
        }

        //send command without answer
        public void Write2Serial(string command)
        {
            if (!_serialPort.IsOpen || !TransmissionEnabled) return;
            // _logger.AddLogEntry("send msg: " + command);
            if (command[1] == 'r' || command[1] == 's') //if run command
            {
                TransmissionEnabled = false;
                _serialPort.WriteLine(command);
                TransmissionEnabled = true;
            }

            if (command[1] == 'g') //if question
            {
                _serialPort.DiscardInBuffer(); //clear input buffer
                _serialPort.WriteLine(command); //send question
                TransmissionEnabled = false; //disable transmission of next command
                ComTimer.Start(); //start 1000 ms timer for waiting
            }
        }

        public void UpdateData()
        {
            Write2Serial("2gcp");
            Write2Serial("2gsf");
            Write2Serial("2gss");
            Write2Serial("2ges");
        }
    }
}