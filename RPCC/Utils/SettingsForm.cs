﻿using System;
using System.Globalization;
using System.Windows.Forms;

namespace RPCC.Utils
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
            comboBoxReadout.SelectedItem = _settings.CamRoMode;
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
            CultureInfo ruCulture = new CultureInfo("ru-RU");
            CultureInfo usCulture = new CultureInfo ("en-US");

            #region Image Analysis
            try
            {
                _settings.LowerBrightnessSd = double.Parse(textBoxLowerBrightnessSd.Text, ruCulture);
            }
            catch (FormatException)
            {
                _settings.LowerBrightnessSd = double.Parse(textBoxLowerBrightnessSd.Text, usCulture);
            }
            try
            {
                _settings.UpperBrightnessSd = double.Parse(textBoxUpperBrightnessSd.Text, ruCulture);
            }
            catch (FormatException)
            {
                _settings.UpperBrightnessSd = double.Parse(textBoxUpperBrightnessSd.Text, usCulture);
            }
            _settings.ApertureRadius = (int)numericUpDownApertureRadius.Value;
            _settings.AnnulusInnerRadius = (int)numericUpDownAnnulusInnerRadius.Value;
            _settings.AnnulusOuterRadius = (int)numericUpDownAnnulusOuterRadius.Value;
            #endregion

            #region Cameras
            _settings.SnCamG = textBoxgCamSn.Text;
            _settings.SnCamR = textBoxrCamSn.Text;
            _settings.SnCamI = textBoxiCamSn.Text;
            _settings.NumFlushes = (int)numericUpDownNumFlushes.Value;
            try
            {
                _settings.CamTemp = double.Parse(comboBoxTemp.Text, ruCulture);
            }
            catch (FormatException)
            {
                _settings.CamTemp = double.Parse(comboBoxTemp.Text, usCulture);
            }
            _settings.CamBin = (int)numericUpDownBin.Value;
            _settings.CamRoMode = (string)comboBoxReadout.SelectedItem;
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