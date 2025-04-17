using System;
using System.Drawing;
using System.Windows.Forms;
using RPCC.Tasks;

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
        private readonly object _bitmapLock = new();
        
        public void Update(double ccdTemp, double baseTemp, double coolerPwr, string status, 
            int remTime, string model, string sn, string filter)
        {
            if (GroupBoxCam == null || GroupBoxCam.IsDisposed)
                return;
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
            lock (_bitmapLock)
            {
               PictureBoxPreview?.Invoke((MethodInvoker)delegate
               {
                   PictureBoxPreview.Image?.Dispose();
                   PictureBoxPreview.Image = (Bitmap)preview.Clone();
               }); 
            }
        }

        public void UpdateProgressBar(DateTime startTime)
        {
            if (ExposureProgressBar is null)
                return;
            if (!(Head.IsObserve | Head.IsDoDarks | Head.IsDoFlats) | CameraControl.loadedTask is null)
            {
                ExposureProgressBar.Value = 0;
                return;
            }
            var elapsed = (DateTime.UtcNow - startTime).TotalSeconds;
            if (elapsed < 0)
            {
                ExposureProgressBar.Value = 0;
                return;
            }
            var percent = Math.Min(100, (int)(elapsed / CameraControl.loadedTask.Exp * 100));
            ExposureProgressBar.Value = Math.Max(0, percent);
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
