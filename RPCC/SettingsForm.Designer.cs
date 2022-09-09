namespace RPCC
{
    partial class SettingsForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SettingsForm));
            this.tabControlSettings = new System.Windows.Forms.TabControl();
            this.tabPageImageAnalysis = new System.Windows.Forms.TabPage();
            this.numericUpDownAnnulusOuterRadius = new System.Windows.Forms.NumericUpDown();
            this.numericUpDownAnnulusInnerRadius = new System.Windows.Forms.NumericUpDown();
            this.numericUpDownApertureRadius = new System.Windows.Forms.NumericUpDown();
            this.labelAnnulusOuterRadius = new System.Windows.Forms.Label();
            this.labelAnnulusInnerRadius = new System.Windows.Forms.Label();
            this.labelApertureRadius = new System.Windows.Forms.Label();
            this.textBoxUpperBrightnessSd = new System.Windows.Forms.TextBox();
            this.labelUpperBrightnessSd = new System.Windows.Forms.Label();
            this.textBoxLowerBrightnessSd = new System.Windows.Forms.TextBox();
            this.labelLowerBrightnessSd = new System.Windows.Forms.Label();
            this.tabPagePlaceholder = new System.Windows.Forms.TabPage();
            this.buttonCancel = new System.Windows.Forms.Button();
            this.buttonAccept = new System.Windows.Forms.Button();
            this.tabControlSettings.SuspendLayout();
            this.tabPageImageAnalysis.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownAnnulusOuterRadius)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownAnnulusInnerRadius)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownApertureRadius)).BeginInit();
            this.SuspendLayout();
            // 
            // tabControlSettings
            // 
            this.tabControlSettings.Controls.Add(this.tabPageImageAnalysis);
            this.tabControlSettings.Controls.Add(this.tabPagePlaceholder);
            this.tabControlSettings.Dock = System.Windows.Forms.DockStyle.Top;
            this.tabControlSettings.Location = new System.Drawing.Point(0, 0);
            this.tabControlSettings.Name = "tabControlSettings";
            this.tabControlSettings.SelectedIndex = 0;
            this.tabControlSettings.Size = new System.Drawing.Size(300, 327);
            this.tabControlSettings.TabIndex = 0;
            // 
            // tabPageImageAnalysis
            // 
            this.tabPageImageAnalysis.BackColor = System.Drawing.SystemColors.Control;
            this.tabPageImageAnalysis.Controls.Add(this.numericUpDownAnnulusOuterRadius);
            this.tabPageImageAnalysis.Controls.Add(this.numericUpDownAnnulusInnerRadius);
            this.tabPageImageAnalysis.Controls.Add(this.numericUpDownApertureRadius);
            this.tabPageImageAnalysis.Controls.Add(this.labelAnnulusOuterRadius);
            this.tabPageImageAnalysis.Controls.Add(this.labelAnnulusInnerRadius);
            this.tabPageImageAnalysis.Controls.Add(this.labelApertureRadius);
            this.tabPageImageAnalysis.Controls.Add(this.textBoxUpperBrightnessSd);
            this.tabPageImageAnalysis.Controls.Add(this.labelUpperBrightnessSd);
            this.tabPageImageAnalysis.Controls.Add(this.textBoxLowerBrightnessSd);
            this.tabPageImageAnalysis.Controls.Add(this.labelLowerBrightnessSd);
            this.tabPageImageAnalysis.Location = new System.Drawing.Point(4, 22);
            this.tabPageImageAnalysis.Name = "tabPageImageAnalysis";
            this.tabPageImageAnalysis.Padding = new System.Windows.Forms.Padding(3);
            this.tabPageImageAnalysis.Size = new System.Drawing.Size(292, 301);
            this.tabPageImageAnalysis.TabIndex = 0;
            this.tabPageImageAnalysis.Text = "Image Analysis";
            // 
            // numericUpDownAnnulusOuterRadius
            // 
            this.numericUpDownAnnulusOuterRadius.Location = new System.Drawing.Point(235, 192);
            this.numericUpDownAnnulusOuterRadius.Maximum = new decimal(new int[] {
            2048,
            0,
            0,
            0});
            this.numericUpDownAnnulusOuterRadius.Name = "numericUpDownAnnulusOuterRadius";
            this.numericUpDownAnnulusOuterRadius.Size = new System.Drawing.Size(49, 20);
            this.numericUpDownAnnulusOuterRadius.TabIndex = 9;
            // 
            // numericUpDownAnnulusInnerRadius
            // 
            this.numericUpDownAnnulusInnerRadius.Location = new System.Drawing.Point(235, 151);
            this.numericUpDownAnnulusInnerRadius.Maximum = new decimal(new int[] {
            2048,
            0,
            0,
            0});
            this.numericUpDownAnnulusInnerRadius.Name = "numericUpDownAnnulusInnerRadius";
            this.numericUpDownAnnulusInnerRadius.Size = new System.Drawing.Size(49, 20);
            this.numericUpDownAnnulusInnerRadius.TabIndex = 8;
            // 
            // numericUpDownApertureRadius
            // 
            this.numericUpDownApertureRadius.Location = new System.Drawing.Point(235, 112);
            this.numericUpDownApertureRadius.Maximum = new decimal(new int[] {
            2048,
            0,
            0,
            0});
            this.numericUpDownApertureRadius.Name = "numericUpDownApertureRadius";
            this.numericUpDownApertureRadius.Size = new System.Drawing.Size(49, 20);
            this.numericUpDownApertureRadius.TabIndex = 7;
            // 
            // labelAnnulusOuterRadius
            // 
            this.labelAnnulusOuterRadius.AutoSize = true;
            this.labelAnnulusOuterRadius.Location = new System.Drawing.Point(6, 194);
            this.labelAnnulusOuterRadius.Name = "labelAnnulusOuterRadius";
            this.labelAnnulusOuterRadius.Size = new System.Drawing.Size(103, 13);
            this.labelAnnulusOuterRadius.TabIndex = 6;
            this.labelAnnulusOuterRadius.Text = "Annulus outer radius";
            // 
            // labelAnnulusInnerRadius
            // 
            this.labelAnnulusInnerRadius.AutoSize = true;
            this.labelAnnulusInnerRadius.Location = new System.Drawing.Point(6, 153);
            this.labelAnnulusInnerRadius.Name = "labelAnnulusInnerRadius";
            this.labelAnnulusInnerRadius.Size = new System.Drawing.Size(102, 13);
            this.labelAnnulusInnerRadius.TabIndex = 5;
            this.labelAnnulusInnerRadius.Text = "Annulus inner radius";
            // 
            // labelApertureRadius
            // 
            this.labelApertureRadius.AutoSize = true;
            this.labelApertureRadius.Location = new System.Drawing.Point(6, 114);
            this.labelApertureRadius.Name = "labelApertureRadius";
            this.labelApertureRadius.Size = new System.Drawing.Size(78, 13);
            this.labelApertureRadius.TabIndex = 4;
            this.labelApertureRadius.Text = "Aperture radius";
            // 
            // textBoxUpperBrightnessSd
            // 
            this.textBoxUpperBrightnessSd.Location = new System.Drawing.Point(184, 66);
            this.textBoxUpperBrightnessSd.Name = "textBoxUpperBrightnessSd";
            this.textBoxUpperBrightnessSd.Size = new System.Drawing.Size(100, 20);
            this.textBoxUpperBrightnessSd.TabIndex = 3;
            // 
            // labelUpperBrightnessSd
            // 
            this.labelUpperBrightnessSd.AutoSize = true;
            this.labelUpperBrightnessSd.Location = new System.Drawing.Point(6, 69);
            this.labelUpperBrightnessSd.Name = "labelUpperBrightnessSd";
            this.labelUpperBrightnessSd.Size = new System.Drawing.Size(105, 13);
            this.labelUpperBrightnessSd.TabIndex = 2;
            this.labelUpperBrightnessSd.Text = "Upper brightness SD";
            // 
            // textBoxLowerBrightnessSd
            // 
            this.textBoxLowerBrightnessSd.Location = new System.Drawing.Point(184, 21);
            this.textBoxLowerBrightnessSd.Name = "textBoxLowerBrightnessSd";
            this.textBoxLowerBrightnessSd.Size = new System.Drawing.Size(100, 20);
            this.textBoxLowerBrightnessSd.TabIndex = 1;
            // 
            // labelLowerBrightnessSd
            // 
            this.labelLowerBrightnessSd.AutoSize = true;
            this.labelLowerBrightnessSd.Location = new System.Drawing.Point(6, 24);
            this.labelLowerBrightnessSd.Name = "labelLowerBrightnessSd";
            this.labelLowerBrightnessSd.Size = new System.Drawing.Size(105, 13);
            this.labelLowerBrightnessSd.TabIndex = 0;
            this.labelLowerBrightnessSd.Text = "Lower brightness SD";
            // 
            // tabPagePlaceholder
            // 
            this.tabPagePlaceholder.BackColor = System.Drawing.SystemColors.Control;
            this.tabPagePlaceholder.Location = new System.Drawing.Point(4, 22);
            this.tabPagePlaceholder.Name = "tabPagePlaceholder";
            this.tabPagePlaceholder.Padding = new System.Windows.Forms.Padding(3);
            this.tabPagePlaceholder.Size = new System.Drawing.Size(326, 301);
            this.tabPagePlaceholder.TabIndex = 1;
            this.tabPagePlaceholder.Text = "Placeholder";
            // 
            // buttonCancel
            // 
            this.buttonCancel.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.buttonCancel.Location = new System.Drawing.Point(213, 400);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new System.Drawing.Size(75, 23);
            this.buttonCancel.TabIndex = 1;
            this.buttonCancel.Text = "Cancel";
            this.buttonCancel.UseVisualStyleBackColor = true;
            this.buttonCancel.Click += new System.EventHandler(this.ButtonCancel_Click);
            // 
            // buttonAccept
            // 
            this.buttonAccept.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.buttonAccept.Location = new System.Drawing.Point(132, 400);
            this.buttonAccept.Name = "buttonAccept";
            this.buttonAccept.Size = new System.Drawing.Size(75, 23);
            this.buttonAccept.TabIndex = 2;
            this.buttonAccept.Text = "Accept";
            this.buttonAccept.UseVisualStyleBackColor = true;
            this.buttonAccept.Click += new System.EventHandler(this.ButtonAccept_Click);
            // 
            // SettingsForm
            // 
            this.AcceptButton = this.buttonAccept;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.buttonCancel;
            this.ClientSize = new System.Drawing.Size(300, 461);
            this.ControlBox = false;
            this.Controls.Add(this.buttonAccept);
            this.Controls.Add(this.buttonCancel);
            this.Controls.Add(this.tabControlSettings);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "SettingsForm";
            this.Text = "Settings";
            this.TopMost = true;
            this.tabControlSettings.ResumeLayout(false);
            this.tabPageImageAnalysis.ResumeLayout(false);
            this.tabPageImageAnalysis.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownAnnulusOuterRadius)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownAnnulusInnerRadius)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownApertureRadius)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TabControl tabControlSettings;
        private System.Windows.Forms.TabPage tabPageImageAnalysis;
        private System.Windows.Forms.TabPage tabPagePlaceholder;
        private System.Windows.Forms.Button buttonCancel;
        private System.Windows.Forms.Button buttonAccept;
        private System.Windows.Forms.TextBox textBoxLowerBrightnessSd;
        private System.Windows.Forms.Label labelLowerBrightnessSd;
        private System.Windows.Forms.TextBox textBoxUpperBrightnessSd;
        private System.Windows.Forms.Label labelUpperBrightnessSd;
        private System.Windows.Forms.Label labelAnnulusOuterRadius;
        private System.Windows.Forms.Label labelAnnulusInnerRadius;
        private System.Windows.Forms.Label labelApertureRadius;
        private System.Windows.Forms.NumericUpDown numericUpDownAnnulusOuterRadius;
        private System.Windows.Forms.NumericUpDown numericUpDownAnnulusInnerRadius;
        private System.Windows.Forms.NumericUpDown numericUpDownApertureRadius;
    }
}