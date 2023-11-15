namespace RPCC.Utils
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
            this.tabPageCameras = new System.Windows.Forms.TabPage();
            this.comboBoxTemp = new System.Windows.Forms.ComboBox();
            this.labelTemp = new System.Windows.Forms.Label();
            this.numericUpDownNumFlushes = new System.Windows.Forms.NumericUpDown();
            this.labelNumFlushes = new System.Windows.Forms.Label();
            this.textBoxiCamSn = new System.Windows.Forms.TextBox();
            this.labeliCamSn = new System.Windows.Forms.Label();
            this.textBoxrCamSn = new System.Windows.Forms.TextBox();
            this.labelrCamSn = new System.Windows.Forms.Label();
            this.textBoxgCamSn = new System.Windows.Forms.TextBox();
            this.labelgCamSn = new System.Windows.Forms.Label();
            this.tabPageSurvey = new System.Windows.Forms.TabPage();
            this.labelMainOutFolderDisplay = new System.Windows.Forms.Label();
            this.buttonSetMainOutFolder = new System.Windows.Forms.Button();
            this.labelMainOutFolder = new System.Windows.Forms.Label();
            this.tabPageComms = new System.Windows.Forms.TabPage();
            this.textBoxSiTechExeTcpIpPort = new System.Windows.Forms.TextBox();
            this.textBoxDonutsTcpIpPort = new System.Windows.Forms.TextBox();
            this.textBoxMeteoDomeTcpIpPort = new System.Windows.Forms.TextBox();
            this.numericUpDownFocusComId = new System.Windows.Forms.NumericUpDown();
            this.labelSiTechExeTcpIpPort = new System.Windows.Forms.Label();
            this.labelDonutsTcpIpPort = new System.Windows.Forms.Label();
            this.labelMeteoDomeTcpIpPort = new System.Windows.Forms.Label();
            this.labelFocusComId = new System.Windows.Forms.Label();
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
            this.buttonCancel = new System.Windows.Forms.Button();
            this.buttonAccept = new System.Windows.Forms.Button();
            this.folderBrowserDialogSetFolder = new System.Windows.Forms.FolderBrowserDialog();
            this.tabControlSettings.SuspendLayout();
            this.tabPageCameras.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownNumFlushes)).BeginInit();
            this.tabPageSurvey.SuspendLayout();
            this.tabPageComms.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownFocusComId)).BeginInit();
            this.tabPageImageAnalysis.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownAnnulusOuterRadius)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownAnnulusInnerRadius)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownApertureRadius)).BeginInit();
            this.SuspendLayout();
            // 
            // tabControlSettings
            // 
            this.tabControlSettings.Controls.Add(this.tabPageCameras);
            this.tabControlSettings.Controls.Add(this.tabPageSurvey);
            this.tabControlSettings.Controls.Add(this.tabPageComms);
            this.tabControlSettings.Controls.Add(this.tabPageImageAnalysis);
            this.tabControlSettings.Dock = System.Windows.Forms.DockStyle.Top;
            this.tabControlSettings.Location = new System.Drawing.Point(0, 0);
            this.tabControlSettings.Name = "tabControlSettings";
            this.tabControlSettings.SelectedIndex = 0;
            this.tabControlSettings.Size = new System.Drawing.Size(299, 368);
            this.tabControlSettings.TabIndex = 0;
            // 
            // tabPageCameras
            // 
            this.tabPageCameras.BackColor = System.Drawing.SystemColors.Control;
            this.tabPageCameras.Controls.Add(this.comboBoxTemp);
            this.tabPageCameras.Controls.Add(this.labelTemp);
            this.tabPageCameras.Controls.Add(this.numericUpDownNumFlushes);
            this.tabPageCameras.Controls.Add(this.labelNumFlushes);
            this.tabPageCameras.Controls.Add(this.textBoxiCamSn);
            this.tabPageCameras.Controls.Add(this.labeliCamSn);
            this.tabPageCameras.Controls.Add(this.textBoxrCamSn);
            this.tabPageCameras.Controls.Add(this.labelrCamSn);
            this.tabPageCameras.Controls.Add(this.textBoxgCamSn);
            this.tabPageCameras.Controls.Add(this.labelgCamSn);
            this.tabPageCameras.Location = new System.Drawing.Point(4, 22);
            this.tabPageCameras.Name = "tabPageCameras";
            this.tabPageCameras.Padding = new System.Windows.Forms.Padding(3);
            this.tabPageCameras.Size = new System.Drawing.Size(291, 342);
            this.tabPageCameras.TabIndex = 1;
            this.tabPageCameras.Text = "Cameras";
            // 
            // comboBoxTemp
            // 
            this.comboBoxTemp.FormattingEnabled = true;
            this.comboBoxTemp.Items.AddRange(new object[] {
            "0",
            "-5",
            "-10",
            "-15",
            "-20",
            "-25",
            "-30",
            "-35",
            "-40",
            "-45",
            "-50",
            "-55"});
            this.comboBoxTemp.Location = new System.Drawing.Point(233, 204);
            this.comboBoxTemp.Name = "comboBoxTemp";
            this.comboBoxTemp.Size = new System.Drawing.Size(50, 21);
            this.comboBoxTemp.TabIndex = 9;
            // 
            // labelTemp
            // 
            this.labelTemp.AutoSize = true;
            this.labelTemp.Location = new System.Drawing.Point(6, 204);
            this.labelTemp.Name = "labelTemp";
            this.labelTemp.Size = new System.Drawing.Size(67, 13);
            this.labelTemp.TabIndex = 8;
            this.labelTemp.Text = "Temperature";
            // 
            // numericUpDownNumFlushes
            // 
            this.numericUpDownNumFlushes.Location = new System.Drawing.Point(233, 159);
            this.numericUpDownNumFlushes.Maximum = new decimal(new int[] {
            16,
            0,
            0,
            0});
            this.numericUpDownNumFlushes.Name = "numericUpDownNumFlushes";
            this.numericUpDownNumFlushes.Size = new System.Drawing.Size(50, 20);
            this.numericUpDownNumFlushes.TabIndex = 7;
            // 
            // labelNumFlushes
            // 
            this.labelNumFlushes.AutoSize = true;
            this.labelNumFlushes.Location = new System.Drawing.Point(6, 159);
            this.labelNumFlushes.Name = "labelNumFlushes";
            this.labelNumFlushes.Size = new System.Drawing.Size(92, 13);
            this.labelNumFlushes.TabIndex = 6;
            this.labelNumFlushes.Text = "Number of flushes";
            // 
            // textBoxiCamSn
            // 
            this.textBoxiCamSn.Location = new System.Drawing.Point(183, 111);
            this.textBoxiCamSn.Name = "textBoxiCamSn";
            this.textBoxiCamSn.Size = new System.Drawing.Size(100, 20);
            this.textBoxiCamSn.TabIndex = 5;
            // 
            // labeliCamSn
            // 
            this.labeliCamSn.AutoSize = true;
            this.labeliCamSn.Location = new System.Drawing.Point(6, 114);
            this.labeliCamSn.Name = "labeliCamSn";
            this.labeliCamSn.Size = new System.Drawing.Size(122, 13);
            this.labeliCamSn.TabIndex = 4;
            this.labeliCamSn.Text = "\"i\" camera serial number";
            // 
            // textBoxrCamSn
            // 
            this.textBoxrCamSn.Location = new System.Drawing.Point(183, 66);
            this.textBoxrCamSn.Name = "textBoxrCamSn";
            this.textBoxrCamSn.Size = new System.Drawing.Size(100, 20);
            this.textBoxrCamSn.TabIndex = 3;
            // 
            // labelrCamSn
            // 
            this.labelrCamSn.AutoSize = true;
            this.labelrCamSn.Location = new System.Drawing.Point(6, 69);
            this.labelrCamSn.Name = "labelrCamSn";
            this.labelrCamSn.Size = new System.Drawing.Size(123, 13);
            this.labelrCamSn.TabIndex = 2;
            this.labelrCamSn.Text = "\"r\" camera serial number";
            // 
            // textBoxgCamSn
            // 
            this.textBoxgCamSn.Location = new System.Drawing.Point(183, 21);
            this.textBoxgCamSn.Name = "textBoxgCamSn";
            this.textBoxgCamSn.Size = new System.Drawing.Size(100, 20);
            this.textBoxgCamSn.TabIndex = 1;
            // 
            // labelgCamSn
            // 
            this.labelgCamSn.AutoSize = true;
            this.labelgCamSn.Location = new System.Drawing.Point(6, 24);
            this.labelgCamSn.Name = "labelgCamSn";
            this.labelgCamSn.Size = new System.Drawing.Size(126, 13);
            this.labelgCamSn.TabIndex = 0;
            this.labelgCamSn.Text = "\"g\" camera serial number";
            // 
            // tabPageSurvey
            // 
            this.tabPageSurvey.BackColor = System.Drawing.SystemColors.Control;
            this.tabPageSurvey.Controls.Add(this.labelMainOutFolderDisplay);
            this.tabPageSurvey.Controls.Add(this.buttonSetMainOutFolder);
            this.tabPageSurvey.Controls.Add(this.labelMainOutFolder);
            this.tabPageSurvey.Location = new System.Drawing.Point(4, 22);
            this.tabPageSurvey.Name = "tabPageSurvey";
            this.tabPageSurvey.Size = new System.Drawing.Size(291, 342);
            this.tabPageSurvey.TabIndex = 2;
            this.tabPageSurvey.Text = "Survey";
            // 
            // labelMainOutFolderDisplay
            // 
            this.labelMainOutFolderDisplay.AutoEllipsis = true;
            this.labelMainOutFolderDisplay.Location = new System.Drawing.Point(6, 45);
            this.labelMainOutFolderDisplay.Name = "labelMainOutFolderDisplay";
            this.labelMainOutFolderDisplay.Size = new System.Drawing.Size(277, 13);
            this.labelMainOutFolderDisplay.TabIndex = 2;
            this.labelMainOutFolderDisplay.Text = "OUTPUT IMAGES FOLDER OUTPUT IMAGES FOLDER";
            // 
            // buttonSetMainOutFolder
            // 
            this.buttonSetMainOutFolder.Location = new System.Drawing.Point(208, 19);
            this.buttonSetMainOutFolder.Name = "buttonSetMainOutFolder";
            this.buttonSetMainOutFolder.Size = new System.Drawing.Size(75, 23);
            this.buttonSetMainOutFolder.TabIndex = 1;
            this.buttonSetMainOutFolder.Text = "Set folder";
            this.buttonSetMainOutFolder.UseVisualStyleBackColor = true;
            this.buttonSetMainOutFolder.Click += new System.EventHandler(this.ButtonSetFolder_Click);
            // 
            // labelMainOutFolder
            // 
            this.labelMainOutFolder.AutoSize = true;
            this.labelMainOutFolder.Location = new System.Drawing.Point(6, 24);
            this.labelMainOutFolder.Name = "labelMainOutFolder";
            this.labelMainOutFolder.Size = new System.Drawing.Size(92, 13);
            this.labelMainOutFolder.TabIndex = 0;
            this.labelMainOutFolder.Text = "Main output folder";
            // 
            // tabPageComms
            // 
            this.tabPageComms.BackColor = System.Drawing.SystemColors.Control;
            this.tabPageComms.Controls.Add(this.textBoxSiTechExeTcpIpPort);
            this.tabPageComms.Controls.Add(this.textBoxDonutsTcpIpPort);
            this.tabPageComms.Controls.Add(this.textBoxMeteoDomeTcpIpPort);
            this.tabPageComms.Controls.Add(this.numericUpDownFocusComId);
            this.tabPageComms.Controls.Add(this.labelSiTechExeTcpIpPort);
            this.tabPageComms.Controls.Add(this.labelDonutsTcpIpPort);
            this.tabPageComms.Controls.Add(this.labelMeteoDomeTcpIpPort);
            this.tabPageComms.Controls.Add(this.labelFocusComId);
            this.tabPageComms.Location = new System.Drawing.Point(4, 22);
            this.tabPageComms.Name = "tabPageComms";
            this.tabPageComms.Size = new System.Drawing.Size(291, 342);
            this.tabPageComms.TabIndex = 3;
            this.tabPageComms.Text = "Comms";
            // 
            // textBoxSiTechExeTcpIpPort
            // 
            this.textBoxSiTechExeTcpIpPort.Location = new System.Drawing.Point(238, 159);
            this.textBoxSiTechExeTcpIpPort.Name = "textBoxSiTechExeTcpIpPort";
            this.textBoxSiTechExeTcpIpPort.Size = new System.Drawing.Size(45, 20);
            this.textBoxSiTechExeTcpIpPort.TabIndex = 7;
            // 
            // textBoxDonutsTcpIpPort
            // 
            this.textBoxDonutsTcpIpPort.Location = new System.Drawing.Point(238, 114);
            this.textBoxDonutsTcpIpPort.Name = "textBoxDonutsTcpIpPort";
            this.textBoxDonutsTcpIpPort.Size = new System.Drawing.Size(45, 20);
            this.textBoxDonutsTcpIpPort.TabIndex = 6;
            // 
            // textBoxMeteoDomeTcpIpPort
            // 
            this.textBoxMeteoDomeTcpIpPort.Location = new System.Drawing.Point(238, 69);
            this.textBoxMeteoDomeTcpIpPort.Name = "textBoxMeteoDomeTcpIpPort";
            this.textBoxMeteoDomeTcpIpPort.Size = new System.Drawing.Size(45, 20);
            this.textBoxMeteoDomeTcpIpPort.TabIndex = 5;
            // 
            // numericUpDownFocusComId
            // 
            this.numericUpDownFocusComId.Location = new System.Drawing.Point(241, 24);
            this.numericUpDownFocusComId.Maximum = new decimal(new int[] {
            256,
            0,
            0,
            0});
            this.numericUpDownFocusComId.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.numericUpDownFocusComId.Name = "numericUpDownFocusComId";
            this.numericUpDownFocusComId.Size = new System.Drawing.Size(42, 20);
            this.numericUpDownFocusComId.TabIndex = 4;
            this.numericUpDownFocusComId.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            // 
            // labelSiTechExeTcpIpPort
            // 
            this.labelSiTechExeTcpIpPort.AutoSize = true;
            this.labelSiTechExeTcpIpPort.Location = new System.Drawing.Point(6, 159);
            this.labelSiTechExeTcpIpPort.Name = "labelSiTechExeTcpIpPort";
            this.labelSiTechExeTcpIpPort.Size = new System.Drawing.Size(120, 13);
            this.labelSiTechExeTcpIpPort.TabIndex = 3;
            this.labelSiTechExeTcpIpPort.Text = "SiTechExe TCP/IP Port";
            // 
            // labelDonutsTcpIpPort
            // 
            this.labelDonutsTcpIpPort.AutoSize = true;
            this.labelDonutsTcpIpPort.Location = new System.Drawing.Point(6, 114);
            this.labelDonutsTcpIpPort.Name = "labelDonutsTcpIpPort";
            this.labelDonutsTcpIpPort.Size = new System.Drawing.Size(114, 13);
            this.labelDonutsTcpIpPort.TabIndex = 2;
            this.labelDonutsTcpIpPort.Text = "DONUTS TCP/IP Port";
            // 
            // labelMeteoDomeTcpIpPort
            // 
            this.labelMeteoDomeTcpIpPort.AutoSize = true;
            this.labelMeteoDomeTcpIpPort.Location = new System.Drawing.Point(6, 69);
            this.labelMeteoDomeTcpIpPort.Name = "labelMeteoDomeTcpIpPort";
            this.labelMeteoDomeTcpIpPort.Size = new System.Drawing.Size(126, 13);
            this.labelMeteoDomeTcpIpPort.TabIndex = 1;
            this.labelMeteoDomeTcpIpPort.Text = "MeteoDome TCP/IP Port";
            // 
            // labelFocusComId
            // 
            this.labelFocusComId.AutoSize = true;
            this.labelFocusComId.Location = new System.Drawing.Point(6, 24);
            this.labelFocusComId.Name = "labelFocusComId";
            this.labelFocusComId.Size = new System.Drawing.Size(77, 13);
            this.labelFocusComId.TabIndex = 0;
            this.labelFocusComId.Text = "Focus COM ID";
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
            this.tabPageImageAnalysis.Size = new System.Drawing.Size(291, 342);
            this.tabPageImageAnalysis.TabIndex = 0;
            this.tabPageImageAnalysis.Text = "Image Analysis";
            // 
            // numericUpDownAnnulusOuterRadius
            // 
            this.numericUpDownAnnulusOuterRadius.Location = new System.Drawing.Point(234, 192);
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
            this.numericUpDownAnnulusInnerRadius.Location = new System.Drawing.Point(234, 151);
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
            this.numericUpDownApertureRadius.Location = new System.Drawing.Point(234, 112);
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
            this.textBoxUpperBrightnessSd.Location = new System.Drawing.Point(208, 66);
            this.textBoxUpperBrightnessSd.Name = "textBoxUpperBrightnessSd";
            this.textBoxUpperBrightnessSd.Size = new System.Drawing.Size(75, 20);
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
            this.textBoxLowerBrightnessSd.Location = new System.Drawing.Point(208, 21);
            this.textBoxLowerBrightnessSd.Name = "textBoxLowerBrightnessSd";
            this.textBoxLowerBrightnessSd.Size = new System.Drawing.Size(75, 20);
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
            // buttonCancel
            // 
            this.buttonCancel.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.buttonCancel.Location = new System.Drawing.Point(212, 376);
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
            this.buttonAccept.Location = new System.Drawing.Point(131, 376);
            this.buttonAccept.Name = "buttonAccept";
            this.buttonAccept.Size = new System.Drawing.Size(75, 23);
            this.buttonAccept.TabIndex = 2;
            this.buttonAccept.Text = "Accept";
            this.buttonAccept.UseVisualStyleBackColor = true;
            this.buttonAccept.Click += new System.EventHandler(this.ButtonAccept_Click);
            // 
            // folderBrowserDialogSetFolder
            // 
            this.folderBrowserDialogSetFolder.RootFolder = System.Environment.SpecialFolder.MyComputer;
            // 
            // SettingsForm
            // 
            this.AcceptButton = this.buttonAccept;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.buttonCancel;
            this.ClientSize = new System.Drawing.Size(299, 411);
            this.ControlBox = false;
            this.Controls.Add(this.buttonCancel);
            this.Controls.Add(this.tabControlSettings);
            this.Controls.Add(this.buttonAccept);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "SettingsForm";
            this.Text = "Settings";
            this.tabControlSettings.ResumeLayout(false);
            this.tabPageCameras.ResumeLayout(false);
            this.tabPageCameras.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownNumFlushes)).EndInit();
            this.tabPageSurvey.ResumeLayout(false);
            this.tabPageSurvey.PerformLayout();
            this.tabPageComms.ResumeLayout(false);
            this.tabPageComms.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownFocusComId)).EndInit();
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
        private System.Windows.Forms.TabPage tabPageCameras;
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
        private System.Windows.Forms.TextBox textBoxgCamSn;
        private System.Windows.Forms.Label labelgCamSn;
        private System.Windows.Forms.Label labelrCamSn;
        private System.Windows.Forms.TextBox textBoxrCamSn;
        private System.Windows.Forms.Label labeliCamSn;
        private System.Windows.Forms.TextBox textBoxiCamSn;
        private System.Windows.Forms.Label labelNumFlushes;
        private System.Windows.Forms.NumericUpDown numericUpDownNumFlushes;
        private System.Windows.Forms.ComboBox comboBoxTemp;
        private System.Windows.Forms.Label labelTemp;
        private System.Windows.Forms.TabPage tabPageSurvey;
        private System.Windows.Forms.Label labelMainOutFolder;
        private System.Windows.Forms.FolderBrowserDialog folderBrowserDialogSetFolder;
        private System.Windows.Forms.Button buttonSetMainOutFolder;
        private System.Windows.Forms.Label labelMainOutFolderDisplay;
        private System.Windows.Forms.TabPage tabPageComms;
        private System.Windows.Forms.Label labelSiTechExeTcpIpPort;
        private System.Windows.Forms.Label labelDonutsTcpIpPort;
        private System.Windows.Forms.Label labelMeteoDomeTcpIpPort;
        private System.Windows.Forms.Label labelFocusComId;
        private System.Windows.Forms.NumericUpDown numericUpDownFocusComId;
        private System.Windows.Forms.TextBox textBoxMeteoDomeTcpIpPort;
        private System.Windows.Forms.TextBox textBoxSiTechExeTcpIpPort;
        private System.Windows.Forms.TextBox textBoxDonutsTcpIpPort;
    }
}