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
        /// Требуемый метод для поддержки конструктора — не изменяйте 
        /// содержимое этого метода с помощью редактора кода.
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
            this.optionsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.settingsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.loadConfigToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.saveConfigToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.debugToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.addTestLogEntryToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.testDLLlibrariesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.loadTestImageToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.testCamerasToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
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
            this.statusStrip.SuspendLayout();
            this.menuStrip.SuspendLayout();
            this.groupBoxLogs.SuspendLayout();
            this.panelFitsImage.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxFits)).BeginInit();
            this.groupBoxImageAnalysis.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxProfile)).BeginInit();
            this.SuspendLayout();
            // 
            // statusStrip
            // 
            this.statusStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tSStatusClock});
            this.statusStrip.Location = new System.Drawing.Point(0, 515);
            this.statusStrip.Name = "statusStrip";
            this.statusStrip.Size = new System.Drawing.Size(672, 22);
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
            this.menuStrip.Size = new System.Drawing.Size(672, 24);
            this.menuStrip.TabIndex = 1;
            this.menuStrip.Text = "menuStrip";
            // 
            // launchToolStripMenuItem
            // 
            this.launchToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.findCamerasToolStripMenuItem});
            this.launchToolStripMenuItem.Name = "launchToolStripMenuItem";
            this.launchToolStripMenuItem.Size = new System.Drawing.Size(58, 20);
            this.launchToolStripMenuItem.Text = "Launch";
            // 
            // findCamerasToolStripMenuItem
            // 
            this.findCamerasToolStripMenuItem.Name = "findCamerasToolStripMenuItem";
            this.findCamerasToolStripMenuItem.Size = new System.Drawing.Size(144, 22);
            this.findCamerasToolStripMenuItem.Text = "Find cameras";
            // 
            // optionsToolStripMenuItem
            // 
            this.optionsToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.settingsToolStripMenuItem,
            this.loadConfigToolStripMenuItem,
            this.saveConfigToolStripMenuItem});
            this.optionsToolStripMenuItem.Name = "optionsToolStripMenuItem";
            this.optionsToolStripMenuItem.Size = new System.Drawing.Size(61, 20);
            this.optionsToolStripMenuItem.Text = "Options";
            // 
            // settingsToolStripMenuItem
            // 
            this.settingsToolStripMenuItem.Name = "settingsToolStripMenuItem";
            this.settingsToolStripMenuItem.Size = new System.Drawing.Size(137, 22);
            this.settingsToolStripMenuItem.Text = "Settings";
            this.settingsToolStripMenuItem.Click += new System.EventHandler(this.SettingsToolStripMenuItem_Click);
            // 
            // loadConfigToolStripMenuItem
            // 
            this.loadConfigToolStripMenuItem.Name = "loadConfigToolStripMenuItem";
            this.loadConfigToolStripMenuItem.Size = new System.Drawing.Size(137, 22);
            this.loadConfigToolStripMenuItem.Text = "Load config";
            this.loadConfigToolStripMenuItem.Click += new System.EventHandler(this.LoadConfigToolStripMenuItem_Click);
            // 
            // saveConfigToolStripMenuItem
            // 
            this.saveConfigToolStripMenuItem.Name = "saveConfigToolStripMenuItem";
            this.saveConfigToolStripMenuItem.Size = new System.Drawing.Size(137, 22);
            this.saveConfigToolStripMenuItem.Text = "Save config";
            this.saveConfigToolStripMenuItem.Click += new System.EventHandler(this.SaveConfigToolStripMenuItem_Click);
            // 
            // debugToolStripMenuItem
            // 
            this.debugToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.addTestLogEntryToolStripMenuItem,
            this.testDLLlibrariesToolStripMenuItem,
            this.loadTestImageToolStripMenuItem,
            this.testCamerasToolStripMenuItem,
            this.restoreDefaultConfigFileToolStripMenuItem});
            this.debugToolStripMenuItem.Name = "debugToolStripMenuItem";
            this.debugToolStripMenuItem.Size = new System.Drawing.Size(54, 20);
            this.debugToolStripMenuItem.Text = "Debug";
            // 
            // addTestLogEntryToolStripMenuItem
            // 
            this.addTestLogEntryToolStripMenuItem.Name = "addTestLogEntryToolStripMenuItem";
            this.addTestLogEntryToolStripMenuItem.Size = new System.Drawing.Size(209, 22);
            this.addTestLogEntryToolStripMenuItem.Text = "Add test log entry";
            this.addTestLogEntryToolStripMenuItem.Click += new System.EventHandler(this.AddTestLogEntryToolStripMenuItem_Click);
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
            // testCamerasToolStripMenuItem
            // 
            this.testCamerasToolStripMenuItem.Name = "testCamerasToolStripMenuItem";
            this.testCamerasToolStripMenuItem.Size = new System.Drawing.Size(209, 22);
            this.testCamerasToolStripMenuItem.Text = "Test cameras";
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
            this.groupBoxLogs.Size = new System.Drawing.Size(330, 482);
            this.groupBoxLogs.TabIndex = 2;
            this.groupBoxLogs.TabStop = false;
            this.groupBoxLogs.Text = "Logs";
            // 
            // buttonLogsSave
            // 
            this.buttonLogsSave.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonLogsSave.Location = new System.Drawing.Point(224, 453);
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
            this.buttonLogsClear.Location = new System.Drawing.Point(4, 453);
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
            this.listBoxLogs.Size = new System.Drawing.Size(320, 433);
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
            this.groupBoxImageAnalysis.Size = new System.Drawing.Size(312, 482);
            this.groupBoxImageAnalysis.TabIndex = 5;
            this.groupBoxImageAnalysis.TabStop = false;
            this.groupBoxImageAnalysis.Text = "Image Analysis";
            // 
            // pictureBoxProfile
            // 
            this.pictureBoxProfile.BackColor = System.Drawing.SystemColors.Desktop;
            this.pictureBoxProfile.Location = new System.Drawing.Point(6, 326);
            this.pictureBoxProfile.Name = "pictureBoxProfile";
            this.pictureBoxProfile.Size = new System.Drawing.Size(300, 150);
            this.pictureBoxProfile.TabIndex = 5;
            this.pictureBoxProfile.TabStop = false;
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(672, 537);
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
            this.statusStrip.ResumeLayout(false);
            this.statusStrip.PerformLayout();
            this.menuStrip.ResumeLayout(false);
            this.menuStrip.PerformLayout();
            this.groupBoxLogs.ResumeLayout(false);
            this.panelFitsImage.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxFits)).EndInit();
            this.groupBoxImageAnalysis.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxProfile)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.StatusStrip statusStrip;
        private System.Windows.Forms.ToolStripStatusLabel tSStatusClock;
        private System.Windows.Forms.MenuStrip menuStrip;
        private System.Windows.Forms.ToolStripMenuItem launchToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem findCamerasToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem debugToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem testDLLlibrariesToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem testCamerasToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem loadTestImageToolStripMenuItem;
        private System.Windows.Forms.GroupBox groupBoxLogs;
        private System.Windows.Forms.Timer timerClock;
        private System.Windows.Forms.ToolStripMenuItem addTestLogEntryToolStripMenuItem;
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
    }
}

