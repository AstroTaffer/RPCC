using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using System.Threading.Tasks;
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
        private readonly Logger _logger;
        private Settings _settings;

        private ushort[][] _currentImage;        
        private GeneralImageStat _currentImageGStat;
        private bool _isCurrentImageLoaded;
        private bool _isZoomModeActivated;

        private CameraControl _cameraControl;
        private int _idleCamNum;

        private CameraFocus _cameraFocus;

        private WeatherSocket _domeSocket;
        private WeatherDataCollector _weatherDc;
        
        private DonutsSocket _donutsSocket;
        //private MountDataCollector _mountDc;
        
        //private RpccSocketClient _siTechExeSocket;

        private static readonly System.Timers.Timer FocusTimer = new System.Timers.Timer();

        public MainForm()
        {
            InitializeComponent();

            timerClock.Start();
            comboBoxImgType.SelectedIndex = 0;

            _logger = new Logger(listBoxLogs);
            _logger.AddLogEntry("Application launched");

            _settings = new Settings();
            try
            {
                _settings.LoadXmlConfig("SettingsDefault.xml");
                _logger.AddLogEntry("Default config loaded");
            }
            catch (FileNotFoundException)
            {
                _logger.AddLogEntry("WARNING Default config file not found, restoring");
                _settings.RestoreDefaultXmlConfig();
                _settings.LoadXmlConfig("SettingsDefault.xml");
                _logger.AddLogEntry("Default config loaded");
            }

            // MeteoDome connect
            _weatherDc = new WeatherDataCollector();
            _domeSocket = new WeatherSocket(_logger, _settings, _weatherDc);
            _domeSocket.Connect();

            // Donuts connect
            _donutsSocket = new DonutsSocket(_logger, _settings);
            _donutsSocket.Connect();

            // Create timer for focus loop
            FocusTimer.Elapsed += OnTimedEvent_Clock;
            FocusTimer.Interval = 1000;
            FocusTimer.Start();

            // Hardware controls
            _cameraControl = new CameraControl(_logger, _settings);
            _cameraFocus = new CameraFocus(_logger, _settings);

            // SiTechExe connect
            //_siTechExeSocket = new RpccSocketClient(_logger, _settings, "ste");
            //_siTechExeSocket.Connect();
            //_mountDc = new MountDataCollector();

            // HACK: For the love of god stop exiting the program when something is not connected!
            // Call FindFocusToolStripMenuItem_Click

            if (!_cameraFocus.Init())
            {
                MessageBox.Show(@"Can't open Focus serial port", @"OK", MessageBoxButtons.OK);
                _logger.AddLogEntry(@"Can't open Focus serial port");
            }

            Tasker.logger = _logger;
            Tasker.dataGridViewTasker = dataGridViewTasker;
            Tasker.contextMenuStripTasker = contextMenuStripTasker;
            Tasker.SetHeader();
            Tasker.LoadTasksFromXml();
        }


        #region General

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            
            Tasker.SaveTasksToXml();
            timerClock.Stop();

            _domeSocket.Disconnect();
            _donutsSocket.Disconnect();
            //_donutsSocket.DisconnectAll();
            //// _weatherDc.Dispose(); TODO
            //_domeSocket.Dispose();
            //_siTechExeSocket.DisconnectAll();
            //_mountDc.Dispose();

            _cameraControl.DisconnectCameras();
            
            _cameraFocus.SerialFocus.Close_Port();
            _cameraFocus.DeFocus = 0;
            _cameraFocus.IsZenith = false;
            labelFocusPos.Dispose();
            FocusTimer.Dispose();
        }

        private void TimerClock_Tick(object sender, EventArgs e)
        {
            tSStatusClock.Text = @"UTC: " + DateTime.UtcNow.ToString("yyyy-MM-ddTHH-mm-ss.fff");

            //label1.Text = _weatherDc.Sun.ToString(CultureInfo.InvariantCulture);
            
            _cameraControl.GetStatus();
            switch (_cameraControl.cameras.Length)
            {
                case 3:
                    labelCam3CcdTemp.Text = $@"CCD Temp: {_cameraControl.cameras[2].ccdTemp}";
                    labelCam3BaseTemp.Text = $@"Base Temp: {_cameraControl.cameras[2].baseTemp}";
                    labelCam3CoolerPwr.Text = $@"Cooler Power: {_cameraControl.cameras[2].coolerPwr} %";
                    labelCam3Status.Text = $@"Status: {_cameraControl.cameras[2].status}";
                    labelCam3RemTime.Text = $@"Remaining: {_cameraControl.cameras[2].remTime / 1000}";
                    goto case 2;
                case 2:
                    labelCam2CcdTemp.Text = $@"CCD Temp: {_cameraControl.cameras[1].ccdTemp}";
                    labelCam2BaseTemp.Text = $@"Base Temp: {_cameraControl.cameras[1].baseTemp}";
                    labelCam2CoolerPwr.Text = $@"Cooler Power: {_cameraControl.cameras[1].coolerPwr} %";
                    labelCam2Status.Text = $@"Status: {_cameraControl.cameras[1].status}";
                    labelCam2RemTime.Text = $@"Remaining: {_cameraControl.cameras[1].remTime / 1000}";
                    goto case 1;
                case 1:
                    labelCam1CcdTemp.Text = $@"CCD Temp: {_cameraControl.cameras[0].ccdTemp}";
                    labelCam1BaseTemp.Text = $@"Base Temp: {_cameraControl.cameras[0].baseTemp}";
                    labelCam1CoolerPwr.Text = $@"Cooler Power: {_cameraControl.cameras[0].coolerPwr}  %";
                    labelCam1Status.Text = $@"Status: {_cameraControl.cameras[0].status}";
                    labelCam1RemTime.Text = $@"Remaining: {_cameraControl.cameras[0].remTime / 1000}";
                    break;
            }

            _idleCamNum = 0;
            for (int i = 0; i < _cameraControl.cameras.Length; i++)
            {
                if (_cameraControl.cameras[i].status == "IDLE")
                {
                    _idleCamNum++;
                    if (_cameraControl.cameras[i].isExposing)
                    {
                        // Image ready

                        // TODO: Move to separate thread
                        RpccFits imageFits = _cameraControl.ReadImage(_cameraControl.cameras[i]);

                        imageFits.SaveFitsFile(_settings, _cameraControl, _weatherDc, i);

                        if (i == _cameraControl.task.viewCamIndex)
                        {
                            _isCurrentImageLoaded = false;
                            _currentImage = imageFits.data;
                            _currentImageGStat = new GeneralImageStat(_currentImage);
                            // TODO: Too much logs - get rid of them or write stats to labels
                            _logger.AddLogEntry($"Minimum: {_currentImageGStat.min}");
                            _logger.AddLogEntry($"Maximum: {_currentImageGStat.max}");
                            _logger.AddLogEntry($"Mean: {_currentImageGStat.mean:0.##}");
                            _logger.AddLogEntry($"SD: {_currentImageGStat.sd:0.##}");
                            PlotFitsImage();
                            _isCurrentImageLoaded = true;
                        }

                        // TODO: AUTOFOCUS, check quality
                        // _cameraFocus.AutoFocus();
                        _cameraControl.cameras[i].isExposing = false;
                    }
                }
            }

            if (_idleCamNum == _cameraControl.cameras.Length)
            {
                if (_cameraControl.task.framesNum > 1)
                {
                    _cameraControl.task.framesNum--;
                    numericUpDownSequence.Value = _cameraControl.task.framesNum;
                    _cameraControl.StartExposure();
                }
                else if (!buttonSurveyStart.Enabled)
                {
                    buttonSurveyStart.Enabled = true;
                    comboBoxImgType.Enabled = true;
                    numericUpDownSequence.Enabled = true;
                    numericUpDownExpTime.Enabled = true;
                    updateCamerasSettingsToolStripMenuItem.Enabled = true;
                    _logger.AddLogEntry("Survey finished");
                }
            }
        }

        #endregion

        #region Launch Menu

        private void FindCamerasToolStripMenuItem_Click(object sender, EventArgs e)
        {
            _cameraControl.LaunchCameras();

            switch (_cameraControl.cameras.Length)
            {
                case 3:
                    groupBoxCam3.Enabled = true;
                    radioButtonViewCam3.Enabled = true;
                    labelCam3Model.Text = $@"Model: {_cameraControl.cameras[2].modelName}";
                    labelCam3Sn.Text = $@"Serial Num: {_cameraControl.cameras[2].serialNumber}";
                    labelCam3Filter.Text = $@"Filter: {_cameraControl.cameras[2].filter}";
                    goto case 2;
                case 2:
                    groupBoxCam2.Enabled = true;
                    radioButtonViewCam2.Enabled = true;
                    labelCam2Model.Text = $@"Model: {_cameraControl.cameras[1].modelName}";
                    labelCam2Sn.Text = $@"Serial Num: {_cameraControl.cameras[1].serialNumber}";
                    labelCam2Filter.Text = $@"Filter: {_cameraControl.cameras[1].filter}";
                    goto case 1;
                case 1:
                    groupBoxCam1.Enabled = true;
                    radioButtonViewCam1.Enabled = true;
                    labelCam1Model.Text = $@"Model: {_cameraControl.cameras[0].modelName}";
                    labelCam1Sn.Text = $@"Serial Num: {_cameraControl.cameras[0].serialNumber}";
                    labelCam1Filter.Text = $@"Filter: {_cameraControl.cameras[0].filter}";
                    break;
            }
        }

        private void FindFocusToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // HACK: Can serial port be opened again if it has been closed? Check and implement properly
            //       This function must work properly when called more than once
            //       Watch out! SerialFocus.Init() keeps adding functions on ComTimer.Elapsed every time it is called!

            //_cameraFocus.SerialFocus.Close_Port();
            //_cameraFocus.Init();
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

        }

        private void ReconnectAllToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ReconnectMeteoDomeToolStripMenuItem_Click(sender, e);
            ReconnectDonutsToolStripMenuItem_Click(sender, e);
        }
        #endregion

        #region Logs
        private void ListBoxLogs_DoubleClick(object sender, EventArgs e)
        {
            // TODO: Nice idea, but nothing points to this feature. Implement in a less obscure way.
            _logger.CopyLogItem();
        }

        private void ClearToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //Clear Log window
            _logger.ClearLogs();
            _logger.AddLogEntry("Logs have been cleaned");
        }

        private void SaveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //Save logs in file
            _logger.SaveLogs();
            _logger.AddLogEntry("Logs have been saved");
        }
        #endregion

        #region Images Plotting

        private void PlotFitsImage()
        {
            pictureBoxFits.Image = null;
            pictureBoxProfile.Image = null;

            var lowerBrightnessBorder = _currentImageGStat.mean - _settings.LowerBrightnessSd * _currentImageGStat.sd;
            if (lowerBrightnessBorder < _currentImageGStat.min) lowerBrightnessBorder = _currentImageGStat.min;

            var upperBrightnessBorder = _currentImageGStat.mean + _settings.UpperBrightnessSd * _currentImageGStat.sd;
            if (upperBrightnessBorder > _currentImageGStat.max) upperBrightnessBorder = _currentImageGStat.max;

            upperBrightnessBorder -= lowerBrightnessBorder;
            var colorScale = 255 / upperBrightnessBorder;

            var bitmapFits = new Bitmap(_currentImage.Length, _currentImage[0].Length, PixelFormat.Format24bppRgb);

            int pixelColor;
            for (ushort i = 0; i < _currentImage.Length; i++)
            for (ushort j = 0; j < _currentImage[i].Length; j++)
            {
                pixelColor = (int) ((_currentImage[i][j] - lowerBrightnessBorder) * colorScale);
                if (pixelColor < 0) pixelColor = 0;
                if (pixelColor > 255) pixelColor = 255;
                bitmapFits.SetPixel(j, i, Color.FromArgb(pixelColor, pixelColor, pixelColor));
            }

            pictureBoxFits.Image = bitmapFits;
        }

        private void PlotProfileImage(ref Settings settings, ProfileImageStat pStat, ushort[][] image)
        {
            // It calculates scaling and placing parameters in a rather non-obvious way
            // But it seems harmless. If you don't have anything better to do,
            // You can tidy it up and create appropriate settings variables

            var bitmapProfile =
                new Bitmap(pictureBoxProfile.Width, pictureBoxProfile.Height, PixelFormat.Format24bppRgb);

            double pixelFlux;
            double pixelRadius;
            int pixelBitmapXCoordinate;
            int pixelBitmapYCoordinate;
            var bitmapScaleX = pictureBoxProfile.Width / (settings.AnnulusOuterRadius * Math.Sqrt(2));
            var bitmapScaleY = pictureBoxProfile.Height * 0.9 / (pStat.maxValue - pStat.background);
            for (var i = 0; i < image.Length; i++)
            for (var j = 0; j < image[i].Length; j++)
            {
                pixelFlux = image[i][j] - pStat.background;
                pixelBitmapYCoordinate = pictureBoxProfile.Height -
                                         (int) Math.Round(pictureBoxProfile.Height * 0.05 + pixelFlux * bitmapScaleY);

                pixelRadius = Math.Sqrt(Math.Pow(i - pStat.centroidYCoordinate, 2) +
                                        Math.Pow(j - pStat.centroidXCoordinate, 2));
                pixelBitmapXCoordinate = (int) Math.Round(pixelRadius * bitmapScaleX);

                if (pixelBitmapXCoordinate >= 0 &&
                    pixelBitmapXCoordinate < pictureBoxProfile.Width &&
                    pixelBitmapYCoordinate >= 0 &&
                    pixelBitmapYCoordinate < pictureBoxProfile.Height)
                    bitmapProfile.SetPixel(pixelBitmapXCoordinate, pixelBitmapYCoordinate,
                        Color.FromArgb(255, 255, 255));
            }

            var apertureBitmapXCoordinate = (int) Math.Round(settings.ApertureRadius * bitmapScaleX);
            var annulusInnerRadiusBitmapXCoordinate = (int) Math.Round(settings.AnnulusInnerRadius * bitmapScaleX);
            for (var i = 0; i < pictureBoxProfile.Height; i++)
            {
                bitmapProfile.SetPixel(apertureBitmapXCoordinate, i, Color.FromArgb(255, 0, 0));
                bitmapProfile.SetPixel(annulusInnerRadiusBitmapXCoordinate, i, Color.FromArgb(0, 0, 255));
            }

            pictureBoxProfile.Image = bitmapProfile;
        }

        private void PictureBoxFits_MouseClick(object sender, MouseEventArgs e)
        {
            if (_isCurrentImageLoaded)
            {
                if (e.Button == MouseButtons.Right)
                {
                    _isZoomModeActivated = !_isZoomModeActivated;
                    if (_isZoomModeActivated)
                    {
                        panelFitsImage.AutoScroll = true;
                        pictureBoxFits.SizeMode = PictureBoxSizeMode.AutoSize;
                    }
                    else
                    {
                        panelFitsImage.AutoScroll = false;
                        pictureBoxFits.SizeMode = PictureBoxSizeMode.StretchImage;
                    }
                }
                else if (e.Button == MouseButtons.Left)
                {
                    pictureBoxProfile.Image = null;

                    var xCoordinate = (int) ((double) e.X / pictureBoxFits.Width * _currentImage[0].Length);
                    var yCoordinate = (int) ((double) e.Y / pictureBoxFits.Height * _currentImage.Length);
                    _logger.AddLogEntry($"Pixel ({xCoordinate}, {yCoordinate}) selected");

                    if (xCoordinate - _settings.AnnulusOuterRadius < 0 ||
                        xCoordinate + _settings.AnnulusOuterRadius > _currentImage[0].Length - 1 ||
                        yCoordinate - _settings.AnnulusOuterRadius < 0 ||
                        yCoordinate + _settings.AnnulusOuterRadius > _currentImage.Length - 1)
                    {
                        _logger.AddLogEntry("WARNING Pixel is too close to the frame borders");
                    }
                    else
                    {
                        var subProfileImage = new ushort[2 * _settings.AnnulusOuterRadius + 1][];
                        for (var i = 0; i < subProfileImage.Length; i++)
                        {
                            subProfileImage[i] = new ushort[2 * _settings.AnnulusOuterRadius + 1];
                            for (var j = 0; j < subProfileImage[i].Length; j++)
                                subProfileImage[i][j] =
                                    _currentImage[yCoordinate - _settings.AnnulusOuterRadius + i]
                                        [xCoordinate - _settings.AnnulusOuterRadius + j];
                        }

                        var localStat = new ProfileImageStat(subProfileImage, ref _settings);
                        _logger.AddLogEntry($"Maximum: {localStat.maxValue} " +
                                           $"({localStat.maxXCoordinate + xCoordinate - _settings.AnnulusOuterRadius}, " +
                                           $"{localStat.maxYCoordinate + yCoordinate - _settings.AnnulusOuterRadius})");
                        _logger.AddLogEntry($"Background: {localStat.background:0.#}");
                        _logger.AddLogEntry(
                            $"Centroid: ({localStat.centroidXCoordinate + xCoordinate - _settings.AnnulusOuterRadius:0.#}, " +
                            $"{localStat.centroidYCoordinate + yCoordinate - _settings.AnnulusOuterRadius:0.#})");
                        _logger.AddLogEntry($"SNR: {localStat.snr:0.#}");
                        _logger.AddLogEntry($"FWHM: {localStat.fwhm:0.##}");
                        PlotProfileImage(ref _settings, localStat, subProfileImage);
                        _logger.AddLogEntry("Profile image plotted");
                    }
                }
            }
            else
            {
                _logger.AddLogEntry("WARNING Image not loaded");
            }
        }

        #endregion

        #region Survey

        private void ButtonSurveyStart_Click(object sender, EventArgs e)
        {
            buttonSurveyStart.Enabled = false;
            comboBoxImgType.Enabled = false;
            numericUpDownSequence.Enabled = false;
            numericUpDownExpTime.Enabled = false;
            updateCamerasSettingsToolStripMenuItem.Enabled = false;

            _cameraControl.task.framesNum = (int) numericUpDownSequence.Value;
            _cameraControl.task.framesType = comboBoxImgType.Text;
            if (_cameraControl.task.framesType == "Bias") _cameraControl.task.framesExpTime = 0;
            else _cameraControl.task.framesExpTime = (int) numericUpDownExpTime.Value * 1000;

            if (radioButtonViewCam1.Checked) _cameraControl.task.viewCamIndex = 0;
            else if (radioButtonViewCam2.Checked) _cameraControl.task.viewCamIndex = 1;
            else if (radioButtonViewCam3.Checked) _cameraControl.task.viewCamIndex = 2;

            _cameraControl.SetSurveySettings();
            _cameraControl.StartExposure();
            _logger.AddLogEntry($"Survey started - {_cameraControl.task.framesNum} {_cameraControl.task.framesType}" +
                $" frames with exposure of {_cameraControl.task.framesExpTime * 1e-3} seconds");
        }

        private void ButtonSurveyStop_Click(object sender, EventArgs e)
        {
            _cameraControl.CancelSurvey();
            _logger.AddLogEntry($"Survey cancelled, {_cameraControl.task.framesNum} {(_cameraControl.task.framesNum == 1 ? "frame" : "frames")} skipped");
            _cameraControl.task.framesNum = 1;
            numericUpDownSequence.Value = 1;

            buttonSurveyStart.Enabled = true;
            comboBoxImgType.Enabled = true;
            numericUpDownSequence.Enabled = true;
            numericUpDownExpTime.Enabled = true;
            updateCamerasSettingsToolStripMenuItem.Enabled = true;
        }

        #endregion

        #region Options

        private void SettingsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var settingsForm = new SettingsForm(_settings);
            settingsForm.ShowDialog();
            if (settingsForm.DialogResult == DialogResult.OK) _logger.AddLogEntry("Settings changed");
        }

        private void UpdateCameraSettingsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            _cameraControl.UpdateSettings();
        }

        private void LoadConfigToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (openFileDialogConfig.ShowDialog() == DialogResult.OK)
            {
                _settings.LoadXmlConfig(openFileDialogConfig.FileName);
                _logger.AddLogEntry($"Config file {saveFileDialogConfig.FileName} loaded");
            }
        }

        private void SaveConfigToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (saveFileDialogConfig.ShowDialog() == DialogResult.OK)
            {
                _settings.SaveXmlConfig(saveFileDialogConfig.FileName);
                _logger.AddLogEntry($"Config file {saveFileDialogConfig.FileName} saved");
            }
        }

        #endregion

        #region Debug Menu

        private void TestDLLlibrariesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var testFits = new RpccFits(".\\Cams\\TestImage.fits");
            var testHeader = testFits.header;
            _logger.AddLogEntry($"Template DATE-OBS: {testHeader.GetStringValue("DATE-OBS")}");

            var libVer = new StringBuilder(128);
            var len = new IntPtr(128);
            var errorLastFliCmd = NativeMethods.FLIGetLibVersion(libVer, len);
            if (errorLastFliCmd == 0)
                _logger.AddLogEntry(libVer.ToString());
            else
                _logger.AddLogEntry("WARNING Unable to retrieve FLI library version");
        }

        private async void LoadTestImageToolStripMenuItem_Click(object sender, EventArgs e)
        {
            _isCurrentImageLoaded = false;
            var testFits = new RpccFits(".\\Cams\\TestImage.fits");
            _currentImage = testFits.data;

            _currentImageGStat = new GeneralImageStat(_currentImage);
            _logger.AddLogEntry($"Minimum: {_currentImageGStat.min}");
            _logger.AddLogEntry($"Maximum: {_currentImageGStat.max}");
            _logger.AddLogEntry($"Mean: {_currentImageGStat.mean:0.##}");
            _logger.AddLogEntry($"SD: {_currentImageGStat.sd:0.##}");

            await Task.Run(() => PlotFitsImage());
            _isCurrentImageLoaded = true;
            _logger.AddLogEntry("Test image plotted");
        }

        private void RestoreDefaultConfigFileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            _settings.RestoreDefaultXmlConfig();
            _logger.AddLogEntry("Default config file restored");
        }

        private void SocketToolStripMenuItem_Click(object sender, EventArgs e)
        {
            _logger.AddLogEntry("Test donuts");
            _logger.AddLogEntry(_donutsSocket.PingServer());
            
            var cwd = Directory.GetCurrentDirectory();
            var refFile = cwd + "\\Guid\\2023-04-07T17-56-16.918_EAST_V.fits";
            var testFile = cwd + "\\Guid\\2023-04-07T18-00-24.167_EAST_V.fits";
            var req = refFile + ";" + testFile;
            // _logger.AddLogEntry(req);
            var outPut = _donutsSocket.GetGuideCorrection(req);
            if (outPut == null) return;
            _logger.AddLogEntry("shifts = " + outPut[0] + "x " + outPut[1] + "y ");
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
            _cameraFocus.SerialFocus.UpdateData();
            Thread.Sleep(waitTime);
            try
            {
                labelFocusPos.Invoke((MethodInvoker) delegate
                {
                    labelFocusPos.Text = $@"Focus position: {_cameraFocus.SerialFocus.CurrentPosition}";
                });

                labelEndSwitch.Text = @"Endswitch: " + (_cameraFocus.SerialFocus.Switches[6] ? "joint" : "unjoint");
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
            _cameraFocus.isAutoFocus = isAutoFocusEnabled;
        }

        private void NumericUpDownSetDefoc_ValueChanged(object sender, EventArgs e)
        {
            _cameraFocus.DeFocus = (int)numericUpDownSetDefoc.Value;
        }

        private void ButtonSetZeroPos_Click(object sender, EventArgs e)
        {
            _cameraFocus.SerialFocus.Set_Zero();
        }

        private void ButtonRunStop_Click(object sender, EventArgs e)
        {
            _cameraFocus.SerialFocus.Stop();
        }

        private void ButtonRun_Click(object sender, EventArgs e)
        {
            if (radioButtonRunFast.Checked) _cameraFocus.SerialFocus.FRun_To((int)numericUpDownRun.Value);
            else if (radioButtonRunSlow.Checked) _cameraFocus.SerialFocus.SRun_To((int)numericUpDownRun.Value);
        }

        private void CheckBoxGoZenith_CheckedChanged(object sender, EventArgs e)
        {
            _cameraFocus.IsZenith = checkBoxGoZenith.Checked;
        }
        #endregion

        private void addToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // _logger.AddLogEntry("Add Task click");
            var taskForm = new TaskForm(true);
            taskForm.Show();
        }

        private void dataGridViewTasker_CellMouseDoubleClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            // MessageBox.Show(e.RowIndex.ToString());
            if (e.RowIndex == -1) return;
            var taskForm = new TaskForm(false, e.RowIndex);
            taskForm.Show();
        }
    }
}
