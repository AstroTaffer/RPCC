using System;
using System.Threading;
using System.Timers;
using System.Windows.Forms;
using Timer = System.Timers.Timer;

namespace RPCC
{
    public partial class FocusForm : Form
    {
        private static readonly Timer FocusTimer = new Timer();
        private readonly SerialFocus _serialFocus = new SerialFocus();

        private int _counter;

        // private CameraFocus _focus = new CameraFocus();
        private int _fPos;

        public FocusForm()
        {
            InitializeComponent();
            //create timer for focus loop
            FocusTimer.Elapsed += OnTimedEvent_Clock;
            FocusTimer.Interval = 5000;
            FocusTimer.Start();
        }

        private void OnTimedEvent_Clock(object sender, ElapsedEventArgs e)
        {
            var getFocus = new Thread(GetData);
            getFocus.Start();
        }

        private void Focus_Form_FormClosing(object sender, FormClosingEventArgs e)
        {
            _serialFocus.Close_Port();
        }

        private void Run_slow_numericUpDown_ValueChanged(object sender, EventArgs e)
        {
            _serialFocus.SRun_To((int) Run_slow_numericUpDown.Value);
        }

        private void Run_fast_numericUpDown_ValueChanged(object sender, EventArgs e)
        {
            _serialFocus.FRun_To((int) Run_fast_numericUpDown.Value);
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            _counter++;
            if (_counter < 15) return;
            _counter = 0;
            SetFocus();
        }

        private void SetFocus()
        {
            _fPos = _serialFocus.currentPosition;

            void Action()
            {
                Focus_pos_label.Text = @"Focus position: " + _fPos;
            }

            if (InvokeRequired)
                Invoke((Action) Action);
            else
                Action();

            if (checkBox_AutoFocus.Checked)
            {
                // TODO AUTOFOCUS
            }
        }

        private void GetData()
        {
            const int waitTime = 1000;
            _serialFocus.Write2Serial("2gcp");
            _serialFocus.Write2Serial("2gsf");
            _serialFocus.Write2Serial("2gss");
            _serialFocus.Write2Serial("2ges");
            Thread.Sleep(waitTime);
        }

        private void button_stop_Click(object sender, EventArgs e)
        {
            _serialFocus.Stop();
        }

        private void numericUpDown_setF_ValueChanged(object sender, EventArgs e)
        {
            _serialFocus.Set_Speed_Fast((int) numericUpDown_setF.Value);
        }

        private void numericUpDown_setS_ValueChanged(object sender, EventArgs e)
        {
            _serialFocus.Set_Speed_Slow((int) numericUpDown_setS.Value);
        }

        private void numericUpDown_setZero_ValueChanged(object sender, EventArgs e)
        {
            _serialFocus.Set_Zero((int) numericUpDown_setZero.Value);
        }

        private void checkBox_AutoFocus_CheckedChanged(object sender, EventArgs e)
        {
            var isEnabled = !checkBox_AutoFocus.Checked;
            numericUpDown_setF.Enabled = isEnabled;
            numericUpDown_setS.Enabled = isEnabled;
            numericUpDown_setZero.Enabled = isEnabled;
            Run_fast_numericUpDown.Enabled = isEnabled;
            Run_slow_numericUpDown.Enabled = isEnabled;
            button_stop.Enabled = isEnabled;
        }
    }
}