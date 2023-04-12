namespace RPCC
{
    partial class MainForm
    {
        /// <summary>
        /// Обязательная переменная конструктора.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Освободить все используемые ресурсы.
        /// </summary>
        /// <param name="disposing">истинно, если управляемый ресурс должен быть удален; иначе ложно.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Код, автоматически созданный конструктором форм Windows

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            this.statusStrip = new System.Windows.Forms.StatusStrip();
            this.tSStatusClock = new System.Windows.Forms.ToolStripStatusLabel();
            this.menuStrip = new System.Windows.Forms.MenuStrip();
            this.launchToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.findCamerasToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.focusToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.optionsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.settingsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.updateCamerasSettingsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.loadConfigToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.saveConfigToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.reconnectSocketsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.debugToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.testDLLlibrariesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.loadTestImageToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.restoreDefaultConfigFileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.groupBoxLogs = new System.Windows.Forms.GroupBox();
            this.buttonLogsSave = new System.Windows.Forms.Button();
            this.buttonLogsClear = new System.Windows.Forms.Button();
            this.listBoxLogs = new System.Windows.Forms.ListBox();
            this.timerClock = new System.Windows.Forms.Timer(this.components);
            this.saveFileDialogConfig = new System.Windows.Forms.SaveFileDialog();
            this.openFileDialogConfig = new System.Windows.Forms.OpenFileDialog();
            this.panelFitsImage = new System.Windows.Forms.Panel();
            this.pictureBoxFits = new System.Windows.Forms.PictureBox();
            this.groupBoxImageAnalysis = new System.Windows.Forms.GroupBox();
            this.pictureBoxProfile = new System.Windows.Forms.PictureBox();
            this.groupBoxCam1 = new System.Windows.Forms.GroupBox();
            this.labelCam1RemTime = new System.Windows.Forms.Label();
            this.labelCam1Status = new System.Windows.Forms.Label();
            this.labelCam1CoolerPwr = new System.Windows.Forms.Label();
            this.labelCam1BaseTemp = new System.Windows.Forms.Label();
            this.labelCam1CcdTemp = new System.Windows.Forms.Label();
            this.labelCam1Filter = new System.Windows.Forms.Label();
            this.labelCam1Sn = new System.Windows.Forms.Label();
            this.labelCam1Model = new System.Windows.Forms.Label();
            this.groupBoxCam2 = new System.Windows.Forms.GroupBox();
            this.labelCam2RemTime = new System.Windows.Forms.Label();
            this.labelCam2Status = new System.Windows.Forms.Label();
            this.labelCam2CoolerPwr = new System.Windows.Forms.Label();
            this.labelCam2BaseTemp = new System.Windows.Forms.Label();
            this.labelCam2CcdTemp = new System.Windows.Forms.Label();
            this.labelCam2Filter = new System.Windows.Forms.Label();
            this.labelCam2Sn = new System.Windows.Forms.Label();
            this.labelCam2Model = new System.Windows.Forms.Label();
            this.groupBoxCam3 = new System.Windows.Forms.GroupBox();
            this.labelCam3RemTime = new System.Windows.Forms.Label();
            this.labelCam3Status = new System.Windows.Forms.Label();
            this.labelCam3CoolerPwr = new System.Windows.Forms.Label();
            this.labelCam3BaseTemp = new System.Windows.Forms.Label();
            this.labelCam3CcdTemp = new System.Windows.Forms.Label();
            this.labelCam3Filter = new System.Windows.Forms.Label();
            this.labelCam3Sn = new System.Windows.Forms.Label();
            this.labelCam3Model = new System.Windows.Forms.Label();
            this.radioButtonViewCam3 = new System.Windows.Forms.RadioButton();
            this.radioButtonViewCam2 = new System.Windows.Forms.RadioButton();
            this.radioButtonViewCam1 = new System.Windows.Forms.RadioButton();
            this.groupBoxSurvey = new System.Windows.Forms.GroupBox();
            this.buttonSurveyStop = new System.Windows.Forms.Button();
            this.buttonSurveyStart = new System.Windows.Forms.Button();
            this.numericUpDownSequence = new System.Windows.Forms.NumericUpDown();
            this.labelSequence = new System.Windows.Forms.Label();
            this.numericUpDownExpTime = new System.Windows.Forms.NumericUpDown();
            this.labelExpTime = new System.Windows.Forms.Label();
            this.comboBoxImgType = new System.Windows.Forms.ComboBox();
            this.labelImgType = new System.Windows.Forms.Label();
            this.testDonToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.reconnectSocketsToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.statusStrip.SuspendLayout();
            this.menuStrip.SuspendLayout();
            this.groupBoxLogs.SuspendLayout();
            this.panelFitsImage.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxFits)).BeginInit();
            this.groupBoxImageAnalysis.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxProfile)).BeginInit();
            this.groupBoxCam1.SuspendLayout();
            this.groupBoxCam2.SuspendLayout();
            this.groupBoxCam3.SuspendLayout();
            this.groupBoxSurvey.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownSequence)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownExpTime)).BeginInit();
            this.SuspendLayout();
            // 
            // statusStrip
            // 
            this.statusStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tSStatusClock});
            this.statusStrip.Location = new System.Drawing.Point(0, 509);
            this.statusStrip.Name = "statusStrip";
            this.statusStrip.Size = new System.Drawing.Size(1140, 22);
            this.statusStrip.SizingGrip = false;
            this.statusStrip.TabIndex = 0;
            this.statusStrip.Text = "statusStrip";
            // 
            // tSStatusClock
            // 
            this.tSStatusClock.Name = "tSStatusClock";
            this.tSStatusClock.Size = new System.Drawing.Size(158, 17);
            this.tSStatusClock.Text = "yyyy-MM-ddTHH-mm-ss.fff";
            // 
            // menuStrip
            // 
            this.menuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.launchToolStripMenuItem,
            this.optionsToolStripMenuItem,
            this.debugToolStripMenuItem});
            this.menuStrip.Location = new System.Drawing.Point(0, 0);
            this.menuStrip.Name = "menuStrip";
            this.menuStrip.Size = new System.Drawing.Size(1140, 24);
            this.menuStrip.TabIndex = 1;
            this.menuStrip.Text = "menuStrip";
            // 
            // launchToolStripMenuItem
            // 
            this.launchToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.findCamerasToolStripMenuItem,
            this.focusToolStripMenuItem,
            this.reconnectSocketsToolStripMenuItem1});
            this.launchToolStripMenuItem.Name = "launchToolStripMenuItem";
            this.launchToolStripMenuItem.Size = new System.Drawing.Size(58, 20);
            this.launchToolStripMenuItem.Text = "Launch";
            // 
            // findCamerasToolStripMenuItem
            // 
            this.findCamerasToolStripMenuItem.Name = "findCamerasToolStripMenuItem";
            this.findCamerasToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            this.findCamerasToolStripMenuItem.Text = "Find cameras";
            this.findCamerasToolStripMenuItem.Click += new System.EventHandler(this.FindCamerasToolStripMenuItem_Click);
            // 
            // focusToolStripMenuItem
            // 
            this.focusToolStripMenuItem.Name = "focusToolStripMenuItem";
            this.focusToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            this.focusToolStripMenuItem.Text = "Focus";
            this.focusToolStripMenuItem.Click += new System.EventHandler(this.FocusToolStripMenuItem_Click);
            // 
            // optionsToolStripMenuItem
            // 
            this.optionsToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.settingsToolStripMenuItem,
            this.updateCamerasSettingsToolStripMenuItem,
            this.loadConfigToolStripMenuItem,
            this.saveConfigToolStripMenuItem,
            this.reconnectSocketsToolStripMenuItem});
            this.optionsToolStripMenuItem.Name = "optionsToolStripMenuItem";
            this.optionsToolStripMenuItem.Size = new System.Drawing.Size(61, 20);
            this.optionsToolStripMenuItem.Text = "Options";
            // 
            // settingsToolStripMenuItem
            // 
            this.settingsToolStripMenuItem.Name = "settingsToolStripMenuItem";
            this.settingsToolStripMenuItem.Size = new System.Drawing.Size(203, 22);
            this.settingsToolStripMenuItem.Text = "Settings";
            this.settingsToolStripMenuItem.Click += new System.EventHandler(this.SettingsToolStripMenuItem_Click);
            // 
            // updateCamerasSettingsToolStripMenuItem
            // 
            this.updateCamerasSettingsToolStripMenuItem.Name = "updateCamerasSettingsToolStripMenuItem";
            this.updateCamerasSettingsToolStripMenuItem.Size = new System.Drawing.Size(203, 22);
            this.updateCamerasSettingsToolStripMenuItem.Text = "Update cameras settings";
            this.updateCamerasSettingsToolStripMenuItem.Click += new System.EventHandler(this.UpdateCameraSettingsToolStripMenuItem_Click);
            // 
            // loadConfigToolStripMenuItem
            // 
            this.loadConfigToolStripMenuItem.Name = "loadConfigToolStripMenuItem";
            this.loadConfigToolStripMenuItem.Size = new System.Drawing.Size(203, 22);
            this.loadConfigToolStripMenuItem.Text = "Load config";
            this.loadConfigToolStripMenuItem.Click += new System.EventHandler(this.LoadConfigToolStripMenuItem_Click);
            // 
            // saveConfigToolStripMenuItem
            // 
            this.saveConfigToolStripMenuItem.Name = "saveConfigToolStripMenuItem";
            this.saveConfigToolStripMenuItem.Size = new System.Drawing.Size(203, 22);
            this.saveConfigToolStripMenuItem.Text = "Save config";
            this.saveConfigToolStripMenuItem.Click += new System.EventHandler(this.SaveConfigToolStripMenuItem_Click);
            // 
            // reconnectSocketsToolStripMenuItem
            // 
            this.reconnectSocketsToolStripMenuItem.Name = "reconnectSocketsToolStripMenuItem";
            this.reconnectSocketsToolStripMenuItem.Size = new System.Drawing.Size(203, 22);
            this.reconnectSocketsToolStripMenuItem.Text = "Reconnect sockets";
            this.reconnectSocketsToolStripMenuItem.Click += new System.EventHandler(this.ReconnectSocketsToolStripMenuItem_Click);
            // 
            // debugToolStripMenuItem
            // 
            this.debugToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.testDLLlibrariesToolStripMenuItem,
            this.loadTestImageToolStripMenuItem,
            this.restoreDefaultConfigFileToolStripMenuItem,
            this.testDonToolStripMenuItem});
            this.debugToolStripMenuItem.Name = "debugToolStripMenuItem";
            this.debugToolStripMenuItem.Size = new System.Drawing.Size(54, 20);
            this.debugToolStripMenuItem.Text = "Debug";
            // 
            // testDLLlibrariesToolStripMenuItem
            // 
            this.testDLLlibrariesToolStripMenuItem.Name = "testDLLlibrariesToolStripMenuItem";
            this.testDLLlibrariesToolStripMenuItem.Size = new System.Drawing.Size(209, 22);
            this.testDLLlibrariesToolStripMenuItem.Text = "Test DLL-libraries";
            this.testDLLlibrariesToolStripMenuItem.Click += new System.EventHandler(this.TestDLLlibrariesToolStripMenuItem_Click);
            // 
            // loadTestImageToolStripMenuItem
            // 
            this.loadTestImageToolStripMenuItem.Name = "loadTestImageToolStripMenuItem";
            this.loadTestImageToolStripMenuItem.Size = new System.Drawing.Size(209, 22);
            this.loadTestImageToolStripMenuItem.Text = "Load test image";
            this.loadTestImageToolStripMenuItem.Click += new System.EventHandler(this.LoadTestImageToolStripMenuItem_Click);
            // 
            // restoreDefaultConfigFileToolStripMenuItem
            // 
            this.restoreDefaultConfigFileToolStripMenuItem.Name = "restoreDefaultConfigFileToolStripMenuItem";
            this.restoreDefaultConfigFileToolStripMenuItem.Size = new System.Drawing.Size(209, 22);
            this.restoreDefaultConfigFileToolStripMenuItem.Text = "Restore default config file";
            this.restoreDefaultConfigFileToolStripMenuItem.Click += new System.EventHandler(this.RestoreDefaultConfigFileToolStripMenuItem_Click);
            // 
            // groupBoxLogs
            // 
            this.groupBoxLogs.Controls.Add(this.buttonLogsSave);
            this.groupBoxLogs.Controls.Add(this.buttonLogsClear);
            this.groupBoxLogs.Controls.Add(this.listBoxLogs);
            this.groupBoxLogs.Location = new System.Drawing.Point(12, 27);
            this.groupBoxLogs.Name = "groupBoxLogs";
            this.groupBoxLogs.Size = new System.Drawing.Size(330, 477);
            this.groupBoxLogs.TabIndex = 2;
            this.groupBoxLogs.TabStop = false;
            this.groupBoxLogs.Text = "Logs";
            // 
            // buttonLogsSave
            // 
            this.buttonLogsSave.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonLogsSave.Location = new System.Drawing.Point(224, 448);
            this.buttonLogsSave.Name = "buttonLogsSave";
            this.buttonLogsSave.Size = new System.Drawing.Size(100, 23);
            this.buttonLogsSave.TabIndex = 2;
            this.buttonLogsSave.Text = "Save";
            this.buttonLogsSave.UseVisualStyleBackColor = true;
            this.buttonLogsSave.Click += new System.EventHandler(this.ButtonLogsSave_Click);
            // 
            // buttonLogsClear
            // 
            this.buttonLogsClear.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.buttonLogsClear.Location = new System.Drawing.Point(4, 448);
            this.buttonLogsClear.Name = "buttonLogsClear";
            this.buttonLogsClear.Size = new System.Drawing.Size(100, 23);
            this.buttonLogsClear.TabIndex = 1;
            this.buttonLogsClear.Text = "Clear";
            this.buttonLogsClear.UseVisualStyleBackColor = true;
            this.buttonLogsClear.Click += new System.EventHandler(this.ButtonLogsClear_Click);
            // 
            // listBoxLogs
            // 
            this.listBoxLogs.FormattingEnabled = true;
            this.listBoxLogs.HorizontalScrollbar = true;
            this.listBoxLogs.Location = new System.Drawing.Point(4, 16);
            this.listBoxLogs.Name = "listBoxLogs";
            this.listBoxLogs.Size = new System.Drawing.Size(320, 420);
            this.listBoxLogs.TabIndex = 0;
            // 
            // timerClock
            // 
            this.timerClock.Interval = 1000;
            this.timerClock.Tick += new System.EventHandler(this.TimerClock_Tick);
            // 
            // saveFileDialogConfig
            // 
            this.saveFileDialogConfig.DefaultExt = "xml";
            this.saveFileDialogConfig.FileName = "NewConfig.xml";
            this.saveFileDialogConfig.Filter = "XML-файлы (*.xml)|*.xml|All files (*.*)|*.*";
            this.saveFileDialogConfig.Title = "Save config";
            // 
            // openFileDialogConfig
            // 
            this.openFileDialogConfig.DefaultExt = "xml";
            this.openFileDialogConfig.Filter = "XML-файлы (*.xml)|*.xml|All files (*.*)|*.*";
            this.openFileDialogConfig.Title = "Load config";
            // 
            // panelFitsImage
            // 
            this.panelFitsImage.Controls.Add(this.pictureBoxFits);
            this.panelFitsImage.Location = new System.Drawing.Point(6, 19);
            this.panelFitsImage.Name = "panelFitsImage";
            this.panelFitsImage.Size = new System.Drawing.Size(300, 300);
            this.panelFitsImage.TabIndex = 4;
            // 
            // pictureBoxFits
            // 
            this.pictureBoxFits.BackColor = System.Drawing.SystemColors.Desktop;
            this.pictureBoxFits.Location = new System.Drawing.Point(0, 0);
            this.pictureBoxFits.Name = "pictureBoxFits";
            this.pictureBoxFits.Size = new System.Drawing.Size(300, 300);
            this.pictureBoxFits.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.pictureBoxFits.TabIndex = 0;
            this.pictureBoxFits.TabStop = false;
            this.pictureBoxFits.MouseClick += new System.Windows.Forms.MouseEventHandler(this.PictureBoxFits_MouseClick);
            // 
            // groupBoxImageAnalysis
            // 
            this.groupBoxImageAnalysis.Controls.Add(this.pictureBoxProfile);
            this.groupBoxImageAnalysis.Controls.Add(this.panelFitsImage);
            this.groupBoxImageAnalysis.Location = new System.Drawing.Point(348, 27);
            this.groupBoxImageAnalysis.Name = "groupBoxImageAnalysis";
            this.groupBoxImageAnalysis.Size = new System.Drawing.Size(312, 477);
            this.groupBoxImageAnalysis.TabIndex = 5;
            this.groupBoxImageAnalysis.TabStop = false;
            this.groupBoxImageAnalysis.Text = "Image Analysis";
            // 
            // pictureBoxProfile
            // 
            this.pictureBoxProfile.BackColor = System.Drawing.SystemColors.Desktop;
            this.pictureBoxProfile.Location = new System.Drawing.Point(6, 326);
            this.pictureBoxProfile.Name = "pictureBoxProfile";
            this.pictureBoxProfile.Size = new System.Drawing.Size(300, 144);
            this.pictureBoxProfile.TabIndex = 5;
            this.pictureBoxProfile.TabStop = false;
            // 
            // groupBoxCam1
            // 
            this.groupBoxCam1.Controls.Add(this.labelCam1RemTime);
            this.groupBoxCam1.Controls.Add(this.labelCam1Status);
            this.groupBoxCam1.Controls.Add(this.labelCam1CoolerPwr);
            this.groupBoxCam1.Controls.Add(this.labelCam1BaseTemp);
            this.groupBoxCam1.Controls.Add(this.labelCam1CcdTemp);
            this.groupBoxCam1.Controls.Add(this.labelCam1Filter);
            this.groupBoxCam1.Controls.Add(this.labelCam1Sn);
            this.groupBoxCam1.Controls.Add(this.labelCam1Model);
            this.groupBoxCam1.Enabled = false;
            this.groupBoxCam1.Location = new System.Drawing.Point(666, 27);
            this.groupBoxCam1.Name = "groupBoxCam1";
            this.groupBoxCam1.Size = new System.Drawing.Size(150, 155);
            this.groupBoxCam1.TabIndex = 6;
            this.groupBoxCam1.TabStop = false;
            this.groupBoxCam1.Text = "Camera 1";
            // 
            // labelCam1RemTime
            // 
            this.labelCam1RemTime.AutoSize = true;
            this.labelCam1RemTime.Location = new System.Drawing.Point(6, 135);
            this.labelCam1RemTime.Name = "labelCam1RemTime";
            this.labelCam1RemTime.Size = new System.Drawing.Size(60, 13);
            this.labelCam1RemTime.TabIndex = 7;
            this.labelCam1RemTime.Text = "Remaining:";
            // 
            // labelCam1Status
            // 
            this.labelCam1Status.AutoSize = true;
            this.labelCam1Status.Location = new System.Drawing.Point(6, 118);
            this.labelCam1Status.Name = "labelCam1Status";
            this.labelCam1Status.Size = new System.Drawing.Size(40, 13);
            this.labelCam1Status.TabIndex = 6;
            this.labelCam1Status.Text = "Status:";
            // 
            // labelCam1CoolerPwr
            // 
            this.labelCam1CoolerPwr.AutoSize = true;
            this.labelCam1CoolerPwr.Location = new System.Drawing.Point(6, 101);
            this.labelCam1CoolerPwr.Name = "labelCam1CoolerPwr";
            this.labelCam1CoolerPwr.Size = new System.Drawing.Size(73, 13);
            this.labelCam1CoolerPwr.TabIndex = 5;
            this.labelCam1CoolerPwr.Text = "Cooler Power:";
            // 
            // labelCam1BaseTemp
            // 
            this.labelCam1BaseTemp.AutoSize = true;
            this.labelCam1BaseTemp.Location = new System.Drawing.Point(6, 84);
            this.labelCam1BaseTemp.Name = "labelCam1BaseTemp";
            this.labelCam1BaseTemp.Size = new System.Drawing.Size(64, 13);
            this.labelCam1BaseTemp.TabIndex = 4;
            this.labelCam1BaseTemp.Text = "Base Temp:";
            // 
            // labelCam1CcdTemp
            // 
            this.labelCam1CcdTemp.AutoSize = true;
            this.labelCam1CcdTemp.Location = new System.Drawing.Point(6, 67);
            this.labelCam1CcdTemp.Name = "labelCam1CcdTemp";
            this.labelCam1CcdTemp.Size = new System.Drawing.Size(62, 13);
            this.labelCam1CcdTemp.TabIndex = 3;
            this.labelCam1CcdTemp.Text = "CCD Temp:";
            // 
            // labelCam1Filter
            // 
            this.labelCam1Filter.AutoSize = true;
            this.labelCam1Filter.Location = new System.Drawing.Point(6, 50);
            this.labelCam1Filter.Name = "labelCam1Filter";
            this.labelCam1Filter.Size = new System.Drawing.Size(32, 13);
            this.labelCam1Filter.TabIndex = 2;
            this.labelCam1Filter.Text = "Filter:";
            // 
            // labelCam1Sn
            // 
            this.labelCam1Sn.AutoSize = true;
            this.labelCam1Sn.Location = new System.Drawing.Point(6, 33);
            this.labelCam1Sn.Name = "labelCam1Sn";
            this.labelCam1Sn.Size = new System.Drawing.Size(61, 13);
            this.labelCam1Sn.TabIndex = 1;
            this.labelCam1Sn.Text = "Serial Num:";
            // 
            // labelCam1Model
            // 
            this.labelCam1Model.AutoSize = true;
            this.labelCam1Model.Location = new System.Drawing.Point(6, 16);
            this.labelCam1Model.Name = "labelCam1Model";
            this.labelCam1Model.Size = new System.Drawing.Size(39, 13);
            this.labelCam1Model.TabIndex = 0;
            this.labelCam1Model.Text = "Model:";
            // 
            // groupBoxCam2
            // 
            this.groupBoxCam2.Controls.Add(this.labelCam2RemTime);
            this.groupBoxCam2.Controls.Add(this.labelCam2Status);
            this.groupBoxCam2.Controls.Add(this.labelCam2CoolerPwr);
            this.groupBoxCam2.Controls.Add(this.labelCam2BaseTemp);
            this.groupBoxCam2.Controls.Add(this.labelCam2CcdTemp);
            this.groupBoxCam2.Controls.Add(this.labelCam2Filter);
            this.groupBoxCam2.Controls.Add(this.labelCam2Sn);
            this.groupBoxCam2.Controls.Add(this.labelCam2Model);
            this.groupBoxCam2.Enabled = false;
            this.groupBoxCam2.Location = new System.Drawing.Point(822, 27);
            this.groupBoxCam2.Name = "groupBoxCam2";
            this.groupBoxCam2.Size = new System.Drawing.Size(150, 155);
            this.groupBoxCam2.TabIndex = 7;
            this.groupBoxCam2.TabStop = false;
            this.groupBoxCam2.Text = "Camera 2";
            // 
            // labelCam2RemTime
            // 
            this.labelCam2RemTime.AutoSize = true;
            this.labelCam2RemTime.Location = new System.Drawing.Point(6, 135);
            this.labelCam2RemTime.Name = "labelCam2RemTime";
            this.labelCam2RemTime.Size = new System.Drawing.Size(60, 13);
            this.labelCam2RemTime.TabIndex = 15;
            this.labelCam2RemTime.Text = "Remaining:";
            // 
            // labelCam2Status
            // 
            this.labelCam2Status.AutoSize = true;
            this.labelCam2Status.Location = new System.Drawing.Point(6, 118);
            this.labelCam2Status.Name = "labelCam2Status";
            this.labelCam2Status.Size = new System.Drawing.Size(40, 13);
            this.labelCam2Status.TabIndex = 14;
            this.labelCam2Status.Text = "Status:";
            // 
            // labelCam2CoolerPwr
            // 
            this.labelCam2CoolerPwr.AutoSize = true;
            this.labelCam2CoolerPwr.Location = new System.Drawing.Point(6, 101);
            this.labelCam2CoolerPwr.Name = "labelCam2CoolerPwr";
            this.labelCam2CoolerPwr.Size = new System.Drawing.Size(73, 13);
            this.labelCam2CoolerPwr.TabIndex = 13;
            this.labelCam2CoolerPwr.Text = "Cooler Power:";
            // 
            // labelCam2BaseTemp
            // 
            this.labelCam2BaseTemp.AutoSize = true;
            this.labelCam2BaseTemp.Location = new System.Drawing.Point(6, 84);
            this.labelCam2BaseTemp.Name = "labelCam2BaseTemp";
            this.labelCam2BaseTemp.Size = new System.Drawing.Size(64, 13);
            this.labelCam2BaseTemp.TabIndex = 12;
            this.labelCam2BaseTemp.Text = "Base Temp:";
            // 
            // labelCam2CcdTemp
            // 
            this.labelCam2CcdTemp.AutoSize = true;
            this.labelCam2CcdTemp.Location = new System.Drawing.Point(6, 67);
            this.labelCam2CcdTemp.Name = "labelCam2CcdTemp";
            this.labelCam2CcdTemp.Size = new System.Drawing.Size(62, 13);
            this.labelCam2CcdTemp.TabIndex = 11;
            this.labelCam2CcdTemp.Text = "CCD Temp:";
            // 
            // labelCam2Filter
            // 
            this.labelCam2Filter.AutoSize = true;
            this.labelCam2Filter.Location = new System.Drawing.Point(6, 50);
            this.labelCam2Filter.Name = "labelCam2Filter";
            this.labelCam2Filter.Size = new System.Drawing.Size(32, 13);
            this.labelCam2Filter.TabIndex = 10;
            this.labelCam2Filter.Text = "Filter:";
            // 
            // labelCam2Sn
            // 
            this.labelCam2Sn.AutoSize = true;
            this.labelCam2Sn.Location = new System.Drawing.Point(6, 33);
            this.labelCam2Sn.Name = "labelCam2Sn";
            this.labelCam2Sn.Size = new System.Drawing.Size(61, 13);
            this.labelCam2Sn.TabIndex = 9;
            this.labelCam2Sn.Text = "Serial Num:";
            // 
            // labelCam2Model
            // 
            this.labelCam2Model.AutoSize = true;
            this.labelCam2Model.Location = new System.Drawing.Point(6, 16);
            this.labelCam2Model.Name = "labelCam2Model";
            this.labelCam2Model.Size = new System.Drawing.Size(39, 13);
            this.labelCam2Model.TabIndex = 8;
            this.labelCam2Model.Text = "Model:";
            // 
            // groupBoxCam3
            // 
            this.groupBoxCam3.Controls.Add(this.labelCam3RemTime);
            this.groupBoxCam3.Controls.Add(this.labelCam3Status);
            this.groupBoxCam3.Controls.Add(this.labelCam3CoolerPwr);
            this.groupBoxCam3.Controls.Add(this.labelCam3BaseTemp);
            this.groupBoxCam3.Controls.Add(this.labelCam3CcdTemp);
            this.groupBoxCam3.Controls.Add(this.labelCam3Filter);
            this.groupBoxCam3.Controls.Add(this.labelCam3Sn);
            this.groupBoxCam3.Controls.Add(this.labelCam3Model);
            this.groupBoxCam3.Enabled = false;
            this.groupBoxCam3.Location = new System.Drawing.Point(978, 27);
            this.groupBoxCam3.Name = "groupBoxCam3";
            this.groupBoxCam3.Size = new System.Drawing.Size(150, 155);
            this.groupBoxCam3.TabIndex = 8;
            this.groupBoxCam3.TabStop = false;
            this.groupBoxCam3.Text = "Camera 3";
            // 
            // labelCam3RemTime
            // 
            this.labelCam3RemTime.AutoSize = true;
            this.labelCam3RemTime.Location = new System.Drawing.Point(6, 135);
            this.labelCam3RemTime.Name = "labelCam3RemTime";
            this.labelCam3RemTime.Size = new System.Drawing.Size(60, 13);
            this.labelCam3RemTime.TabIndex = 23;
            this.labelCam3RemTime.Text = "Remaining:";
            // 
            // labelCam3Status
            // 
            this.labelCam3Status.AutoSize = true;
            this.labelCam3Status.Location = new System.Drawing.Point(6, 118);
            this.labelCam3Status.Name = "labelCam3Status";
            this.labelCam3Status.Size = new System.Drawing.Size(40, 13);
            this.labelCam3Status.TabIndex = 22;
            this.labelCam3Status.Text = "Status:";
            // 
            // labelCam3CoolerPwr
            // 
            this.labelCam3CoolerPwr.AutoSize = true;
            this.labelCam3CoolerPwr.Location = new System.Drawing.Point(6, 101);
            this.labelCam3CoolerPwr.Name = "labelCam3CoolerPwr";
            this.labelCam3CoolerPwr.Size = new System.Drawing.Size(73, 13);
            this.labelCam3CoolerPwr.TabIndex = 21;
            this.labelCam3CoolerPwr.Text = "Cooler Power:";
            // 
            // labelCam3BaseTemp
            // 
            this.labelCam3BaseTemp.AutoSize = true;
            this.labelCam3BaseTemp.Location = new System.Drawing.Point(6, 84);
            this.labelCam3BaseTemp.Name = "labelCam3BaseTemp";
            this.labelCam3BaseTemp.Size = new System.Drawing.Size(64, 13);
            this.labelCam3BaseTemp.TabIndex = 20;
            this.labelCam3BaseTemp.Text = "Base Temp:";
            // 
            // labelCam3CcdTemp
            // 
            this.labelCam3CcdTemp.AutoSize = true;
            this.labelCam3CcdTemp.Location = new System.Drawing.Point(6, 67);
            this.labelCam3CcdTemp.Name = "labelCam3CcdTemp";
            this.labelCam3CcdTemp.Size = new System.Drawing.Size(62, 13);
            this.labelCam3CcdTemp.TabIndex = 19;
            this.labelCam3CcdTemp.Text = "CCD Temp:";
            // 
            // labelCam3Filter
            // 
            this.labelCam3Filter.AutoSize = true;
            this.labelCam3Filter.Location = new System.Drawing.Point(6, 50);
            this.labelCam3Filter.Name = "labelCam3Filter";
            this.labelCam3Filter.Size = new System.Drawing.Size(32, 13);
            this.labelCam3Filter.TabIndex = 18;
            this.labelCam3Filter.Text = "Filter:";
            // 
            // labelCam3Sn
            // 
            this.labelCam3Sn.AutoSize = true;
            this.labelCam3Sn.Location = new System.Drawing.Point(6, 33);
            this.labelCam3Sn.Name = "labelCam3Sn";
            this.labelCam3Sn.Size = new System.Drawing.Size(61, 13);
            this.labelCam3Sn.TabIndex = 17;
            this.labelCam3Sn.Text = "Serial Num:";
            // 
            // labelCam3Model
            // 
            this.labelCam3Model.AutoSize = true;
            this.labelCam3Model.Location = new System.Drawing.Point(6, 16);
            this.labelCam3Model.Name = "labelCam3Model";
            this.labelCam3Model.Size = new System.Drawing.Size(39, 13);
            this.labelCam3Model.TabIndex = 16;
            this.labelCam3Model.Text = "Model:";
            // 
            // radioButtonViewCam3
            // 
            this.radioButtonViewCam3.AutoSize = true;
            this.radioButtonViewCam3.Enabled = false;
            this.radioButtonViewCam3.Location = new System.Drawing.Point(197, 62);
            this.radioButtonViewCam3.Name = "radioButtonViewCam3";
            this.radioButtonViewCam3.Size = new System.Drawing.Size(96, 17);
            this.radioButtonViewCam3.TabIndex = 3;
            this.radioButtonViewCam3.Text = "View Camera 3";
            this.radioButtonViewCam3.UseVisualStyleBackColor = true;
            // 
            // radioButtonViewCam2
            // 
            this.radioButtonViewCam2.AutoSize = true;
            this.radioButtonViewCam2.Enabled = false;
            this.radioButtonViewCam2.Location = new System.Drawing.Point(197, 39);
            this.radioButtonViewCam2.Name = "radioButtonViewCam2";
            this.radioButtonViewCam2.Size = new System.Drawing.Size(96, 17);
            this.radioButtonViewCam2.TabIndex = 2;
            this.radioButtonViewCam2.Text = "View Camera 2";
            this.radioButtonViewCam2.UseVisualStyleBackColor = true;
            // 
            // radioButtonViewCam1
            // 
            this.radioButtonViewCam1.AutoSize = true;
            this.radioButtonViewCam1.Checked = true;
            this.radioButtonViewCam1.Enabled = false;
            this.radioButtonViewCam1.Location = new System.Drawing.Point(197, 16);
            this.radioButtonViewCam1.Name = "radioButtonViewCam1";
            this.radioButtonViewCam1.Size = new System.Drawing.Size(96, 17);
            this.radioButtonViewCam1.TabIndex = 1;
            this.radioButtonViewCam1.TabStop = true;
            this.radioButtonViewCam1.Text = "View Camera 1";
            this.radioButtonViewCam1.UseVisualStyleBackColor = true;
            // 
            // groupBoxSurvey
            // 
            this.groupBoxSurvey.Controls.Add(this.radioButtonViewCam3);
            this.groupBoxSurvey.Controls.Add(this.buttonSurveyStop);
            this.groupBoxSurvey.Controls.Add(this.radioButtonViewCam2);
            this.groupBoxSurvey.Controls.Add(this.buttonSurveyStart);
            this.groupBoxSurvey.Controls.Add(this.radioButtonViewCam1);
            this.groupBoxSurvey.Controls.Add(this.numericUpDownSequence);
            this.groupBoxSurvey.Controls.Add(this.labelSequence);
            this.groupBoxSurvey.Controls.Add(this.numericUpDownExpTime);
            this.groupBoxSurvey.Controls.Add(this.labelExpTime);
            this.groupBoxSurvey.Controls.Add(this.comboBoxImgType);
            this.groupBoxSurvey.Controls.Add(this.labelImgType);
            this.groupBoxSurvey.Location = new System.Drawing.Point(666, 188);
            this.groupBoxSurvey.Name = "groupBoxSurvey";
            this.groupBoxSurvey.Size = new System.Drawing.Size(306, 136);
            this.groupBoxSurvey.TabIndex = 10;
            this.groupBoxSurvey.TabStop = false;
            this.groupBoxSurvey.Text = "Survey";
            // 
            // buttonSurveyStop
            // 
            this.buttonSurveyStop.Location = new System.Drawing.Point(97, 106);
            this.buttonSurveyStop.Name = "buttonSurveyStop";
            this.buttonSurveyStop.Size = new System.Drawing.Size(75, 23);
            this.buttonSurveyStop.TabIndex = 7;
            this.buttonSurveyStop.Text = "Stop";
            this.buttonSurveyStop.UseVisualStyleBackColor = true;
            this.buttonSurveyStop.Click += new System.EventHandler(this.ButtonSurveyStop_Click);
            // 
            // buttonSurveyStart
            // 
            this.buttonSurveyStart.Location = new System.Drawing.Point(9, 106);
            this.buttonSurveyStart.Name = "buttonSurveyStart";
            this.buttonSurveyStart.Size = new System.Drawing.Size(75, 23);
            this.buttonSurveyStart.TabIndex = 6;
            this.buttonSurveyStart.Text = "Start";
            this.buttonSurveyStart.UseVisualStyleBackColor = true;
            this.buttonSurveyStart.Click += new System.EventHandler(this.ButtonSurveyStart_Click);
            // 
            // numericUpDownSequence
            // 
            this.numericUpDownSequence.Location = new System.Drawing.Point(107, 80);
            this.numericUpDownSequence.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.numericUpDownSequence.Name = "numericUpDownSequence";
            this.numericUpDownSequence.Size = new System.Drawing.Size(65, 20);
            this.numericUpDownSequence.TabIndex = 5;
            this.numericUpDownSequence.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            // 
            // labelSequence
            // 
            this.labelSequence.AutoSize = true;
            this.labelSequence.Location = new System.Drawing.Point(6, 80);
            this.labelSequence.Name = "labelSequence";
            this.labelSequence.Size = new System.Drawing.Size(59, 13);
            this.labelSequence.TabIndex = 4;
            this.labelSequence.Text = "Sequence:";
            // 
            // numericUpDownExpTime
            // 
            this.numericUpDownExpTime.Location = new System.Drawing.Point(107, 50);
            this.numericUpDownExpTime.Name = "numericUpDownExpTime";
            this.numericUpDownExpTime.Size = new System.Drawing.Size(65, 20);
            this.numericUpDownExpTime.TabIndex = 3;
            // 
            // labelExpTime
            // 
            this.labelExpTime.AutoSize = true;
            this.labelExpTime.Location = new System.Drawing.Point(6, 50);
            this.labelExpTime.Name = "labelExpTime";
            this.labelExpTime.Size = new System.Drawing.Size(80, 13);
            this.labelExpTime.TabIndex = 2;
            this.labelExpTime.Text = "Exposure Time:";
            // 
            // comboBoxImgType
            // 
            this.comboBoxImgType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxImgType.FormattingEnabled = true;
            this.comboBoxImgType.Items.AddRange(new object[] {
            "Object",
            "Bias",
            "Dark",
            "Flat",
            "Test"});
            this.comboBoxImgType.Location = new System.Drawing.Point(107, 17);
            this.comboBoxImgType.Name = "comboBoxImgType";
            this.comboBoxImgType.Size = new System.Drawing.Size(65, 21);
            this.comboBoxImgType.TabIndex = 1;
            // 
            // labelImgType
            // 
            this.labelImgType.AutoSize = true;
            this.labelImgType.Location = new System.Drawing.Point(6, 20);
            this.labelImgType.Name = "labelImgType";
            this.labelImgType.Size = new System.Drawing.Size(66, 13);
            this.labelImgType.TabIndex = 0;
            this.labelImgType.Text = "Image Type:";
            // 
            // testDonToolStripMenuItem
            // 
            this.testDonToolStripMenuItem.Name = "testDonToolStripMenuItem";
            this.testDonToolStripMenuItem.Size = new System.Drawing.Size(209, 22);
            this.testDonToolStripMenuItem.Text = "Test Don";
            this.testDonToolStripMenuItem.Click += new System.EventHandler(this.SocketToolStripMenuItem_Click);
            // 
            // reconnectSocketsToolStripMenuItem1
            // 
            this.reconnectSocketsToolStripMenuItem1.Name = "reconnectSocketsToolStripMenuItem1";
            this.reconnectSocketsToolStripMenuItem1.Size = new System.Drawing.Size(180, 22);
            this.reconnectSocketsToolStripMenuItem1.Text = "Reconnect sockets";
            this.reconnectSocketsToolStripMenuItem1.Click += new System.EventHandler(this.ReconnectSocketsToolStripMenuItem_Click);
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1140, 531);
            this.Controls.Add(this.groupBoxSurvey);
            this.Controls.Add(this.groupBoxCam3);
            this.Controls.Add(this.groupBoxCam2);
            this.Controls.Add(this.groupBoxCam1);
            this.Controls.Add(this.groupBoxImageAnalysis);
            this.Controls.Add(this.groupBoxLogs);
            this.Controls.Add(this.statusStrip);
            this.Controls.Add(this.menuStrip);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MainMenuStrip = this.menuStrip;
            this.MaximizeBox = false;
            this.Name = "MainForm";
            this.Text = "RoboPhot Cameras Controls";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.MainForm_FormClosing);
            this.statusStrip.ResumeLayout(false);
            this.statusStrip.PerformLayout();
            this.menuStrip.ResumeLayout(false);
            this.menuStrip.PerformLayout();
            this.groupBoxLogs.ResumeLayout(false);
            this.panelFitsImage.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxFits)).EndInit();
            this.groupBoxImageAnalysis.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxProfile)).EndInit();
            this.groupBoxCam1.ResumeLayout(false);
            this.groupBoxCam1.PerformLayout();
            this.groupBoxCam2.ResumeLayout(false);
            this.groupBoxCam2.PerformLayout();
            this.groupBoxCam3.ResumeLayout(false);
            this.groupBoxCam3.PerformLayout();
            this.groupBoxSurvey.ResumeLayout(false);
            this.groupBoxSurvey.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownSequence)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownExpTime)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem1;

        #endregion

        private System.Windows.Forms.StatusStrip statusStrip;
        private System.Windows.Forms.ToolStripStatusLabel tSStatusClock;
        private System.Windows.Forms.MenuStrip menuStrip;
        private System.Windows.Forms.ToolStripMenuItem launchToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem findCamerasToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem debugToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem testDLLlibrariesToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem loadTestImageToolStripMenuItem;
        private System.Windows.Forms.GroupBox groupBoxLogs;
        private System.Windows.Forms.Timer timerClock;
        private System.Windows.Forms.ListBox listBoxLogs;
        private System.Windows.Forms.Button buttonLogsSave;
        private System.Windows.Forms.Button buttonLogsClear;
        private System.Windows.Forms.ToolStripMenuItem optionsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem settingsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem loadConfigToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem saveConfigToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem restoreDefaultConfigFileToolStripMenuItem;
        private System.Windows.Forms.SaveFileDialog saveFileDialogConfig;
        private System.Windows.Forms.OpenFileDialog openFileDialogConfig;
        private System.Windows.Forms.Panel panelFitsImage;
        private System.Windows.Forms.PictureBox pictureBoxFits;
        private System.Windows.Forms.GroupBox groupBoxImageAnalysis;
        private System.Windows.Forms.PictureBox pictureBoxProfile;
        private System.Windows.Forms.ToolStripMenuItem focusToolStripMenuItem;
        private System.Windows.Forms.GroupBox groupBoxCam1;
        private System.Windows.Forms.GroupBox groupBoxCam2;
        private System.Windows.Forms.GroupBox groupBoxCam3;
        private System.Windows.Forms.Label labelCam1Filter;
        private System.Windows.Forms.Label labelCam1Sn;
        private System.Windows.Forms.Label labelCam1Model;
        private System.Windows.Forms.Label labelCam1RemTime;
        private System.Windows.Forms.Label labelCam1Status;
        private System.Windows.Forms.Label labelCam1CoolerPwr;
        private System.Windows.Forms.Label labelCam1BaseTemp;
        private System.Windows.Forms.Label labelCam1CcdTemp;
        private System.Windows.Forms.Label labelCam2RemTime;
        private System.Windows.Forms.Label labelCam2Status;
        private System.Windows.Forms.Label labelCam2CoolerPwr;
        private System.Windows.Forms.Label labelCam2BaseTemp;
        private System.Windows.Forms.Label labelCam2CcdTemp;
        private System.Windows.Forms.Label labelCam2Filter;
        private System.Windows.Forms.Label labelCam2Sn;
        private System.Windows.Forms.Label labelCam2Model;
        private System.Windows.Forms.Label labelCam3RemTime;
        private System.Windows.Forms.Label labelCam3Status;
        private System.Windows.Forms.Label labelCam3CoolerPwr;
        private System.Windows.Forms.Label labelCam3BaseTemp;
        private System.Windows.Forms.Label labelCam3CcdTemp;
        private System.Windows.Forms.Label labelCam3Filter;
        private System.Windows.Forms.Label labelCam3Sn;
        private System.Windows.Forms.Label labelCam3Model;
        private System.Windows.Forms.ToolStripMenuItem updateCamerasSettingsToolStripMenuItem;
        private System.Windows.Forms.RadioButton radioButtonViewCam3;
        private System.Windows.Forms.RadioButton radioButtonViewCam2;
        private System.Windows.Forms.RadioButton radioButtonViewCam1;
        private System.Windows.Forms.GroupBox groupBoxSurvey;
        private System.Windows.Forms.NumericUpDown numericUpDownSequence;
        private System.Windows.Forms.Label labelSequence;
        private System.Windows.Forms.NumericUpDown numericUpDownExpTime;
        private System.Windows.Forms.Label labelExpTime;
        private System.Windows.Forms.Label labelImgType;
        private System.Windows.Forms.Button buttonSurveyStop;
        private System.Windows.Forms.Button buttonSurveyStart;
        private System.Windows.Forms.ComboBox comboBoxImgType;
        private System.Windows.Forms.ToolStripMenuItem reconnectSocketsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem testDonToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem reconnectSocketsToolStripMenuItem1;
    }
}

