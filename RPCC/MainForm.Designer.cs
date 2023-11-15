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
            this.dataGridViewTasker = new System.Windows.Forms.DataGridView();
            this.statusStrip = new System.Windows.Forms.StatusStrip();
            this.tSStatusClock = new System.Windows.Forms.ToolStripStatusLabel();
            this.menuStrip = new System.Windows.Forms.MenuStrip();
            this.launchToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.findCamerasToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.findFocusToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.reconnectSocketToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.reconnectMeteoDomeToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.reconnectDonutsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.reconnectSiTechExeToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.reconnectAllToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.optionsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.settingsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.updateCamerasSettingsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.loadConfigToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.saveConfigToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.debugToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.restoreDefaultConfigFileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.groupBoxLogs = new System.Windows.Forms.GroupBox();
            this.listBoxLogs = new System.Windows.Forms.ListBox();
            this.contextMenuStripLogs = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.clearToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.saveToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.timerUi = new System.Windows.Forms.Timer(this.components);
            this.saveFileDialogConfig = new System.Windows.Forms.SaveFileDialog();
            this.openFileDialogConfig = new System.Windows.Forms.OpenFileDialog();
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
            this.groupBoxSurvey = new System.Windows.Forms.GroupBox();
            this.checkBoxHead = new System.Windows.Forms.CheckBox();
            this.buttonSurveyStop = new System.Windows.Forms.Button();
            this.tabControlMain = new System.Windows.Forms.TabControl();
            this.tabPageInfo = new System.Windows.Forms.TabPage();
            this.tabPageTasks = new System.Windows.Forms.TabPage();
            this.dataGridTasks = new System.Windows.Forms.DataGrid();
            this.tabPageCams = new System.Windows.Forms.TabPage();
            this.groupBoxInfo = new System.Windows.Forms.GroupBox();
            this.labelFocusPos = new System.Windows.Forms.Label();
            this.labelEndSwitch = new System.Windows.Forms.Label();
            this.groupBoxFocusSettings = new System.Windows.Forms.GroupBox();
            this.buttonSetZeroPos = new System.Windows.Forms.Button();
            this.radioButtonRunSlow = new System.Windows.Forms.RadioButton();
            this.radioButtonRunFast = new System.Windows.Forms.RadioButton();
            this.buttonRun = new System.Windows.Forms.Button();
            this.buttonRunStop = new System.Windows.Forms.Button();
            this.numericUpDownRun = new System.Windows.Forms.NumericUpDown();
            this.groupBoxAutoFocus = new System.Windows.Forms.GroupBox();
            this.checkBoxGoZenith = new System.Windows.Forms.CheckBox();
            this.numericUpDownSetDefoc = new System.Windows.Forms.NumericUpDown();
            this.labelSetDefocus = new System.Windows.Forms.Label();
            this.checkBoxAutoFocus = new System.Windows.Forms.CheckBox();
            this.tabPageImages = new System.Windows.Forms.TabPage();
            this.groupBoxImage3 = new System.Windows.Forms.GroupBox();
            this.pictureBoxProfile3 = new System.Windows.Forms.PictureBox();
            this.panelImage3 = new System.Windows.Forms.Panel();
            this.pictureBoxImage3 = new System.Windows.Forms.PictureBox();
            this.groupBoxImage2 = new System.Windows.Forms.GroupBox();
            this.pictureBoxProfile2 = new System.Windows.Forms.PictureBox();
            this.panelImage2 = new System.Windows.Forms.Panel();
            this.pictureBoxImage2 = new System.Windows.Forms.PictureBox();
            this.groupBoxImage1 = new System.Windows.Forms.GroupBox();
            this.pictureBoxProfile1 = new System.Windows.Forms.PictureBox();
            this.panelImage1 = new System.Windows.Forms.Panel();
            this.pictureBoxImage1 = new System.Windows.Forms.PictureBox();
            this.contextMenuStripTasker = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.addToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.dataGridTextBoxColumn1 = new System.Windows.Forms.DataGridTextBoxColumn();
            this.dataGridTextBoxColumn2 = new System.Windows.Forms.DataGridTextBoxColumn();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridViewTasker)).BeginInit();
            this.statusStrip.SuspendLayout();
            this.menuStrip.SuspendLayout();
            this.groupBoxLogs.SuspendLayout();
            this.contextMenuStripLogs.SuspendLayout();
            this.groupBoxCam1.SuspendLayout();
            this.groupBoxCam2.SuspendLayout();
            this.groupBoxCam3.SuspendLayout();
            this.groupBoxSurvey.SuspendLayout();
            this.tabControlMain.SuspendLayout();
            this.tabPageInfo.SuspendLayout();
            this.tabPageTasks.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridTasks)).BeginInit();
            this.tabPageCams.SuspendLayout();
            this.groupBoxInfo.SuspendLayout();
            this.groupBoxFocusSettings.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownRun)).BeginInit();
            this.groupBoxAutoFocus.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownSetDefoc)).BeginInit();
            this.tabPageImages.SuspendLayout();
            this.groupBoxImage3.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxProfile3)).BeginInit();
            this.panelImage3.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxImage3)).BeginInit();
            this.groupBoxImage2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxProfile2)).BeginInit();
            this.panelImage2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxImage2)).BeginInit();
            this.groupBoxImage1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxProfile1)).BeginInit();
            this.panelImage1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxImage1)).BeginInit();
            this.contextMenuStripTasker.SuspendLayout();
            this.SuspendLayout();
            // 
            // dataGridViewTasker
            // 
            this.dataGridViewTasker.BackgroundColor = System.Drawing.SystemColors.GradientInactiveCaption;
            this.dataGridViewTasker.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.dataGridViewTasker.ClipboardCopyMode = System.Windows.Forms.DataGridViewClipboardCopyMode.EnableWithoutHeaderText;
            this.dataGridViewTasker.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridViewTasker.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dataGridViewTasker.GridColor = System.Drawing.SystemColors.ActiveBorder;
            this.dataGridViewTasker.Location = new System.Drawing.Point(3, 3);
            this.dataGridViewTasker.Name = "dataGridViewTasker";
            this.dataGridViewTasker.RowHeadersVisible = false;
            this.dataGridViewTasker.Size = new System.Drawing.Size(1126, 493);
            this.dataGridViewTasker.TabIndex = 1;
            this.dataGridViewTasker.CellMouseDoubleClick += new System.Windows.Forms.DataGridViewCellMouseEventHandler(this.DataGridViewTasker_CellMouseDoubleClick);
            // 
            // statusStrip
            // 
            this.statusStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tSStatusClock});
            this.statusStrip.Location = new System.Drawing.Point(0, 549);
            this.statusStrip.Name = "statusStrip";
            this.statusStrip.Size = new System.Drawing.Size(1140, 22);
            this.statusStrip.SizingGrip = false;
            this.statusStrip.TabIndex = 0;
            this.statusStrip.Text = "statusStrip";
            // 
            // tSStatusClock
            // 
            this.tSStatusClock.Name = "tSStatusClock";
            this.tSStatusClock.Size = new System.Drawing.Size(143, 17);
            this.tSStatusClock.Text = "yyyy-MM-ddTHH-mm-ss";
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
            this.findFocusToolStripMenuItem,
            this.reconnectSocketToolStripMenuItem});
            this.launchToolStripMenuItem.Name = "launchToolStripMenuItem";
            this.launchToolStripMenuItem.Size = new System.Drawing.Size(58, 20);
            this.launchToolStripMenuItem.Text = "Launch";
            // 
            // findCamerasToolStripMenuItem
            // 
            this.findCamerasToolStripMenuItem.Name = "findCamerasToolStripMenuItem";
            this.findCamerasToolStripMenuItem.Size = new System.Drawing.Size(167, 22);
            this.findCamerasToolStripMenuItem.Text = "Find cameras";
            this.findCamerasToolStripMenuItem.Click += new System.EventHandler(this.FindCamerasToolStripMenuItem_Click);
            // 
            // findFocusToolStripMenuItem
            // 
            this.findFocusToolStripMenuItem.Name = "findFocusToolStripMenuItem";
            this.findFocusToolStripMenuItem.Size = new System.Drawing.Size(167, 22);
            this.findFocusToolStripMenuItem.Text = "Find focus";
            this.findFocusToolStripMenuItem.Click += new System.EventHandler(this.FindFocusToolStripMenuItem_Click);
            // 
            // reconnectSocketToolStripMenuItem
            // 
            this.reconnectSocketToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.reconnectMeteoDomeToolStripMenuItem,
            this.reconnectDonutsToolStripMenuItem,
            this.reconnectSiTechExeToolStripMenuItem,
            this.toolStripSeparator1,
            this.reconnectAllToolStripMenuItem});
            this.reconnectSocketToolStripMenuItem.Name = "reconnectSocketToolStripMenuItem";
            this.reconnectSocketToolStripMenuItem.Size = new System.Drawing.Size(167, 22);
            this.reconnectSocketToolStripMenuItem.Text = "Reconnect socket";
            // 
            // reconnectMeteoDomeToolStripMenuItem
            // 
            this.reconnectMeteoDomeToolStripMenuItem.Name = "reconnectMeteoDomeToolStripMenuItem";
            this.reconnectMeteoDomeToolStripMenuItem.Size = new System.Drawing.Size(140, 22);
            this.reconnectMeteoDomeToolStripMenuItem.Text = "MeteoDome";
            this.reconnectMeteoDomeToolStripMenuItem.Click += new System.EventHandler(this.ReconnectMeteoDomeToolStripMenuItem_Click);
            // 
            // reconnectDonutsToolStripMenuItem
            // 
            this.reconnectDonutsToolStripMenuItem.Name = "reconnectDonutsToolStripMenuItem";
            this.reconnectDonutsToolStripMenuItem.Size = new System.Drawing.Size(140, 22);
            this.reconnectDonutsToolStripMenuItem.Text = "DONUTS";
            this.reconnectDonutsToolStripMenuItem.Click += new System.EventHandler(this.ReconnectDonutsToolStripMenuItem_Click);
            // 
            // reconnectSiTechExeToolStripMenuItem
            // 
            this.reconnectSiTechExeToolStripMenuItem.Name = "reconnectSiTechExeToolStripMenuItem";
            this.reconnectSiTechExeToolStripMenuItem.Size = new System.Drawing.Size(140, 22);
            this.reconnectSiTechExeToolStripMenuItem.Text = "SiTechExe";
            this.reconnectSiTechExeToolStripMenuItem.Click += new System.EventHandler(this.ReconnectSiTechExeToolStripMenuItem_Click);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(137, 6);
            // 
            // reconnectAllToolStripMenuItem
            // 
            this.reconnectAllToolStripMenuItem.Name = "reconnectAllToolStripMenuItem";
            this.reconnectAllToolStripMenuItem.Size = new System.Drawing.Size(140, 22);
            this.reconnectAllToolStripMenuItem.Text = "All";
            this.reconnectAllToolStripMenuItem.Click += new System.EventHandler(this.ReconnectAllToolStripMenuItem_Click);
            // 
            // optionsToolStripMenuItem
            // 
            this.optionsToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.settingsToolStripMenuItem,
            this.updateCamerasSettingsToolStripMenuItem,
            this.loadConfigToolStripMenuItem,
            this.saveConfigToolStripMenuItem});
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
            // debugToolStripMenuItem
            // 
            this.debugToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.restoreDefaultConfigFileToolStripMenuItem});
            this.debugToolStripMenuItem.Name = "debugToolStripMenuItem";
            this.debugToolStripMenuItem.Size = new System.Drawing.Size(54, 20);
            this.debugToolStripMenuItem.Text = "Debug";
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
            this.groupBoxLogs.Controls.Add(this.listBoxLogs);
            this.groupBoxLogs.Dock = System.Windows.Forms.DockStyle.Left;
            this.groupBoxLogs.Location = new System.Drawing.Point(3, 3);
            this.groupBoxLogs.Name = "groupBoxLogs";
            this.groupBoxLogs.Size = new System.Drawing.Size(1121, 493);
            this.groupBoxLogs.TabIndex = 2;
            this.groupBoxLogs.TabStop = false;
            this.groupBoxLogs.Text = "Logs";
            // 
            // listBoxLogs
            // 
            this.listBoxLogs.ContextMenuStrip = this.contextMenuStripLogs;
            this.listBoxLogs.Dock = System.Windows.Forms.DockStyle.Fill;
            this.listBoxLogs.FormattingEnabled = true;
            this.listBoxLogs.HorizontalScrollbar = true;
            this.listBoxLogs.Location = new System.Drawing.Point(3, 16);
            this.listBoxLogs.Name = "listBoxLogs";
            this.listBoxLogs.Size = new System.Drawing.Size(1115, 474);
            this.listBoxLogs.TabIndex = 0;
            this.listBoxLogs.DoubleClick += new System.EventHandler(this.ListBoxLogs_DoubleClick);
            // 
            // contextMenuStripLogs
            // 
            this.contextMenuStripLogs.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.clearToolStripMenuItem,
            this.saveToolStripMenuItem});
            this.contextMenuStripLogs.Name = "contextMenuStrip1";
            this.contextMenuStripLogs.Size = new System.Drawing.Size(102, 48);
            // 
            // clearToolStripMenuItem
            // 
            this.clearToolStripMenuItem.Name = "clearToolStripMenuItem";
            this.clearToolStripMenuItem.Size = new System.Drawing.Size(101, 22);
            this.clearToolStripMenuItem.Text = "Clear";
            this.clearToolStripMenuItem.Click += new System.EventHandler(this.ClearToolStripMenuItem_Click);
            // 
            // saveToolStripMenuItem
            // 
            this.saveToolStripMenuItem.Name = "saveToolStripMenuItem";
            this.saveToolStripMenuItem.Size = new System.Drawing.Size(101, 22);
            this.saveToolStripMenuItem.Text = "Save";
            this.saveToolStripMenuItem.Click += new System.EventHandler(this.SaveToolStripMenuItem_Click);
            // 
            // timerUi
            // 
            this.timerUi.Interval = 1000;
            this.timerUi.Tick += new System.EventHandler(this.TimerUiUpdate);
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
            this.groupBoxCam1.Location = new System.Drawing.Point(8, 6);
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
            this.groupBoxCam2.Location = new System.Drawing.Point(164, 6);
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
            this.groupBoxCam3.Location = new System.Drawing.Point(320, 6);
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
            // groupBoxSurvey
            // 
            this.groupBoxSurvey.Controls.Add(this.checkBoxHead);
            this.groupBoxSurvey.Controls.Add(this.buttonSurveyStop);
            this.groupBoxSurvey.Location = new System.Drawing.Point(8, 167);
            this.groupBoxSurvey.Name = "groupBoxSurvey";
            this.groupBoxSurvey.Size = new System.Drawing.Size(235, 91);
            this.groupBoxSurvey.TabIndex = 10;
            this.groupBoxSurvey.TabStop = false;
            this.groupBoxSurvey.Text = "Survey";
            // 
            // checkBoxHead
            // 
            this.checkBoxHead.Checked = true;
            this.checkBoxHead.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBoxHead.Location = new System.Drawing.Point(9, 21);
            this.checkBoxHead.Name = "checkBoxHead";
            this.checkBoxHead.Size = new System.Drawing.Size(70, 25);
            this.checkBoxHead.TabIndex = 28;
            this.checkBoxHead.Text = "Head";
            this.checkBoxHead.UseVisualStyleBackColor = true;
            this.checkBoxHead.CheckedChanged += new System.EventHandler(this.CheckBoxHead_CheckedChanged);
            // 
            // buttonSurveyStop
            // 
            this.buttonSurveyStop.Location = new System.Drawing.Point(154, 19);
            this.buttonSurveyStop.Name = "buttonSurveyStop";
            this.buttonSurveyStop.Size = new System.Drawing.Size(75, 23);
            this.buttonSurveyStop.TabIndex = 7;
            this.buttonSurveyStop.Text = "Stop";
            this.buttonSurveyStop.UseVisualStyleBackColor = true;
            this.buttonSurveyStop.Click += new System.EventHandler(this.ButtonSurveyStop_Click);
            // 
            // tabControlMain
            // 
            this.tabControlMain.Controls.Add(this.tabPageInfo);
            this.tabControlMain.Controls.Add(this.tabPageTasks);
            this.tabControlMain.Controls.Add(this.tabPageCams);
            this.tabControlMain.Controls.Add(this.tabPageImages);
            this.tabControlMain.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabControlMain.Location = new System.Drawing.Point(0, 24);
            this.tabControlMain.Name = "tabControlMain";
            this.tabControlMain.SelectedIndex = 0;
            this.tabControlMain.Size = new System.Drawing.Size(1140, 525);
            this.tabControlMain.TabIndex = 3;
            // 
            // tabPageInfo
            // 
            this.tabPageInfo.Controls.Add(this.groupBoxLogs);
            this.tabPageInfo.Location = new System.Drawing.Point(4, 22);
            this.tabPageInfo.Name = "tabPageInfo";
            this.tabPageInfo.Padding = new System.Windows.Forms.Padding(3);
            this.tabPageInfo.Size = new System.Drawing.Size(1132, 499);
            this.tabPageInfo.TabIndex = 0;
            this.tabPageInfo.Text = "Info";
            this.tabPageInfo.UseVisualStyleBackColor = true;
            // 
            // tabPageTasks
            // 
            this.tabPageTasks.AutoScroll = true;
            this.tabPageTasks.Controls.Add(this.dataGridViewTasker);
            this.tabPageTasks.Controls.Add(this.dataGridTasks);
            this.tabPageTasks.Location = new System.Drawing.Point(4, 22);
            this.tabPageTasks.Name = "tabPageTasks";
            this.tabPageTasks.Padding = new System.Windows.Forms.Padding(3);
            this.tabPageTasks.Size = new System.Drawing.Size(1132, 499);
            this.tabPageTasks.TabIndex = 1;
            this.tabPageTasks.Text = "Tasks";
            this.tabPageTasks.UseVisualStyleBackColor = true;
            // 
            // dataGridTasks
            // 
            this.dataGridTasks.DataMember = "";
            this.dataGridTasks.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dataGridTasks.HeaderForeColor = System.Drawing.SystemColors.ControlText;
            this.dataGridTasks.Location = new System.Drawing.Point(3, 3);
            this.dataGridTasks.Name = "dataGridTasks";
            this.dataGridTasks.Size = new System.Drawing.Size(1126, 493);
            this.dataGridTasks.TabIndex = 0;
            // 
            // tabPageCams
            // 
            this.tabPageCams.Controls.Add(this.groupBoxInfo);
            this.tabPageCams.Controls.Add(this.groupBoxFocusSettings);
            this.tabPageCams.Controls.Add(this.groupBoxAutoFocus);
            this.tabPageCams.Controls.Add(this.groupBoxCam1);
            this.tabPageCams.Controls.Add(this.groupBoxSurvey);
            this.tabPageCams.Controls.Add(this.groupBoxCam2);
            this.tabPageCams.Controls.Add(this.groupBoxCam3);
            this.tabPageCams.Location = new System.Drawing.Point(4, 22);
            this.tabPageCams.Name = "tabPageCams";
            this.tabPageCams.Padding = new System.Windows.Forms.Padding(3);
            this.tabPageCams.Size = new System.Drawing.Size(1132, 499);
            this.tabPageCams.TabIndex = 2;
            this.tabPageCams.Text = "Cams&Focus";
            this.tabPageCams.UseVisualStyleBackColor = true;
            // 
            // groupBoxInfo
            // 
            this.groupBoxInfo.Controls.Add(this.labelFocusPos);
            this.groupBoxInfo.Controls.Add(this.labelEndSwitch);
            this.groupBoxInfo.Location = new System.Drawing.Point(477, 207);
            this.groupBoxInfo.Name = "groupBoxInfo";
            this.groupBoxInfo.Size = new System.Drawing.Size(324, 51);
            this.groupBoxInfo.TabIndex = 21;
            this.groupBoxInfo.TabStop = false;
            this.groupBoxInfo.Text = "Info";
            // 
            // labelFocusPos
            // 
            this.labelFocusPos.AutoSize = true;
            this.labelFocusPos.Location = new System.Drawing.Point(6, 22);
            this.labelFocusPos.Name = "labelFocusPos";
            this.labelFocusPos.Size = new System.Drawing.Size(78, 13);
            this.labelFocusPos.TabIndex = 4;
            this.labelFocusPos.Text = "Focus position:";
            // 
            // labelEndSwitch
            // 
            this.labelEndSwitch.AutoSize = true;
            this.labelEndSwitch.Location = new System.Drawing.Point(181, 22);
            this.labelEndSwitch.Name = "labelEndSwitch";
            this.labelEndSwitch.Size = new System.Drawing.Size(93, 13);
            this.labelEndSwitch.TabIndex = 13;
            this.labelEndSwitch.Text = "Endswitch: unjoint";
            // 
            // groupBoxFocusSettings
            // 
            this.groupBoxFocusSettings.Controls.Add(this.buttonSetZeroPos);
            this.groupBoxFocusSettings.Controls.Add(this.radioButtonRunSlow);
            this.groupBoxFocusSettings.Controls.Add(this.radioButtonRunFast);
            this.groupBoxFocusSettings.Controls.Add(this.buttonRun);
            this.groupBoxFocusSettings.Controls.Add(this.buttonRunStop);
            this.groupBoxFocusSettings.Controls.Add(this.numericUpDownRun);
            this.groupBoxFocusSettings.Location = new System.Drawing.Point(476, 6);
            this.groupBoxFocusSettings.Name = "groupBoxFocusSettings";
            this.groupBoxFocusSettings.Size = new System.Drawing.Size(324, 102);
            this.groupBoxFocusSettings.TabIndex = 20;
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
            this.buttonSetZeroPos.Click += new System.EventHandler(this.ButtonSetZeroPos_Click);
            // 
            // radioButtonRunSlow
            // 
            this.radioButtonRunSlow.AutoSize = true;
            this.radioButtonRunSlow.Location = new System.Drawing.Point(71, 25);
            this.radioButtonRunSlow.Name = "radioButtonRunSlow";
            this.radioButtonRunSlow.Size = new System.Drawing.Size(48, 17);
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
            this.radioButtonRunFast.Size = new System.Drawing.Size(45, 17);
            this.radioButtonRunFast.TabIndex = 19;
            this.radioButtonRunFast.TabStop = true;
            this.radioButtonRunFast.Text = "Fast";
            this.radioButtonRunFast.UseVisualStyleBackColor = true;
            // 
            // buttonRun
            // 
            this.buttonRun.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonRun.Enabled = false;
            this.buttonRun.Location = new System.Drawing.Point(244, 25);
            this.buttonRun.Name = "buttonRun";
            this.buttonRun.Size = new System.Drawing.Size(74, 29);
            this.buttonRun.TabIndex = 18;
            this.buttonRun.Text = "Run";
            this.buttonRun.UseVisualStyleBackColor = true;
            this.buttonRun.Click += new System.EventHandler(this.ButtonRun_Click);
            // 
            // buttonRunStop
            // 
            this.buttonRunStop.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonRunStop.Location = new System.Drawing.Point(244, 60);
            this.buttonRunStop.Name = "buttonRunStop";
            this.buttonRunStop.Size = new System.Drawing.Size(74, 29);
            this.buttonRunStop.TabIndex = 17;
            this.buttonRunStop.Text = "Stop";
            this.buttonRunStop.UseVisualStyleBackColor = true;
            this.buttonRunStop.Click += new System.EventHandler(this.ButtonRunStop_Click);
            // 
            // numericUpDownRun
            // 
            this.numericUpDownRun.Enabled = false;
            this.numericUpDownRun.Increment = new decimal(new int[] {
            100,
            0,
            0,
            0});
            this.numericUpDownRun.Location = new System.Drawing.Point(138, 25);
            this.numericUpDownRun.Maximum = new decimal(new int[] {
            5000,
            0,
            0,
            0});
            this.numericUpDownRun.Minimum = new decimal(new int[] {
            5000,
            0,
            0,
            -2147483648});
            this.numericUpDownRun.Name = "numericUpDownRun";
            this.numericUpDownRun.Size = new System.Drawing.Size(75, 20);
            this.numericUpDownRun.TabIndex = 1;
            // 
            // groupBoxAutoFocus
            // 
            this.groupBoxAutoFocus.Controls.Add(this.checkBoxGoZenith);
            this.groupBoxAutoFocus.Controls.Add(this.numericUpDownSetDefoc);
            this.groupBoxAutoFocus.Controls.Add(this.labelSetDefocus);
            this.groupBoxAutoFocus.Controls.Add(this.checkBoxAutoFocus);
            this.groupBoxAutoFocus.Location = new System.Drawing.Point(476, 114);
            this.groupBoxAutoFocus.Name = "groupBoxAutoFocus";
            this.groupBoxAutoFocus.Size = new System.Drawing.Size(324, 87);
            this.groupBoxAutoFocus.TabIndex = 19;
            this.groupBoxAutoFocus.TabStop = false;
            this.groupBoxAutoFocus.Text = "Auto Focus";
            // 
            // checkBoxGoZenith
            // 
            this.checkBoxGoZenith.AutoSize = true;
            this.checkBoxGoZenith.Location = new System.Drawing.Point(181, 57);
            this.checkBoxGoZenith.Name = "checkBoxGoZenith";
            this.checkBoxGoZenith.Size = new System.Drawing.Size(98, 17);
            this.checkBoxGoZenith.TabIndex = 16;
            this.checkBoxGoZenith.Text = "Focus at zenith";
            this.checkBoxGoZenith.UseVisualStyleBackColor = true;
            this.checkBoxGoZenith.CheckedChanged += new System.EventHandler(this.CheckBoxGoZenith_CheckedChanged);
            // 
            // numericUpDownSetDefoc
            // 
            this.numericUpDownSetDefoc.Location = new System.Drawing.Point(242, 25);
            this.numericUpDownSetDefoc.Maximum = new decimal(new int[] {
            1500,
            0,
            0,
            0});
            this.numericUpDownSetDefoc.Minimum = new decimal(new int[] {
            1500,
            0,
            0,
            -2147483648});
            this.numericUpDownSetDefoc.Name = "numericUpDownSetDefoc";
            this.numericUpDownSetDefoc.Size = new System.Drawing.Size(76, 20);
            this.numericUpDownSetDefoc.TabIndex = 15;
            this.numericUpDownSetDefoc.ValueChanged += new System.EventHandler(this.NumericUpDownSetDefoc_ValueChanged);
            // 
            // labelSetDefocus
            // 
            this.labelSetDefocus.AutoSize = true;
            this.labelSetDefocus.Location = new System.Drawing.Point(6, 22);
            this.labelSetDefocus.Name = "labelSetDefocus";
            this.labelSetDefocus.Size = new System.Drawing.Size(98, 13);
            this.labelSetDefocus.TabIndex = 14;
            this.labelSetDefocus.Text = "Set defocus (steps)";
            // 
            // checkBoxAutoFocus
            // 
            this.checkBoxAutoFocus.AutoSize = true;
            this.checkBoxAutoFocus.Checked = true;
            this.checkBoxAutoFocus.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBoxAutoFocus.Location = new System.Drawing.Point(12, 57);
            this.checkBoxAutoFocus.Name = "checkBoxAutoFocus";
            this.checkBoxAutoFocus.Size = new System.Drawing.Size(77, 17);
            this.checkBoxAutoFocus.TabIndex = 12;
            this.checkBoxAutoFocus.Text = "Auto focus";
            this.checkBoxAutoFocus.UseVisualStyleBackColor = true;
            this.checkBoxAutoFocus.CheckedChanged += new System.EventHandler(this.CheckBoxAutoFocus_CheckedChanged);
            // 
            // tabPageImages
            // 
            this.tabPageImages.Controls.Add(this.groupBoxImage3);
            this.tabPageImages.Controls.Add(this.groupBoxImage2);
            this.tabPageImages.Controls.Add(this.groupBoxImage1);
            this.tabPageImages.Location = new System.Drawing.Point(4, 22);
            this.tabPageImages.Name = "tabPageImages";
            this.tabPageImages.Padding = new System.Windows.Forms.Padding(3);
            this.tabPageImages.Size = new System.Drawing.Size(1132, 499);
            this.tabPageImages.TabIndex = 3;
            this.tabPageImages.Text = "Images";
            this.tabPageImages.UseVisualStyleBackColor = true;
            // 
            // groupBoxImage3
            // 
            this.groupBoxImage3.Controls.Add(this.pictureBoxProfile3);
            this.groupBoxImage3.Controls.Add(this.panelImage3);
            this.groupBoxImage3.Dock = System.Windows.Forms.DockStyle.Left;
            this.groupBoxImage3.Location = new System.Drawing.Point(627, 3);
            this.groupBoxImage3.Name = "groupBoxImage3";
            this.groupBoxImage3.Size = new System.Drawing.Size(312, 493);
            this.groupBoxImage3.TabIndex = 25;
            this.groupBoxImage3.TabStop = false;
            this.groupBoxImage3.Text = "Camera 3 Image";
            // 
            // pictureBoxProfile3
            // 
            this.pictureBoxProfile3.BackColor = System.Drawing.SystemColors.Desktop;
            this.pictureBoxProfile3.Location = new System.Drawing.Point(6, 326);
            this.pictureBoxProfile3.Name = "pictureBoxProfile3";
            this.pictureBoxProfile3.Size = new System.Drawing.Size(300, 161);
            this.pictureBoxProfile3.TabIndex = 5;
            this.pictureBoxProfile3.TabStop = false;
            // 
            // panelImage3
            // 
            this.panelImage3.Controls.Add(this.pictureBoxImage3);
            this.panelImage3.Location = new System.Drawing.Point(6, 19);
            this.panelImage3.Name = "panelImage3";
            this.panelImage3.Size = new System.Drawing.Size(300, 300);
            this.panelImage3.TabIndex = 4;
            // 
            // pictureBoxImage3
            // 
            this.pictureBoxImage3.BackColor = System.Drawing.SystemColors.Desktop;
            this.pictureBoxImage3.Location = new System.Drawing.Point(0, 0);
            this.pictureBoxImage3.Name = "pictureBoxImage3";
            this.pictureBoxImage3.Size = new System.Drawing.Size(300, 300);
            this.pictureBoxImage3.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.pictureBoxImage3.TabIndex = 0;
            this.pictureBoxImage3.TabStop = false;
            // 
            // groupBoxImage2
            // 
            this.groupBoxImage2.Controls.Add(this.pictureBoxProfile2);
            this.groupBoxImage2.Controls.Add(this.panelImage2);
            this.groupBoxImage2.Dock = System.Windows.Forms.DockStyle.Left;
            this.groupBoxImage2.Location = new System.Drawing.Point(315, 3);
            this.groupBoxImage2.Name = "groupBoxImage2";
            this.groupBoxImage2.Size = new System.Drawing.Size(312, 493);
            this.groupBoxImage2.TabIndex = 24;
            this.groupBoxImage2.TabStop = false;
            this.groupBoxImage2.Text = "Camera 2 Image";
            // 
            // pictureBoxProfile2
            // 
            this.pictureBoxProfile2.BackColor = System.Drawing.SystemColors.Desktop;
            this.pictureBoxProfile2.Location = new System.Drawing.Point(6, 326);
            this.pictureBoxProfile2.Name = "pictureBoxProfile2";
            this.pictureBoxProfile2.Size = new System.Drawing.Size(300, 161);
            this.pictureBoxProfile2.TabIndex = 5;
            this.pictureBoxProfile2.TabStop = false;
            // 
            // panelImage2
            // 
            this.panelImage2.Controls.Add(this.pictureBoxImage2);
            this.panelImage2.Location = new System.Drawing.Point(6, 19);
            this.panelImage2.Name = "panelImage2";
            this.panelImage2.Size = new System.Drawing.Size(300, 300);
            this.panelImage2.TabIndex = 4;
            // 
            // pictureBoxImage2
            // 
            this.pictureBoxImage2.BackColor = System.Drawing.SystemColors.Desktop;
            this.pictureBoxImage2.Location = new System.Drawing.Point(0, 0);
            this.pictureBoxImage2.Name = "pictureBoxImage2";
            this.pictureBoxImage2.Size = new System.Drawing.Size(300, 300);
            this.pictureBoxImage2.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.pictureBoxImage2.TabIndex = 0;
            this.pictureBoxImage2.TabStop = false;
            // 
            // groupBoxImage1
            // 
            this.groupBoxImage1.Controls.Add(this.pictureBoxProfile1);
            this.groupBoxImage1.Controls.Add(this.panelImage1);
            this.groupBoxImage1.Dock = System.Windows.Forms.DockStyle.Left;
            this.groupBoxImage1.Location = new System.Drawing.Point(3, 3);
            this.groupBoxImage1.Name = "groupBoxImage1";
            this.groupBoxImage1.Size = new System.Drawing.Size(312, 493);
            this.groupBoxImage1.TabIndex = 23;
            this.groupBoxImage1.TabStop = false;
            this.groupBoxImage1.Text = "Camera 1 Image";
            // 
            // pictureBoxProfile1
            // 
            this.pictureBoxProfile1.BackColor = System.Drawing.SystemColors.Desktop;
            this.pictureBoxProfile1.Location = new System.Drawing.Point(6, 326);
            this.pictureBoxProfile1.Name = "pictureBoxProfile1";
            this.pictureBoxProfile1.Size = new System.Drawing.Size(300, 161);
            this.pictureBoxProfile1.TabIndex = 5;
            this.pictureBoxProfile1.TabStop = false;
            // 
            // panelImage1
            // 
            this.panelImage1.Controls.Add(this.pictureBoxImage1);
            this.panelImage1.Location = new System.Drawing.Point(6, 19);
            this.panelImage1.Name = "panelImage1";
            this.panelImage1.Size = new System.Drawing.Size(300, 300);
            this.panelImage1.TabIndex = 4;
            // 
            // pictureBoxImage1
            // 
            this.pictureBoxImage1.BackColor = System.Drawing.SystemColors.Desktop;
            this.pictureBoxImage1.Location = new System.Drawing.Point(0, 0);
            this.pictureBoxImage1.Name = "pictureBoxImage1";
            this.pictureBoxImage1.Size = new System.Drawing.Size(300, 300);
            this.pictureBoxImage1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.pictureBoxImage1.TabIndex = 0;
            this.pictureBoxImage1.TabStop = false;
            // 
            // contextMenuStripTasker
            // 
            this.contextMenuStripTasker.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.addToolStripMenuItem});
            this.contextMenuStripTasker.Name = "contextMenuStripTasker";
            this.contextMenuStripTasker.Size = new System.Drawing.Size(97, 26);
            // 
            // addToolStripMenuItem
            // 
            this.addToolStripMenuItem.Name = "addToolStripMenuItem";
            this.addToolStripMenuItem.Size = new System.Drawing.Size(96, 22);
            this.addToolStripMenuItem.Text = "Add";
            this.addToolStripMenuItem.Click += new System.EventHandler(this.AddToolStripMenuItem_Click);
            // 
            // dataGridTextBoxColumn1
            // 
            this.dataGridTextBoxColumn1.Format = "";
            this.dataGridTextBoxColumn1.FormatInfo = null;
            this.dataGridTextBoxColumn1.Width = -1;
            // 
            // dataGridTextBoxColumn2
            // 
            this.dataGridTextBoxColumn2.Format = "";
            this.dataGridTextBoxColumn2.FormatInfo = null;
            this.dataGridTextBoxColumn2.Width = -1;
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1140, 571);
            this.Controls.Add(this.tabControlMain);
            this.Controls.Add(this.statusStrip);
            this.Controls.Add(this.menuStrip);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MainMenuStrip = this.menuStrip;
            this.MaximizeBox = false;
            this.Name = "MainForm";
            this.Text = "RoboPhot Cameras Controls";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.MainForm_FormClosing);
            ((System.ComponentModel.ISupportInitialize)(this.dataGridViewTasker)).EndInit();
            this.statusStrip.ResumeLayout(false);
            this.statusStrip.PerformLayout();
            this.menuStrip.ResumeLayout(false);
            this.menuStrip.PerformLayout();
            this.groupBoxLogs.ResumeLayout(false);
            this.contextMenuStripLogs.ResumeLayout(false);
            this.groupBoxCam1.ResumeLayout(false);
            this.groupBoxCam1.PerformLayout();
            this.groupBoxCam2.ResumeLayout(false);
            this.groupBoxCam2.PerformLayout();
            this.groupBoxCam3.ResumeLayout(false);
            this.groupBoxCam3.PerformLayout();
            this.groupBoxSurvey.ResumeLayout(false);
            this.tabControlMain.ResumeLayout(false);
            this.tabPageInfo.ResumeLayout(false);
            this.tabPageTasks.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dataGridTasks)).EndInit();
            this.tabPageCams.ResumeLayout(false);
            this.groupBoxInfo.ResumeLayout(false);
            this.groupBoxInfo.PerformLayout();
            this.groupBoxFocusSettings.ResumeLayout(false);
            this.groupBoxFocusSettings.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownRun)).EndInit();
            this.groupBoxAutoFocus.ResumeLayout(false);
            this.groupBoxAutoFocus.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownSetDefoc)).EndInit();
            this.tabPageImages.ResumeLayout(false);
            this.groupBoxImage3.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxProfile3)).EndInit();
            this.panelImage3.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxImage3)).EndInit();
            this.groupBoxImage2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxProfile2)).EndInit();
            this.panelImage2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxImage2)).EndInit();
            this.groupBoxImage1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxProfile1)).EndInit();
            this.panelImage1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxImage1)).EndInit();
            this.contextMenuStripTasker.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        private System.Windows.Forms.CheckBox checkBoxHead;

        private System.Windows.Forms.DataGridView dataGridViewTasker;

        private System.Windows.Forms.ContextMenuStrip contextMenuStripTasker;

        private System.Windows.Forms.ContextMenuStrip contextMenuStripLogs;
        private System.Windows.Forms.ToolStripMenuItem addToolStripMenuItem;

        private System.Windows.Forms.DataGrid dataGridTasks;
        private System.Windows.Forms.DataGridTextBoxColumn dataGridTextBoxColumn1;
        private System.Windows.Forms.DataGridTextBoxColumn dataGridTextBoxColumn2;

        private System.Windows.Forms.GroupBox groupBoxInfo;
        private System.Windows.Forms.Label labelFocusPos;
        private System.Windows.Forms.Label labelEndSwitch;
        private System.Windows.Forms.GroupBox groupBoxFocusSettings;
        private System.Windows.Forms.Button buttonSetZeroPos;
        private System.Windows.Forms.RadioButton radioButtonRunSlow;
        private System.Windows.Forms.RadioButton radioButtonRunFast;
        private System.Windows.Forms.Button buttonRun;
        private System.Windows.Forms.Button buttonRunStop;
        private System.Windows.Forms.NumericUpDown numericUpDownRun;
        private System.Windows.Forms.GroupBox groupBoxAutoFocus;
        private System.Windows.Forms.CheckBox checkBoxGoZenith;
        private System.Windows.Forms.NumericUpDown numericUpDownSetDefoc;
        private System.Windows.Forms.Label labelSetDefocus;
        private System.Windows.Forms.CheckBox checkBoxAutoFocus;

        private System.Windows.Forms.ToolStripMenuItem clearToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem saveToolStripMenuItem;

        private System.Windows.Forms.TabPage tabPageCams;

        private System.Windows.Forms.TabControl tabControlMain;
        private System.Windows.Forms.TabPage tabPageInfo;
        private System.Windows.Forms.TabPage tabPageTasks;

        #endregion

        private System.Windows.Forms.StatusStrip statusStrip;
        private System.Windows.Forms.ToolStripStatusLabel tSStatusClock;
        private System.Windows.Forms.MenuStrip menuStrip;
        private System.Windows.Forms.ToolStripMenuItem launchToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem findCamerasToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem debugToolStripMenuItem;
        private System.Windows.Forms.GroupBox groupBoxLogs;
        private System.Windows.Forms.Timer timerUi;
        private System.Windows.Forms.ListBox listBoxLogs;
        private System.Windows.Forms.ToolStripMenuItem optionsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem settingsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem loadConfigToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem saveConfigToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem restoreDefaultConfigFileToolStripMenuItem;
        private System.Windows.Forms.SaveFileDialog saveFileDialogConfig;
        private System.Windows.Forms.OpenFileDialog openFileDialogConfig;
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
        private System.Windows.Forms.GroupBox groupBoxSurvey;
        private System.Windows.Forms.Button buttonSurveyStop;
        private System.Windows.Forms.ToolStripMenuItem findFocusToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem reconnectSocketToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem reconnectMeteoDomeToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem reconnectDonutsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem reconnectSiTechExeToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripMenuItem reconnectAllToolStripMenuItem;
        private System.Windows.Forms.TabPage tabPageImages;
        private System.Windows.Forms.GroupBox groupBoxImage3;
        private System.Windows.Forms.PictureBox pictureBoxProfile3;
        private System.Windows.Forms.Panel panelImage3;
        private System.Windows.Forms.PictureBox pictureBoxImage3;
        private System.Windows.Forms.GroupBox groupBoxImage2;
        private System.Windows.Forms.PictureBox pictureBoxProfile2;
        private System.Windows.Forms.Panel panelImage2;
        private System.Windows.Forms.PictureBox pictureBoxImage2;
        private System.Windows.Forms.GroupBox groupBoxImage1;
        private System.Windows.Forms.PictureBox pictureBoxProfile1;
        private System.Windows.Forms.Panel panelImage1;
        private System.Windows.Forms.PictureBox pictureBoxImage1;
    }
}

