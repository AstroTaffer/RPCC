using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace RPCC
{
    public partial class SettingsForm : Form
    {
        private Settings _settings;

        internal SettingsForm(ref Settings settings)
        {
            InitializeComponent();
            _settings = settings;

            textBoxLowerBrightnessSd.Text = _settings.LowerBrightnessSd.ToString();
            textBoxUpperBrightnessSd.Text = _settings.UpperBrightnessSd.ToString();
            numericUpDownApertureRadius.Value = _settings.ApertureRadius;
            numericUpDownAnnulusInnerRadius.Value = _settings.AnnulusInnerRadius;
            numericUpDownAnnulusOuterRadius.Value = _settings.AnnulusOuterRadius;
        }


        #region Buttons
        private void ButtonCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        private void ButtonAccept_Click(object sender, EventArgs e)
        {
            // TODO: Instead of throwing exceptions it may be better to display error provider message box

            _settings.LowerBrightnessSd = double.Parse(textBoxLowerBrightnessSd.Text);
            _settings.UpperBrightnessSd = double.Parse(textBoxUpperBrightnessSd.Text);
            _settings.ApertureRadius = (int)numericUpDownApertureRadius.Value;
            _settings.AnnulusInnerRadius = (int)numericUpDownAnnulusInnerRadius.Value;
            _settings.AnnulusOuterRadius = (int)numericUpDownAnnulusOuterRadius.Value;

            DialogResult = DialogResult.OK;
            Close();
        }
        #endregion
    }
}