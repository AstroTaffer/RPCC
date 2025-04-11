using System;
using System.Drawing;
using System.Windows.Forms;

namespace RPCC.Cams
{
    public class CameraUiBlock
    {
        public Label LabelCcdTemp;
        public Label LabelBaseTemp;
        public Label LabelCoolerPwr;
        public Label LabelStatus;
        public Label LabelRemTime;
        public GroupBox GroupBoxCam;

        public PictureBox PictureBoxPreview;
        public ProgressBar ExposureProgressBar;
        public Label LabelFilter;
        public Label LabelModel;
        public Label LabelSerial;

        public void Update(double ccdTemp, double baseTemp, double coolerPwr, string status, 
            int remTime, string model, string sn, string filter)
        {
            GroupBoxCam.Enabled = true;

            if (LabelSerial != null) LabelSerial.Text = @$"Serial Num: {sn}";
            if (LabelModel != null) LabelModel.Text = @$"Model: {model}";
            if (LabelFilter != null) LabelFilter.Text = @$"Filter: {filter}";
            
            if (LabelCcdTemp != null) LabelCcdTemp.Text = @$"CCD Temp: {ccdTemp:F3}";
            if (LabelBaseTemp != null) LabelBaseTemp.Text = @$"Base Temp: {baseTemp:F3}";
            if (LabelCoolerPwr != null) LabelCoolerPwr.Text = @$"Cooler Power: {coolerPwr} %";
            if (LabelStatus != null) LabelStatus.Text = @$"Status: {status}";
            if (LabelRemTime != null) LabelRemTime.Text = @$"Remaining: {remTime}";
        }

        public void UpdatePreview(Bitmap preview)
        {
            if (PictureBoxPreview != null)
            {
                PictureBoxPreview.Image?.Dispose();
                PictureBoxPreview.Image = (Bitmap)preview.Clone();
            }
        }

        public void UpdateProgressBar(DateTime startTime, int expDurationSec)
        {
            if (ExposureProgressBar == null)
                return;

            var elapsed = (DateTime.UtcNow - startTime).TotalSeconds;
            if (elapsed < 0 || expDurationSec <= 0)
            {
                ExposureProgressBar.Visible = false;
                return;
            }

            var percent = Math.Min(100, (int)(elapsed / expDurationSec * 100));
            ExposureProgressBar.Value = Math.Max(0, Math.Min(percent, 100));
            ExposureProgressBar.Visible = true;
        }

        public void Clear()
        {
            GroupBoxCam.Enabled = false;

            LabelCcdTemp?.ResetText();
            LabelBaseTemp?.ResetText();
            LabelCoolerPwr?.ResetText();
            LabelStatus?.ResetText();
            LabelRemTime?.ResetText();
            LabelFilter?.ResetText();
            LabelModel?.ResetText();
            LabelSerial?.ResetText();

            if (PictureBoxPreview != null)
            {
                PictureBoxPreview.Image?.Dispose();
                PictureBoxPreview.Image = null;
            }

            if (ExposureProgressBar != null)
            {
                ExposureProgressBar.Value = 0;
                ExposureProgressBar.Visible = false;
            }
        }
    }
}
