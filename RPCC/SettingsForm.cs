using System;
using System.Windows.Forms;
using RPCC.Utils;

namespace RPCC
{
    public partial class SettingsForm : Form
    {
        private readonly Settings _settings;

        internal SettingsForm(Settings settings)
        {
            InitializeComponent();
            _settings = settings;

            #region Image Analysis
            textBoxLowerBrightnessSd.Text = _settings.LowerBrightnessSd.ToString();
            textBoxUpperBrightnessSd.Text = _settings.UpperBrightnessSd.ToString();
            numericUpDownApertureRadius.Value = _settings.ApertureRadius;
            numericUpDownAnnulusInnerRadius.Value = _settings.AnnulusInnerRadius;
            numericUpDownAnnulusOuterRadius.Value = _settings.AnnulusOuterRadius;
            #endregion

            #region Cameras
            textBoxgCamSn.Text = _settings.SnCamG;
            textBoxrCamSn.Text = _settings.SnCamR;
            textBoxiCamSn.Text = _settings.SnCamI;
            numericUpDownNumFlushes.Value = _settings.NumFlushes;
            comboBoxTemp.Text = _settings.CamTemp.ToString();
            numericUpDownBin.Value = _settings.CamBin;
            // HACK: Check for correct data display and value reading
            comboBoxReadout.DataSource = _settings.camRoModes;
            try
            {
                comboBoxReadout.SelectedIndex = _settings.CamRoModeIndex;
            }
            catch (ArgumentOutOfRangeException)
            {
                comboBoxReadout.SelectedIndex = -1;
            }
            #endregion

            #region Survey
            folderBrowserDialogSetFolder.SelectedPath = _settings.OutImgsFolder;
            labelOutFolder.Text = _settings.OutImgsFolder;
            #endregion
        }


        #region Buttons
        private void ButtonCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        private void ButtonAccept_Click(object sender, EventArgs e)
        {
            #region Image Analysis
            _settings.LowerBrightnessSd = double.Parse(textBoxLowerBrightnessSd.Text);
            _settings.UpperBrightnessSd = double.Parse(textBoxUpperBrightnessSd.Text);
            _settings.ApertureRadius = (int)numericUpDownApertureRadius.Value;
            _settings.AnnulusInnerRadius = (int)numericUpDownAnnulusInnerRadius.Value;
            _settings.AnnulusOuterRadius = (int)numericUpDownAnnulusOuterRadius.Value;
            #endregion

            #region Cameras
            _settings.SnCamG = textBoxgCamSn.Text;
            _settings.SnCamR = textBoxrCamSn.Text;
            _settings.SnCamI = textBoxiCamSn.Text;
            _settings.NumFlushes = (int)numericUpDownNumFlushes.Value;
            _settings.CamTemp = double.Parse(comboBoxTemp.Text);
            _settings.CamBin = (int)numericUpDownBin.Value;
            if (comboBoxReadout.SelectedIndex > -1) _settings.CamRoModeIndex = comboBoxReadout.SelectedIndex;
            #endregion

            #region Survey
            _settings.OutImgsFolder = folderBrowserDialogSetFolder.SelectedPath;
            #endregion

            DialogResult = DialogResult.OK;
            Close();
        }

        private void ButtonSetFolder_Click(object sender, EventArgs e)
        {
            if (folderBrowserDialogSetFolder.ShowDialog() == DialogResult.OK)
            {
                labelOutFolder.Text = folderBrowserDialogSetFolder.SelectedPath;
            }
        }
        #endregion

    }
}