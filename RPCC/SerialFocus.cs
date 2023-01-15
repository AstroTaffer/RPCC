using System;
using System.Collections;
using System.IO.Ports;
using System.Linq;
using System.Timers;

namespace RPCC

{
    public class SerialFocus
    {
        private static readonly Timer ComTimer = new Timer(); //timer for Serial port communication delay

        private readonly SerialPort _serialPort = new SerialPort();

        //serial port
        public string comId;
        public int currentPosition;
        public int speedFast;
        public int speedSlow;
        public BitArray switches;
        public bool transmissionEnabled;

        //timer for waiting of reply from mc
        private void OnTimedEvent_Com(object sender, ElapsedEventArgs e)
        {
            ComTimer.Stop();
            transmissionEnabled = true;
        }

        //serial port reader
        private void SerialPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            ComTimer.Stop(); //stop timer

            var indata = "#";
            try
            {
                indata = _serialPort.ReadLine(); //read answer [1ap=1234]
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
                value = Convert.ToByte(indata.Substring(indata.IndexOf('=') + 1));
                bits = new BitArray(BitConverter.GetBytes(value).ToArray());
                var dig = int.Parse(indata.Substring(5, 3));
                // var msg = "";
                switch (reply)
                {
                    case "acp":
                        currentPosition = dig;
                        break;
                    case "asf":
                        speedFast = dig;
                        break;
                    case "ass":
                        speedSlow = dig;
                        break;
                    case "aes":
                        switches = bits;
                        break;
                }
            }
            catch (Exception exception)
            {
                // Logger.AddLogEntry("Can't read the dome answer " + indata);
                // Logger.AddLogEntry(exception.Message);
            }

            try
            {
                indata = _serialPort.ReadExisting(); //cleaning
            }
            catch
            {
                // ignored
            }

            transmissionEnabled = true; //enable transmission
        }

        public void Set_Speed_Fast(int steps)
        {
            // steps = To_Limit(steps);
            var s = "2ssf=" + steps;
            Write2Serial(s);
        }

        public void Set_Speed_Slow(int steps)
        {
            // steps = To_Limit(steps);
            var s = "2szp=" + steps;
            Write2Serial(s);
        }

        public void Set_Zero(int point)
        {
            var s = "2sss=" + point;
            Write2Serial(s);
        }

        public void FRun_To(int steps)
        {
            steps = To_Limit(steps);
            var s = "2rfa=" + steps;
            Write2Serial(s);
        }

        public void SRun_To(int steps)
        {
            steps = To_Limit(steps);
            var s = "2rsl=" + steps;
            Write2Serial(s);
        }

        public void Stop()
        {
            Write2Serial("2rst;");
        }

        private static int To_Limit(int steps)
        {
            if (steps > 234)
                steps = 234;
            else if (steps < -234) steps = -234;

            return steps;
        }

        private void Open_Port()
        {
            _serialPort.PortName = "COM" + comId;
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
                // Logger.AddLogEntry("SerialPort opened");
                transmissionEnabled = true;
            }
            catch (Exception ex)
            {
                // Logger.AddLogEntry("SerialPort opening fails");
                // Logger.AddLogEntry(ex.ToString());
            }
        }

        public void Close_Port()
        {
            try
            {
                ComTimer.Stop();
                transmissionEnabled = false;
                _serialPort.Close();
                // Logger.AddLogEntry("SerialPort closed");
            }
            catch (Exception ex)
            {
                // Logger.AddLogEntry("SerialPort closing fails");
                // Logger.AddLogEntry(ex.ToString());
            }
        }

        public bool Init()
        {
            ComTimer.Elapsed += OnTimedEvent_Com;
            ComTimer.Interval = 1000; // ожидание ответа микроконтроллера 1000мс
            _serialPort.DataReceived += SerialPort_DataReceived;
            // ComTimer.Start();
            Open_Port();
            return transmissionEnabled;
        }

        //send command without answer
        public void Write2Serial(string command)
        {
            if (!_serialPort.IsOpen || !transmissionEnabled) return;
            // Logger.AddLogEntry("send msg: " + command);
            if (command[1] == 'r' || command[1] == 's') //if run command
            {
                transmissionEnabled = false;
                _serialPort.WriteLine(command);
                transmissionEnabled = true;
            }

            if (command[1] == 'g') //if question
            {
                _serialPort.DiscardInBuffer(); //clear input buffer
                _serialPort.WriteLine(command); //send question
                transmissionEnabled = false; //disable transmission of next command
                ComTimer.Start(); //start 1000 ms timer for waiting
            }
        }
    }
}