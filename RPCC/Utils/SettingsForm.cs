using System;
using System.Globalization;
using System.Windows.Forms;

namespace RPCC.Utils
{
    public partial class SettingsForm : Form
    {
        // private readonly Settings Settings;

        internal SettingsForm()
        {
            InitializeComponent();
            // Settings = settings;

            #region Image Analysis
            textBoxLowerBrightnessSd.Text = Settings.LowerBrightnessSd.ToString();
            textBoxUpperBrightnessSd.Text = Settings.UpperBrightnessSd.ToString();
            numericUpDownApertureRadius.Value = Settings.ApertureRadius;
            numericUpDownAnnulusInnerRadius.Value = Settings.AnnulusInnerRadius;
            numericUpDownAnnulusOuterRadius.Value = Settings.AnnulusOuterRadius;
            #endregion

            #region Cameras
            textBoxgCamSn.Text = Settings.SnCamG;
            textBoxrCamSn.Text = Settings.SnCamR;
            textBoxiCamSn.Text = Settings.SnCamI;
            numericUpDownNumFlushes.Value = Settings.NumFlushes;
            comboBoxTemp.Text = Settings.CamTemp.ToString();
            #endregion

            #region Survey
            folderBrowserDialogSetFolder.SelectedPath = Settings.MainOutFolder;
            labelMainOutFolderDisplay.Text = Settings.MainOutFolder;
            #endregion

            #region Comms
            numericUpDownFocusComId.Value = Settings.FocusComId;
            textBoxMeteoDomeTcpIpPort.Text = Settings.MeteoDomeTcpIpPort.ToString();
            textBoxDonutsTcpIpPort.Text = Settings.DonutsTcpIpPort.ToString();
            textBoxSiTechExeTcpIpPort.Text = Settings.SiTechExeTcpIpPort.ToString();
            #endregion
        }


        #region Buttons
        private void ButtonCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        //HACK: This functions handles ArgumentExceptions raised in "Set" properties itself. It should not.
        private void ButtonAccept_Click(object sender, EventArgs e)
        {
            CultureInfo ruCulture = new CultureInfo("ru-RU");
            CultureInfo usCulture = new CultureInfo ("en-US");

            #region Image Analysis
            try
            {
                Settings.LowerBrightnessSd = double.Parse(textBoxLowerBrightnessSd.Text, ruCulture);
            }
            catch (FormatException)
            {
                Settings.LowerBrightnessSd = double.Parse(textBoxLowerBrightnessSd.Text, usCulture);
            }
            try
            {
                Settings.UpperBrightnessSd = double.Parse(textBoxUpperBrightnessSd.Text, ruCulture);
            }
            catch (FormatException)
            {
                Settings.UpperBrightnessSd = double.Parse(textBoxUpperBrightnessSd.Text, usCulture);
            }
            Settings.ApertureRadius = (int)numericUpDownApertureRadius.Value;
            Settings.AnnulusInnerRadius = (int)numericUpDownAnnulusInnerRadius.Value;
            Settings.AnnulusOuterRadius = (int)numericUpDownAnnulusOuterRadius.Value;
            #endregion

            #region Cameras
            Settings.SnCamG = textBoxgCamSn.Text;
            Settings.SnCamR = textBoxrCamSn.Text;
            Settings.SnCamI = textBoxiCamSn.Text;
            Settings.NumFlushes = (int)numericUpDownNumFlushes.Value;
            try
            {
                Settings.CamTemp = double.Parse(comboBoxTemp.Text, ruCulture);
            }
            catch (FormatException)
            {
                Settings.CamTemp = double.Parse(comboBoxTemp.Text, usCulture);
            }
            #endregion

            #region Survey
            Settings.MainOutFolder = folderBrowserDialogSetFolder.SelectedPath;
            #endregion

            #region Comms
            Settings.FocusComId = (int)numericUpDownFocusComId.Value;
            Settings.MeteoDomeTcpIpPort = int.Parse(textBoxMeteoDomeTcpIpPort.Text);
            Settings.DonutsTcpIpPort = int.Parse(textBoxDonutsTcpIpPort.Text);
            Settings.SiTechExeTcpIpPort = int.Parse(textBoxSiTechExeTcpIpPort.Text);
            #endregion

            DialogResult = DialogResult.OK;
            Close();
        }

        private void ButtonSetFolder_Click(object sender, EventArgs e)
        {
            if (folderBrowserDialogSetFolder.ShowDialog() == DialogResult.OK)
            {
                labelMainOutFolderDisplay.Text = folderBrowserDialogSetFolder.SelectedPath;
            }
        }
        #endregion

    }
}