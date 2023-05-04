using System;
using System.Threading;
using System.Timers;
using System.Windows.Forms;
using Timer = System.Timers.Timer;

namespace RPCC.Focus
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
            labelFocusPos.Dispose();
            FocusTimer.Dispose();
        }

        private void OnTimedEvent_Clock(object sender, ElapsedEventArgs e)
        {
            var getFocus = new Thread(GetData);
            getFocus.Start();
        }

        private void GetData()
        {
            const int waitTime = 500;
            _cameraFocus.SerialFocus.UpdateData();
            Thread.Sleep(waitTime);
            labelFocusPos.Invoke((MethodInvoker) delegate
            {
                labelFocusPos.Text = $@"Focus position: {_cameraFocus.SerialFocus.CurrentPosition}";
            });

            labelEndSwitch.Text = @"Endswitch: " + (_cameraFocus.SerialFocus.Switches[0] ? "joint" : "unjoint");
        }

        private void checkBox_AutoFocus_CheckedChanged(object sender, EventArgs e)
        {
            var isAutoFocusEnabled = checkBoxAutoFocus.Checked;

            //Settings
            buttonRun.Enabled = !isAutoFocusEnabled;
            buttonSetZeroPos.Enabled = !isAutoFocusEnabled;
            numericUpDownRun.Enabled = !isAutoFocusEnabled;

            //AutoFocus
            numericUpDownSetDefoc.Enabled = isAutoFocusEnabled;
            checkBoxGoZenith.Enabled = isAutoFocusEnabled;
            _cameraFocus.isAutoFocus = isAutoFocusEnabled;
        }

        private void checkBox_goZenith_CheckedChanged(object sender, EventArgs e)
        {
            _cameraFocus.IsZenith = checkBoxGoZenith.Checked;
        }

        private void numericUpDown_setDefoc_ValueChanged(object sender, EventArgs e)
        {
            _cameraFocus.DeFocus = (int)numericUpDownSetDefoc.Value;
        }

        private void buttonSetZeroPos_Click(object sender, EventArgs e)
        {
            _cameraFocus.SerialFocus.Set_Zero();
        }

        private void buttonRunStop_Click(object sender, EventArgs e)
        {
            _cameraFocus.SerialFocus.Stop();
        }

        private void buttonRun_Click(object sender, EventArgs e)
        {
            if (radioButtonRunFast.Checked) _cameraFocus.SerialFocus.FRun_To((int)numericUpDownRun.Value);
            else if (radioButtonRunSlow.Checked) _cameraFocus.SerialFocus.SRun_To((int)numericUpDownRun.Value);
        }
    }
}