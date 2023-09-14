using System;
using System.Collections;
using System.IO.Ports;
using System.Timers;
using RPCC.Utils;

namespace RPCC.Focus

{
    public static class SerialFocus
    {
        private static readonly Timer ComTimer = new Timer(); //timer for Serial port communication delay
        private static readonly SerialPort SerialPort = new SerialPort();

        static SerialFocus()
        {
            Switches = new BitArray(8);
        }

        public static int CurrentPosition { get; private set; }
        public static int SpeedFast { get; private set; }
        public static int SpeedSlow { get; private set; }
        public static BitArray Switches { get; private set; }
        public static bool TransmissionEnabled { get; private set; }
        //timer for waiting of reply from mc
        private static void OnTimedEvent_Com(object sender, ElapsedEventArgs e)
        {
            ComTimer.Stop();
            TransmissionEnabled = true;
        }

        //serial port reader
        private static void SerialPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            ComTimer.Stop(); //stop timer

            var indata = "#";
            try
            {
                indata = SerialPort.ReadLine(); //read answer [1ap=1234]
                // Logger.AddLogEntry("get msg "+indata);
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
                Logger.AddLogEntry("SERIAL FOCUS: Can't read the focus answer " + indata);
                Logger.AddLogEntry(exception.Message);
            }

            try
            {
                indata = SerialPort.ReadExisting(); //cleaning
            }
            catch
            {
                // ignored
            }

            TransmissionEnabled = true; //enable transmission
        }

        public static void Set_Speed_Fast(int steps)
        {
            var s = "2ssf=" + steps;
            Write2Serial(s);
        }

        public static void Set_Speed_Slow(int steps)
        {
            var s = "2sss=" + steps;
            Write2Serial(s);
        }

        public static void Set_Zero()
        {
            var s = "2szp";
            Write2Serial(s);
        }

        public static void FRun_To(int steps)
        {
            var s = "2rfa=" + steps;
            Write2Serial(s);
        }

        public static void SRun_To(int steps)
        {
            var s = "2rsl=" + steps;
            Write2Serial(s);
        }

        public static void Stop()
        {
            Logger.AddLogEntry("SERIAL FOCUS: stop");
            Write2Serial("2rst");
        }

        private static void Open_Port()
        {
            SerialPort.PortName = "COM" + Settings.FocusComId;
            SerialPort.BaudRate = 9600;
            SerialPort.DataBits = 8;
            try
            {
                SerialPort.Open();
                if (!SerialPort.IsOpen) return;
                SerialPort.ReadTimeout = 500;
                SerialPort.NewLine = "\0";
                SerialPort.ReceivedBytesThreshold = 6;
                SerialPort.DiscardInBuffer(); // чистить порт после открытия
                // Logger.AddLogEntry("SerialPort Focus opened");
                TransmissionEnabled = true;
            }
            catch (Exception ex)
            {
                Logger.AddLogEntry("SERIAL FOCUS: opening fails");
                Logger.AddLogEntry(ex.ToString());
            }
        }

        public static void Close_Port()
        {
            try
            {
                ComTimer.Stop();
                TransmissionEnabled = false;
                SerialPort.Close();
                Logger.AddLogEntry("SERIAL FOCUS: closed");
            }
            catch (Exception ex)
            {
                Logger.AddLogEntry("SERIAL FOCUS: closing fails");
                Logger.AddLogEntry(ex.ToString());
            }
        }

        public static bool Init()
        {
            ComTimer.Elapsed += OnTimedEvent_Com;
            ComTimer.Interval = 1000; // ожидание ответа микроконтроллера 1000мс
            SerialPort.DataReceived += SerialPort_DataReceived;
            // ComTimer.Start();
            Open_Port();
            return TransmissionEnabled;
        }

        //send command without answer
        public static void Write2Serial(string command)
        {
            if (!SerialPort.IsOpen || !TransmissionEnabled) return;
            // Logger.AddLogEntry("send msg: " + command);
            if (command[1] == 'r' || command[1] == 's') //if run command
            {
                TransmissionEnabled = false;
                SerialPort.WriteLine(command);
                TransmissionEnabled = true;
            }

            if (command[1] == 'g') //if question
            {
                SerialPort.DiscardInBuffer(); //clear input buffer
                SerialPort.WriteLine(command); //send question
                TransmissionEnabled = false; //disable transmission of next command
                ComTimer.Start(); //start 1000 ms timer for waiting
            }
        }

        public static void UpdateData()
        {
            Write2Serial("2gcp");
            Write2Serial("2gsf");
            Write2Serial("2gss");
            Write2Serial("2ges");
        }
    }
}