using System;
using System.Threading;
using System.Windows.Forms;
using System.Timers;
using RPCC.Cams;
using RPCC.Focus;
using RPCC.Utils;
using RPCC.Comms;
using RPCC.Tasks;

namespace RPCC
{
    public partial class MainForm : Form
    {
        /// <summary>
        ///     Логика работы основной формы программы
        ///     Обработка команд пользователя с помощью вызова готовых функций
        /// </summary>
        //private bool _isCurrentImageLoaded;
        //private bool _isZoomModeActivated;

        private readonly WeatherSocket _domeSocket;
        private readonly DonutsSocket _donutsSocket;

        private static readonly System.Timers.Timer FocusTimer = new System.Timers.Timer();

        public static bool IsTaskFormOpen;

        #region General
        public MainForm()
        {
            InitializeComponent();

            // _isFirstLoad = true;

            Logger.LogBox = listBoxLogs;
            Logger.AddLogEntry("Application launched");

            Settings.LoadXmlConfig("SettingsDefault.xml");

            // Camera controls
            CameraControl.resetUi = ResetCamsUi;
            CameraControl.resetPics = RefreshImages;

            // MeteoDome connect
            _domeSocket = new WeatherSocket();
            _domeSocket.Connect();

            // Donuts connect
            _donutsSocket = new DonutsSocket();
            _donutsSocket.Connect();

            // SiTechExe connect
            SiTechExeSocket.Connect();

            // Create timer for focus loop
            FocusTimer.Elapsed += OnTimedEvent_Clock;
            FocusTimer.Interval = 1000;
            
            // Focus connect
            // HACK: For the love of god stop exiting the program when something is not connected!
            // Call FindFocusToolStripMenuItem_Click
            if (!SerialFocus.Init())
            {
                MessageBox.Show(@"Can't open Focus serial port", @"OK", MessageBoxButtons.OK);
                Logger.AddLogEntry(@"Can't open Focus serial port");
            }
            groupBoxFocusSettings.Text = $@"Focus Settings (COMPORT {Settings.FocusComId})";

            if (!DbCommunicate.ConnectToDb())
            {
                MessageBox.Show(@"Can't connect to data base", @"OK", MessageBoxButtons.OK);
                Logger.AddLogEntry(@"Can't connect to data base");
            }
            Tasker.dataGridViewTasker = dataGridViewTasker;
            Tasker.contextMenuStripTasker = contextMenuStripTasker;
            Tasker.SetHeader();
            // Tasker.LoadTasksFromXml();
            DbCommunicate.LoadDbTable();
            DbCommunicate.DisconnectFromDb();
            
            FocusTimer.Start();
            timerUi.Start();
            Head.StartThinking();
            if (checkBoxHead.Checked)
            {
                Head.IsThinking = true;
                Head.ThinkingTimer.Start();
            }
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            
            timerUi.Stop();
            
            // Tasker.SaveTasksToXml();
            DbCommunicate.DisconnectFromDb();
            
            _domeSocket.Disconnect();
            DonutsSocket.Disconnect();
            SiTechExeSocket.Disconnect();

            CameraControl.DisconnectCameras();

            SerialFocus.Close_Port();
            CameraFocus.DeFocus = 0;
            CameraFocus.IsZenith = false;
            labelFocusPos.Dispose();
            FocusTimer.Dispose();
        }

        private void TimerUiUpdate(object sender, EventArgs e)
        {
            tSStatusClock.Text = @"UTC: " + DateTime.UtcNow.ToString("yyyy-MM-ddTHH-mm-ss");

            try
            {
                switch (CameraControl.cams.Length)
                {
                    case 3:
                        labelCam3CcdTemp.Text = $"CCD Temp: {CameraControl.cams[2].ccdTemp:F3}";
                        labelCam3BaseTemp.Text = $"Base Temp: {CameraControl.cams[2].baseTemp:F3}";
                        labelCam3CoolerPwr.Text = $"Cooler Power: {CameraControl.cams[2].coolerPwr} %";
                        labelCam3Status.Text = $"Status: {CameraControl.cams[2].status}";
                        labelCam3RemTime.Text = $"Remaining: {CameraControl.cams[2].remTime / 1000}";
                        goto case 2;
                    case 2:
                        labelCam2CcdTemp.Text = $"CCD Temp: {CameraControl.cams[1].ccdTemp:F3}";
                        labelCam2BaseTemp.Text = $"Base Temp: {CameraControl.cams[1].baseTemp:F3}";
                        labelCam2CoolerPwr.Text = $"Cooler Power: {CameraControl.cams[1].coolerPwr} %";
                        labelCam2Status.Text = $"Status: {CameraControl.cams[1].status}";
                        labelCam2RemTime.Text = $"Remaining: {CameraControl.cams[1].remTime / 1000}";
                        goto case 1;
                    case 1:
                        labelCam1CcdTemp.Text = $"CCD Temp: {CameraControl.cams[0].ccdTemp:F3}";
                        labelCam1BaseTemp.Text = $"Base Temp: {CameraControl.cams[0].baseTemp:F3}";
                        labelCam1CoolerPwr.Text = $"Cooler Power: {CameraControl.cams[0].coolerPwr} %";
                        labelCam1Status.Text = $"Status: {CameraControl.cams[0].status}";
                        labelCam1RemTime.Text = $"Remaining: {CameraControl.cams[0].remTime / 1000}";
                        break;
                }
            }
            catch (IndexOutOfRangeException)
            {
                Logger.AddLogEntry("WARNING Cameras list has been reset while updating GUI");
                // If the cameras have been disconnected when we were updating the labels,
                // IndexOutOfRangeException will be raised and silenced. It's not the best solution,
                // but Monitor.Enter will stop the GUI thread and Monitor.TryEnter may cause some
                // loops to be skipped if GUI timer and Cams timer would elapse at the same time.
                // Though I think it's really unlikely. Use Monitor.TryEnter in case of bugs.
            }
        }
        
        internal void ResetCamsUi()
        {
            // Camera 1
            groupBoxCam1.Enabled = false;
            groupBoxImage1.Enabled = false;
            pictureBoxImage1.Image = null;
            pictureBoxProfile1.Image = null;
            labelCam1Model.Text = "Model:";
            labelCam1Sn.Text = "Serial Num:";
            labelCam1Filter.Text = "Filter:";
            labelCam1CcdTemp.Text = "CCD Temp:";
            labelCam1BaseTemp.Text = "Base Temp:";
            labelCam1CoolerPwr.Text = "Cooler Power:";
            labelCam1Status.Text = "Status:";
            labelCam1RemTime.Text = "Remaining:";

            // Camera 2
            groupBoxCam2.Enabled = false;
            groupBoxImage2.Enabled = false;
            pictureBoxImage2.Image = null;
            pictureBoxProfile2.Image = null;
            labelCam2Model.Text = "Model:";
            labelCam2Sn.Text = "Serial Num:";
            labelCam2Filter.Text = "Filter:";
            labelCam2CcdTemp.Text = "CCD Temp:";
            labelCam2BaseTemp.Text = "Base Temp:";
            labelCam2CoolerPwr.Text = "Cooler Power:";
            labelCam2Status.Text = "Status:";
            labelCam2RemTime.Text = "Remaining:";

            // Camera 3
            groupBoxCam3.Enabled = false;
            groupBoxImage3.Enabled = false;
            pictureBoxImage3.Image = null;
            pictureBoxProfile3.Image = null;
            labelCam3Model.Text = "Model:";
            labelCam3Sn.Text = "Serial Num:";
            labelCam3Filter.Text = "Filter:";
            labelCam3CcdTemp.Text = "CCD Temp:";
            labelCam3BaseTemp.Text = "Base Temp:";
            labelCam3CoolerPwr.Text = "Cooler Power:";
            labelCam3Status.Text = "Status:";
            labelCam3RemTime.Text = "Remaining:";

            switch (CameraControl.cams.Length)
            {
                case 3:
                    groupBoxCam3.Invoke((MethodInvoker) delegate
                    {
                        groupBoxCam3.Enabled = true;
                       labelCam3Model.Text = $"Model: {CameraControl.cams[2].modelName}";
                       labelCam3Sn.Text = $"Serial Num: {CameraControl.cams[2].serialNumber}";
                       labelCam3Filter.Text = $"Filter: {CameraControl.cams[2].filter}"; 
                    });
                    groupBoxImage3.Enabled = false;
                    goto case 2;
                case 2:
                    groupBoxCam2.Invoke((MethodInvoker) delegate
                    {
                        groupBoxCam2.Enabled = true;
                        labelCam2Model.Text = $"Model: {CameraControl.cams[1].modelName}";
                        labelCam2Sn.Text = $"Serial Num: {CameraControl.cams[1].serialNumber}";
                        labelCam2Filter.Text = $"Filter: {CameraControl.cams[1].filter}";
                    });
                    groupBoxImage2.Enabled = false;
                    goto case 1;
                case 1:
                    groupBoxCam1.Invoke((MethodInvoker) delegate
                    {
                        groupBoxCam1.Enabled = true;
                        labelCam1Model.Text = $"Model: {CameraControl.cams[0].modelName}";
                        labelCam1Sn.Text = $"Serial Num: {CameraControl.cams[0].serialNumber}";
                        labelCam1Filter.Text = $"Filter: {CameraControl.cams[0].filter}";
                    });
                    groupBoxImage1.Enabled = false;
                    break;
            }
        }

        private void ButtonSurveyStop_Click(object sender, EventArgs e)
        {
            //_cameraControl.CancelSurvey();
            //Logger.AddLogEntry($"Survey cancelled, {_cameraControl.task.framesNum} {(_cameraControl.task.framesNum == 1 ? "frame" : "frames")} skipped");
            //_cameraControl.task.framesNum = 1;
            //numericUpDownSequence.Value = 1;

            //buttonSurveyStart.Enabled = true;
            //comboBoxImgType.Enabled = true;
            //numericUpDownSequence.Enabled = true;
            //numericUpDownExpTime.Enabled = true;
            //updateCamerasSettingsToolStripMenuItem.Enabled = true;
        }
        #endregion

        #region Launch Menu
        private void FindCamerasToolStripMenuItem_Click(object sender, EventArgs e)
        {
            CameraControl.ReconnectCameras();
        }

        private void FindFocusToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // HACK: Can serial port be opened again if it has been closed? Check and implement properly
            //       This function must work properly when called more than once
            //       Watch out! SerialFocus.Init() keeps adding functions on ComTimer.Elapsed every time it is called!

            //SerialFocus.Close_Port();
            //FocusTimer.Elapsed -= OnTimedEvent_Clock;
            //SerialFocus.Init();
        }

        private void ReconnectMeteoDomeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (_domeSocket._isConnected) _domeSocket.Disconnect();
            _domeSocket.Connect();
        }

        private void ReconnectDonutsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // if(_donutsSocket.isConnected) _donutsSocket.Disconnect();
            _donutsSocket.Connect();
        }

        private void ReconnectSiTechExeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (SiTechExeSocket._isConnected) SiTechExeSocket.Disconnect();
            SiTechExeSocket.Connect();
        }

        private void ReconnectAllToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ReconnectMeteoDomeToolStripMenuItem_Click(sender, e);
            ReconnectDonutsToolStripMenuItem_Click(sender, e);
            ReconnectSiTechExeToolStripMenuItem_Click(sender, e);
        }
        #endregion

        #region Logs
        private void ListBoxLogs_DoubleClick(object sender, EventArgs e)
        {
            // TODO: Nice idea, but nothing points to this feature. Implement in a less obscure way.
            Logger.CopyLogItem();
        }

        private void ClearToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // Clear log window
            Logger.ClearLogs();
            Logger.AddLogEntry("Logs have been cleaned");
        }

        private void SaveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // Save logs in file
            Logger.SaveLogs();
            Logger.AddLogEntry("Logs have been saved");
        }
        #endregion

        #region Camera Images
        private void RefreshImages()
        {
            pictureBoxImage1.Image = null;
            pictureBoxProfile1.Image = null;
            pictureBoxImage2.Image = null;
            pictureBoxProfile2.Image = null;
            pictureBoxImage3.Image = null;
            pictureBoxProfile3.Image = null;

            switch (CameraControl.cams.Length)
            {
                case 3:
                    if (CameraControl.cams[2].latestImageBitmap != null)
                        groupBoxImage3.Invoke((MethodInvoker)delegate
                        {
                            pictureBoxImage3.Image = CameraControl.cams[2].latestImageBitmap;
                        });
                    goto case 2;
                case 2:
                    if (CameraControl.cams[1].latestImageBitmap != null)
                        groupBoxImage2.Invoke((MethodInvoker)delegate
                        {
                            pictureBoxImage2.Image = CameraControl.cams[1].latestImageBitmap;
                        });
                    goto case 1;
                case 1:
                    if (CameraControl.cams[0].latestImageBitmap != null)
                        groupBoxImage1.Invoke((MethodInvoker)delegate
                        {
                            pictureBoxImage1.Image = CameraControl.cams[0].latestImageBitmap;
                        });
                    break;
            }
        }
        
        private void PictureBoxFits_MouseClick(object sender, MouseEventArgs e)
        {
            //if (_isCurrentImageLoaded)
            //{
            //    if (e.Button == MouseButtons.Right)
            //    {
            //        _isZoomModeActivated = !_isZoomModeActivated;
            //        if (_isZoomModeActivated)
            //        {
            //            panelFitsImage.AutoScroll = true;
            //            pictureBoxFits.SizeMode = PictureBoxSizeMode.AutoSize;
            //        }
            //        else
            //        {
            //            panelFitsImage.AutoScroll = false;
            //            pictureBoxFits.SizeMode = PictureBoxSizeMode.StretchImage;
            //        }
            //    }
            //    else if (e.Button == MouseButtons.Left)
            //    {
            //        pictureBoxProfile.Image = null;

            //        var xCoordinate = (int) ((double) e.X / pictureBoxFits.Width * _currentImage[0].Length);
            //        var yCoordinate = (int) ((double) e.Y / pictureBoxFits.Height * _currentImage.Length);
            //        Logger.AddLogEntry($"Pixel ({xCoordinate}, {yCoordinate}) selected");

            //        if (xCoordinate - Settings.AnnulusOuterRadius < 0 ||
            //            xCoordinate + Settings.AnnulusOuterRadius > _currentImage[0].Length - 1 ||
            //            yCoordinate - Settings.AnnulusOuterRadius < 0 ||
            //            yCoordinate + Settings.AnnulusOuterRadius > _currentImage.Length - 1)
            //        {
            //            Logger.AddLogEntry("WARNING Pixel is too close to the frame borders");
            //        }
            //        else
            //        {
            //            var subProfileImage = new ushort[2 * Settings.AnnulusOuterRadius + 1][];
            //            for (var i = 0; i < subProfileImage.Length; i++)
            //            {
            //                subProfileImage[i] = new ushort[2 * Settings.AnnulusOuterRadius + 1];
            //                for (var j = 0; j < subProfileImage[i].Length; j++)
            //                    subProfileImage[i][j] =
            //                        _currentImage[yCoordinate - Settings.AnnulusOuterRadius + i]
            //                            [xCoordinate - Settings.AnnulusOuterRadius + j];
            //            }

            //            var localStat = new ProfileImageStat(subProfileImage);
            //            Logger.AddLogEntry($"Max: {localStat.maxValue} " +
            //                               $"({localStat.maxXCoordinate + xCoordinate - Settings.AnnulusOuterRadius}; " +
            //                               $"{localStat.maxYCoordinate + yCoordinate - Settings.AnnulusOuterRadius}); " +
            //                               $"Background: {localStat.background:0.#}; " +
            //                               $"Centroid: ({localStat.centroidXCoordinate + xCoordinate - Settings.AnnulusOuterRadius:0.#}; " +
            //                               $"{localStat.centroidYCoordinate + yCoordinate - Settings.AnnulusOuterRadius:0.#}); " +
            //                               $"SNR: {localStat.snr:0.#}; " +
            //                               $"FWHM: {localStat.fwhm:0.##}");
            //            PlotProfileImage(localStat, subProfileImage);
            //            Logger.AddLogEntry("Profile image plotted");
            //        }
            //    }
            //}
            //else
            //{
            //    Logger.AddLogEntry("WARNING Image not loaded");
            //}
        }
        #endregion

        #region Options
        private void SettingsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var settingsForm = new SettingsForm();
            settingsForm.ShowDialog();
            if (settingsForm.DialogResult == DialogResult.OK) Logger.AddLogEntry("Settings changed");
        }

        private void UpdateCameraSettingsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //_cameraControl.UpdateSettings();
        }

        private void LoadConfigToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (openFileDialogConfig.ShowDialog() == DialogResult.OK)
                Settings.LoadXmlConfig(openFileDialogConfig.FileName);
        }

        private void SaveConfigToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (saveFileDialogConfig.ShowDialog() == DialogResult.OK)
                Settings.SaveXmlConfig(saveFileDialogConfig.FileName);
        }
        #endregion

        #region Debug Menu
        private void RestoreDefaultConfigFileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Settings.RestoreDefaultXmlConfig();
        }
        #endregion

        #region Focus
        private void OnTimedEvent_Clock(object sender, ElapsedEventArgs e)
        {
            GetData();
            // var getFocus = new Thread(GetData);
            // getFocus.Start();
        }
        
        private void GetData()
        {
            const int waitTime = 50;
            SerialFocus.UpdateData();
            Thread.Sleep(waitTime);
            try
            {
                labelFocusPos.Invoke((MethodInvoker) delegate
                {
                    labelFocusPos.Text = $@"Focus position: {SerialFocus.CurrentPosition}";
                });

                labelEndSwitch.Text = @"Endswitch: " + (SerialFocus.Switches[6] ? "joint" : "unjoint");
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }
        
        private void CheckBoxAutoFocus_CheckedChanged(object sender, EventArgs e)
        {
            var isAutoFocusEnabled = checkBoxAutoFocus.Checked;

            //Settings
            buttonRun.Enabled = !isAutoFocusEnabled;
            buttonSetZeroPos.Enabled = !isAutoFocusEnabled;
            numericUpDownRun.Enabled = !isAutoFocusEnabled;

            //AutoFocus
            numericUpDownSetDefoc.Enabled = isAutoFocusEnabled;
            checkBoxGoZenith.Enabled = isAutoFocusEnabled;
            CameraFocus.IsAutoFocus = isAutoFocusEnabled;
        }

        private void NumericUpDownSetDefoc_ValueChanged(object sender, EventArgs e)
        {
            CameraFocus.DeFocus = (int)numericUpDownSetDefoc.Value;
        }

        private void ButtonSetZeroPos_Click(object sender, EventArgs e)
        {
            SerialFocus.Set_Zero();
        }

        private void ButtonRunStop_Click(object sender, EventArgs e)
        {
            SerialFocus.Stop();
        }

        private void ButtonRun_Click(object sender, EventArgs e)
        {
            if (radioButtonRunFast.Checked) SerialFocus.FRun_To((int)numericUpDownRun.Value);
            else if (radioButtonRunSlow.Checked) SerialFocus.SRun_To((int)numericUpDownRun.Value);
        }

        private void CheckBoxGoZenith_CheckedChanged(object sender, EventArgs e)
        {
            CameraFocus.IsZenith = checkBoxGoZenith.Checked;
        }
        #endregion

        #region Tasker
        private void AddToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // Logger.AddLogEntry("Add Task click");
            if (!IsTaskFormOpen)
            {
                IsTaskFormOpen = true;
               var taskForm = new TaskForm(true);
               taskForm.Show(); 
               
            }
            
        }

        private void DataGridViewTasker_CellMouseDoubleClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            // MessageBox.Show(e.RowIndex.ToString());
            if (e.RowIndex == -1) return;
            var taskForm = new TaskForm(false, e.RowIndex);
            taskForm?.Show();
        }
        
        private void CheckBoxHead_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBoxHead.Checked) Head.ThinkingTimer.Start();
            else Head.ThinkingTimer.Stop();

            Head.IsThinking = checkBoxHead.Checked;
        }

        // private void DataGridViewTasker_VisibleChanged(object sender, EventArgs e)
        // {
        //     if (!_isFirstLoad || !dataGridViewTasker.Visible) return;
        //     _isFirstLoad = false;
        //     Tasker.PaintTable();
        // }
        #endregion
    }
}
