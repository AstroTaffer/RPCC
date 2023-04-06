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
        private readonly CameraFocus _cameraFocus;

        public FocusForm(CameraFocus cameraFocus)
        {
            InitializeComponent();
            //create timer for focus loop
            FocusTimer.Elapsed += OnTimedEvent_Clock;
            FocusTimer.Interval = 5000;
            FocusTimer.Start();
            _cameraFocus = cameraFocus;
        }
    
        private void FocusForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            _cameraFocus.DeFocus = 0;
            _cameraFocus.IsZenith = false;
        }

        private void OnTimedEvent_Clock(object sender, ElapsedEventArgs e)
        {
            var getFocus = new Thread(GetData);
            getFocus.Start();
        }

        private void Run_slow_numericUpDown_ValueChanged(object sender, EventArgs e)
        {
            _cameraFocus.SerialFocus.SRun_To((int) Run_slow_numericUpDown.Value);
        }

        private void Run_fast_numericUpDown_ValueChanged(object sender, EventArgs e)
        {
            _cameraFocus.SerialFocus.FRun_To((int) Run_fast_numericUpDown.Value);
        }

        private void GetData()
        {
            const int waitTime = 500;
            _cameraFocus.SerialFocus.UpdateData();
            Thread.Sleep(waitTime);
            numericUpDown_setF.Invoke((MethodInvoker) delegate
            {
                numericUpDown_setF.Value = _cameraFocus.SerialFocus.SpeedFast;
            });
            numericUpDown_setS.Invoke((MethodInvoker) delegate
            {
                numericUpDown_setS.Value = _cameraFocus.SerialFocus.SpeedSlow;
            });
            Focus_pos_label.Invoke((MethodInvoker) delegate
            {
                Focus_pos_label.Text = $@"Focus position: {_cameraFocus.SerialFocus.CurrentPosition}";
            });
            // endswitch.Text = "Endswitch: " + _cameraFocus.SerialFocus.Switches? "" : ""; TODO ENDSWITCH
        }

        private void button_stop_Click(object sender, EventArgs e)
        {
            _cameraFocus.SerialFocus.Stop();
        }

        private void numericUpDown_setF_ValueChanged(object sender, EventArgs e)
        {
            _cameraFocus.SerialFocus.Set_Speed_Fast((int) numericUpDown_setF.Value);
        }

        private void numericUpDown_setS_ValueChanged(object sender, EventArgs e)
        {
            _cameraFocus.SerialFocus.Set_Speed_Slow((int) numericUpDown_setS.Value);
        }

        private void numericUpDown_setZero_ValueChanged(object sender, EventArgs e)
        {
            _cameraFocus.SerialFocus.Set_Zero((int) numericUpDown_setZero.Value);
        }

        private void checkBox_AutoFocus_CheckedChanged(object sender, EventArgs e)
        {
            var isEnabled = checkBox_AutoFocus.Checked;

            //Settings
            numericUpDown_setF.Enabled = !isEnabled;
            numericUpDown_setS.Enabled = !isEnabled;
            numericUpDown_setZero.Enabled = !isEnabled;
            Run_fast_numericUpDown.Enabled = !isEnabled;
            Run_slow_numericUpDown.Enabled = !isEnabled;

            //AutoFocus
            button_stop.Enabled = isEnabled;
            numericUpDown_setDefoc.Enabled = isEnabled;
            checkBox_goZenith.Enabled = isEnabled;
            _cameraFocus.isAutoFocus = isEnabled;
        }

        private void checkBox_goZenith_CheckedChanged(object sender, EventArgs e)
        {
            _cameraFocus.IsZenith = checkBox_goZenith.Checked;
        }

        private void numericUpDown_setDefoc_ValueChanged(object sender, EventArgs e)
        {
            _cameraFocus.DeFocus = (int)numericUpDown_setDefoc.Value;
        }
    }
}