using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Text;
using System.Windows.Forms;
using System.Threading.Tasks;
using RPCC.Cams;
using RPCC.Focus;
using RPCC.Utils;

namespace RPCC
{
    public partial class MainForm : Form
    {
        /// <summary>
        ///     Логика работы основной формы программы.
        ///     Обработка команд пользователя с помощью вызова готовых функций.
        /// </summary>
        private ushort[][] _currentImage;
        
        private GeneralImageStat _currentImageGStat;
        private int _idleCamNum;

        private bool _isCurrentImageLoaded;
        
        private bool _isZoomModeActivated;

        private readonly Logger _logger;
        private Settings _settings;

        private CameraControl _cameraControl;
        private CameraFocus _cameraFocus;

        private RpccSocketClient domeSocket;
        private RpccSocketClient donutsSocket;

        private DataCollector _dataCollector;

        public MainForm()
        {
            InitializeComponent();

            timerClock.Enabled = true;
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
                _logger.AddLogEntry("WARNING Default config file not found");
            }
            
            // // Donuts
            // StartDonutsPy();
            // donutsSocket = new RpccSocketClient(logger, "don");
            // donutsSocket.Connect();

            // Hardware controls
            _cameraControl = new CameraControl(_logger, _settings);
            _cameraFocus = new CameraFocus(_logger);

            // MeteoDome connect
            domeSocket = new RpccSocketClient(_logger, "dom");
            domeSocket.Connect();
            _dataCollector = new DataCollector(domeSocket, _logger);

            // if (!_cameraFocus.Init())  
            //     if (MessageBox.Show(@"Can't open Focus serial port", @"OK", MessageBoxButtons.OK) == DialogResult.OK)
            //         Environment.Exit(1);
        }


        #region General

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            timerClock.Stop();
            domeSocket.DisconnectAll();
            // donutsSocket.DisconnectAll();
            // DataCollector.Dispose();
            _cameraControl.DisconnectCameras();
            _cameraFocus.SerialFocus.Close_Port();
        }

        private void TimerClock_Tick(object sender, EventArgs e)
        {
            tSStatusClock.Text = @"UTC: " + DateTime.UtcNow.ToString("yyyy-MM-ddTHH-mm-ss.fff");

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

                        imageFits.SaveFitsFile(_settings, _cameraControl, i);

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

        private void FocusToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var focusForm = new FocusForm(_cameraFocus);
            focusForm.Show();
        }

        private void ReconnectSocketsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            _logger.AddLogEntry("Reconnect to servers");
            domeSocket.DisconnectAll();
            // donutsSocket.DisconnectAll();
            domeSocket.Connect();
            // donutsSocket.Connect();
        }

        private static void StartDonutsPy()
        {
            var cwd = Directory.GetCurrentDirectory();
            var start = new ProcessStartInfo
            {
                FileName = "python.exe", //cmd is full path to python.exe
                Arguments = cwd + "\\Guid\\DONUTS.py", //args is path to .py file and any cmd line args
                UseShellExecute = false,
                RedirectStandardOutput = true,
                CreateNoWindow = true
            };
            Process.Start(start);
        }

        #endregion

        #region Logs

        private void ButtonLogsClear_Click(object sender, EventArgs e)
        {
            _logger.ClearLogs();
            _logger.AddLogEntry("Logs have been cleaned");
        }

        private void ButtonLogsSave_Click(object sender, EventArgs e)
        {
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
            _cameraControl.task.framesNum = 1;
            numericUpDownSequence.Value = 1;

            buttonSurveyStart.Enabled = true;
            comboBoxImgType.Enabled = true;
            numericUpDownSequence.Enabled = true;
            numericUpDownExpTime.Enabled = true;
            updateCamerasSettingsToolStripMenuItem.Enabled = true;
            _logger.AddLogEntry("Survey cancelled");
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
            var cwd = Directory.GetCurrentDirectory();
            const string refFile = ".\\Guid\\2023-04-07T17-56-16.918_EAST_V.fits";
            const string testFile = "\\Guid\\2023-04-07T18-00-24.167_EAST_V.fits";

            donutsSocket.DonutSetRef(cwd + refFile);
            var outPut = donutsSocket.DonutGetShift(cwd + testFile);
            _logger.AddLogEntry("shifts = " + outPut[0] + "x " + outPut[1] + "y ");

        }

        #endregion

    }
}
