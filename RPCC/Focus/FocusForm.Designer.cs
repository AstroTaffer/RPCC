using System.ComponentModel;

namespace RPCC.Focus
{
    partial class FocusForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }

            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FocusForm));
            this.numericUpDownRun = new System.Windows.Forms.NumericUpDown();
            this.labelFocusPos = new System.Windows.Forms.Label();
            this.timer1 = new System.Windows.Forms.Timer(this.components);
            this.checkBoxAutoFocus = new System.Windows.Forms.CheckBox();
            this.labelEndSwitch = new System.Windows.Forms.Label();
            this.numericUpDownSetDefoc = new System.Windows.Forms.NumericUpDown();
            this.labelSetDefocus = new System.Windows.Forms.Label();
            this.groupBoxAutoFocus = new System.Windows.Forms.GroupBox();
            this.checkBoxGoZenith = new System.Windows.Forms.CheckBox();
            this.groupBoxFocusSettings = new System.Windows.Forms.GroupBox();
            this.buttonSetZeroPos = new System.Windows.Forms.Button();
            this.radioButtonRunSlow = new System.Windows.Forms.RadioButton();
            this.radioButtonRunFast = new System.Windows.Forms.RadioButton();
            this.buttonRun = new System.Windows.Forms.Button();
            this.buttonRunStop = new System.Windows.Forms.Button();
            this.groupBoxInfo = new System.Windows.Forms.GroupBox();
            ((System.ComponentModel.ISupportInitialize) (this.numericUpDownRun)).BeginInit();
            ((System.ComponentModel.ISupportInitialize) (this.numericUpDownSetDefoc)).BeginInit();
            this.groupBoxAutoFocus.SuspendLayout();
            this.groupBoxFocusSettings.SuspendLayout();
            this.groupBoxInfo.SuspendLayout();
            this.SuspendLayout();
            // 
            // numericUpDownRun
            // 
            this.numericUpDownRun.Enabled = false;
            this.numericUpDownRun.Increment = new decimal(new int[] {100, 0, 0, 0});
            this.numericUpDownRun.Location = new System.Drawing.Point(138, 25);
            this.numericUpDownRun.Maximum = new decimal(new int[] {5000, 0, 0, 0});
            this.numericUpDownRun.Minimum = new decimal(new int[] {5000, 0, 0, -2147483648});
            this.numericUpDownRun.Name = "numericUpDownRun";
            this.numericUpDownRun.Size = new System.Drawing.Size(75, 26);
            this.numericUpDownRun.TabIndex = 1;
            // 
            // labelFocusPos
            // 
            this.labelFocusPos.AutoSize = true;
            this.labelFocusPos.Location = new System.Drawing.Point(6, 22);
            this.labelFocusPos.Name = "labelFocusPos";
            this.labelFocusPos.Size = new System.Drawing.Size(116, 20);
            this.labelFocusPos.TabIndex = 4;
            this.labelFocusPos.Text = "Focus position:";
            // 
            // checkBoxAutoFocus
            // 
            this.checkBoxAutoFocus.AutoSize = true;
            this.checkBoxAutoFocus.Checked = true;
            this.checkBoxAutoFocus.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBoxAutoFocus.Location = new System.Drawing.Point(12, 57);
            this.checkBoxAutoFocus.Name = "checkBoxAutoFocus";
            this.checkBoxAutoFocus.Size = new System.Drawing.Size(105, 24);
            this.checkBoxAutoFocus.TabIndex = 12;
            this.checkBoxAutoFocus.Text = "Auto focus";
            this.checkBoxAutoFocus.UseVisualStyleBackColor = true;
            this.checkBoxAutoFocus.CheckedChanged += new System.EventHandler(this.checkBox_AutoFocus_CheckedChanged);
            // 
            // labelEndSwitch
            // 
            this.labelEndSwitch.AutoSize = true;
            this.labelEndSwitch.Location = new System.Drawing.Point(181, 22);
            this.labelEndSwitch.Name = "labelEndSwitch";
            this.labelEndSwitch.Size = new System.Drawing.Size(137, 20);
            this.labelEndSwitch.TabIndex = 13;
            this.labelEndSwitch.Text = "Endswitch: unjoint";
            // 
            // numericUpDownSetDefoc
            // 
            this.numericUpDownSetDefoc.Location = new System.Drawing.Point(242, 25);
            this.numericUpDownSetDefoc.Maximum = new decimal(new int[] {1500, 0, 0, 0});
            this.numericUpDownSetDefoc.Minimum = new decimal(new int[] {1500, 0, 0, -2147483648});
            this.numericUpDownSetDefoc.Name = "numericUpDownSetDefoc";
            this.numericUpDownSetDefoc.Size = new System.Drawing.Size(76, 26);
            this.numericUpDownSetDefoc.TabIndex = 15;
            this.numericUpDownSetDefoc.ValueChanged += new System.EventHandler(this.numericUpDown_setDefoc_ValueChanged);
            // 
            // labelSetDefocus
            // 
            this.labelSetDefocus.AutoSize = true;
            this.labelSetDefocus.Location = new System.Drawing.Point(6, 22);
            this.labelSetDefocus.Name = "labelSetDefocus";
            this.labelSetDefocus.Size = new System.Drawing.Size(148, 20);
            this.labelSetDefocus.TabIndex = 14;
            this.labelSetDefocus.Text = "Set defocus (steps)";
            // 
            // groupBoxAutoFocus
            // 
            this.groupBoxAutoFocus.Controls.Add(this.checkBoxGoZenith);
            this.groupBoxAutoFocus.Controls.Add(this.numericUpDownSetDefoc);
            this.groupBoxAutoFocus.Controls.Add(this.labelSetDefocus);
            this.groupBoxAutoFocus.Controls.Add(this.checkBoxAutoFocus);
            this.groupBoxAutoFocus.Location = new System.Drawing.Point(12, 120);
            this.groupBoxAutoFocus.Name = "groupBoxAutoFocus";
            this.groupBoxAutoFocus.Size = new System.Drawing.Size(324, 87);
            this.groupBoxAutoFocus.TabIndex = 16;
            this.groupBoxAutoFocus.TabStop = false;
            this.groupBoxAutoFocus.Text = "Auto Focus";
            // 
            // checkBoxGoZenith
            // 
            this.checkBoxGoZenith.AutoSize = true;
            this.checkBoxGoZenith.Location = new System.Drawing.Point(181, 57);
            this.checkBoxGoZenith.Name = "checkBoxGoZenith";
            this.checkBoxGoZenith.Size = new System.Drawing.Size(137, 24);
            this.checkBoxGoZenith.TabIndex = 16;
            this.checkBoxGoZenith.Text = "Focus at zenith";
            this.checkBoxGoZenith.UseVisualStyleBackColor = true;
            this.checkBoxGoZenith.CheckedChanged += new System.EventHandler(this.checkBox_goZenith_CheckedChanged);
            // 
            // groupBoxFocusSettings
            // 
            this.groupBoxFocusSettings.Controls.Add(this.buttonSetZeroPos);
            this.groupBoxFocusSettings.Controls.Add(this.radioButtonRunSlow);
            this.groupBoxFocusSettings.Controls.Add(this.radioButtonRunFast);
            this.groupBoxFocusSettings.Controls.Add(this.buttonRun);
            this.groupBoxFocusSettings.Controls.Add(this.buttonRunStop);
            this.groupBoxFocusSettings.Controls.Add(this.numericUpDownRun);
            this.groupBoxFocusSettings.Location = new System.Drawing.Point(12, 12);
            this.groupBoxFocusSettings.Name = "groupBoxFocusSettings";
            this.groupBoxFocusSettings.Size = new System.Drawing.Size(324, 102);
            this.groupBoxFocusSettings.TabIndex = 17;
            this.groupBoxFocusSettings.TabStop = false;
            this.groupBoxFocusSettings.Text = "Focus Settings";
            // 
            // buttonSetZeroPos
            // 
            this.buttonSetZeroPos.Enabled = false;
            this.buttonSetZeroPos.Location = new System.Drawing.Point(6, 61);
            this.buttonSetZeroPos.Name = "buttonSetZeroPos";
            this.buttonSetZeroPos.Size = new System.Drawing.Size(207, 29);
            this.buttonSetZeroPos.TabIndex = 21;
            this.buttonSetZeroPos.Text = "Set zero position";
            this.buttonSetZeroPos.UseVisualStyleBackColor = true;
            this.buttonSetZeroPos.Click += new System.EventHandler(this.buttonSetZeroPos_Click);
            // 
            // radioButtonRunSlow
            // 
            this.radioButtonRunSlow.AutoSize = true;
            this.radioButtonRunSlow.Location = new System.Drawing.Point(71, 25);
            this.radioButtonRunSlow.Name = "radioButtonRunSlow";
            this.radioButtonRunSlow.Size = new System.Drawing.Size(61, 24);
            this.radioButtonRunSlow.TabIndex = 20;
            this.radioButtonRunSlow.Text = "Slow";
            this.radioButtonRunSlow.UseVisualStyleBackColor = true;
            // 
            // radioButtonRunFast
            // 
            this.radioButtonRunFast.AutoSize = true;
            this.radioButtonRunFast.Checked = true;
            this.radioButtonRunFast.Location = new System.Drawing.Point(6, 25);
            this.radioButtonRunFast.Name = "radioButtonRunFast";
            this.radioButtonRunFast.Size = new System.Drawing.Size(59, 24);
            this.radioButtonRunFast.TabIndex = 19;
            this.radioButtonRunFast.TabStop = true;
            this.radioButtonRunFast.Text = "Fast";
            this.radioButtonRunFast.UseVisualStyleBackColor = true;
            // 
            // buttonRun
            // 
            this.buttonRun.Anchor = ((System.Windows.Forms.AnchorStyles) ((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonRun.Enabled = false;
            this.buttonRun.Location = new System.Drawing.Point(244, 25);
            this.buttonRun.Name = "buttonRun";
            this.buttonRun.Size = new System.Drawing.Size(74, 29);
            this.buttonRun.TabIndex = 18;
            this.buttonRun.Text = "Run";
            this.buttonRun.UseVisualStyleBackColor = true;
            this.buttonRun.Click += new System.EventHandler(this.buttonRun_Click);
            // 
            // buttonRunStop
            // 
            this.buttonRunStop.Anchor = ((System.Windows.Forms.AnchorStyles) ((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonRunStop.Location = new System.Drawing.Point(244, 60);
            this.buttonRunStop.Name = "buttonRunStop";
            this.buttonRunStop.Size = new System.Drawing.Size(74, 29);
            this.buttonRunStop.TabIndex = 17;
            this.buttonRunStop.Text = "Stop";
            this.buttonRunStop.UseVisualStyleBackColor = true;
            this.buttonRunStop.Click += new System.EventHandler(this.buttonRunStop_Click);
            // 
            // groupBoxInfo
            // 
            this.groupBoxInfo.Controls.Add(this.labelFocusPos);
            this.groupBoxInfo.Controls.Add(this.labelEndSwitch);
            this.groupBoxInfo.Location = new System.Drawing.Point(13, 213);
            this.groupBoxInfo.Name = "groupBoxInfo";
            this.groupBoxInfo.Size = new System.Drawing.Size(324, 51);
            this.groupBoxInfo.TabIndex = 18;
            this.groupBoxInfo.TabStop = false;
            this.groupBoxInfo.Text = "Info";
            // 
            // FocusForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(349, 271);
            this.Controls.Add(this.groupBoxInfo);
            this.Controls.Add(this.groupBoxFocusSettings);
            this.Controls.Add(this.groupBoxAutoFocus);
            this.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte) (204)));
            this.Icon = ((System.Drawing.Icon) (resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.Name = "FocusForm";
            this.Text = "Focus";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.FocusForm_FormClosing);
            ((System.ComponentModel.ISupportInitialize) (this.numericUpDownRun)).EndInit();
            ((System.ComponentModel.ISupportInitialize) (this.numericUpDownSetDefoc)).EndInit();
            this.groupBoxAutoFocus.ResumeLayout(false);
            this.groupBoxAutoFocus.PerformLayout();
            this.groupBoxFocusSettings.ResumeLayout(false);
            this.groupBoxFocusSettings.PerformLayout();
            this.groupBoxInfo.ResumeLayout(false);
            this.groupBoxInfo.PerformLayout();
            this.ResumeLayout(false);
        }

        private System.Windows.Forms.Button buttonSetZeroPos;
        private System.Windows.Forms.Button buttonRunStop;
        private System.Windows.Forms.Button buttonRun;

        private System.Windows.Forms.RadioButton radioButtonRunFast;
        private System.Windows.Forms.RadioButton radioButtonRunSlow;

        #endregion

        private System.Windows.Forms.NumericUpDown numericUpDownRun;
        private System.Windows.Forms.Label labelFocusPos;
        private System.Windows.Forms.Timer timer1;
        private System.Windows.Forms.CheckBox checkBoxAutoFocus;
        private System.Windows.Forms.Label labelEndSwitch;
        private System.Windows.Forms.NumericUpDown numericUpDownSetDefoc;
        private System.Windows.Forms.Label labelSetDefocus;
        private System.Windows.Forms.GroupBox groupBoxAutoFocus;
        private System.Windows.Forms.GroupBox groupBoxFocusSettings;
        private System.Windows.Forms.GroupBox groupBoxInfo;
        private System.Windows.Forms.CheckBox checkBoxGoZenith;
    }
}