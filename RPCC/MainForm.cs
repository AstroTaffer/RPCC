using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Text;
using System.Windows.Forms;
using System.Threading.Tasks;
using FliProCameraLib;

namespace RPCC
{
    public partial class MainForm : Form
    {
        /// <summary>
        ///     Логика работы основной формы программы.
        ///     Обработка команд пользователя с помощью вызова готовых функций.
        /// </summary>
        private ushort[][] currentImage;

        // HACK: Check for memory leak and refreshing error - if occurs, try use this variable as a reference to another
        private GeneralImageStat currentImageGStat;
        private int _idleCamNum;

        private bool isCurrentImageLoaded;
        
        // FIXME: If new image is loaded, flag must be reset to false and imageBox must be tuned accordingly
        private bool isZoomModeActivated;

        private readonly Logger logger;
        private Settings settings;

        private CameraControl _cameraControl;
        private CameraFocus _cameraFocus;

        private RpccSocketClient domeSocket;
        private RpccSocketClient donutsSocket;


        public MainForm()
        {
            InitializeComponent();

            timerClock.Enabled = true;
            comboBoxImgType.SelectedIndex = 0;

            logger = new Logger(listBoxLogs);
            logger.AddLogEntry("Application launched");

            settings = new Settings();
            try
            {
                settings.LoadXmlConfig("SettingsDefault.xml");
                logger.AddLogEntry("Default config loaded successfully");
            }
            catch (FileNotFoundException)
            {
                logger.AddLogEntry("WARNING Default config file not found");
            }
            
            StartDonutsPy();

            _cameraControl = new CameraControl(logger, settings);
            _cameraFocus = new CameraFocus(logger);

            domeSocket = new RpccSocketClient(logger, "dom");
            domeSocket.Connect();
            donutsSocket = new RpccSocketClient(logger, "don");

            // if (!_cameraFocus.Init())  
            //     if (MessageBox.Show(@"Can't open Focus serial port", @"OK", MessageBoxButtons.OK) == DialogResult.OK)
            //         Environment.Exit(1);
        }

        private static void StartDonutsPy()
        {
            var cwd = Directory.GetCurrentDirectory();
            var start = new ProcessStartInfo
            {
                FileName = @"C:\Users\Nikita\AppData\Local\Programs\Python\Python310\python.exe", //cmd is full path to python.exe
                Arguments = cwd + @"\DONUTS.py", //args is path to .py file and any cmd line args
                UseShellExecute = false,
                RedirectStandardOutput = true
            };
            using(var process = Process.Start(start))
            {
                if (process == null) return;
                using (var reader = process.StandardOutput)
                {
                    var result = reader.ReadToEnd();
                    Console.Write(result);
                }
            }
        }

        #region General

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            timerClock.Stop();
            _cameraControl.DisconnectCameras();
            _cameraFocus.SerialFocus.Close_Port();
            domeSocket.DisconnectAll();
            donutsSocket.DisconnectAll();
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

                        // SaveFits

                        if (i == _cameraControl.task.viewCamIndex)
                        {
                            isCurrentImageLoaded = false;
                            currentImage = imageFits.data;
                            PlotFitsImage();
                            isCurrentImageLoaded = true;
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
                }
            }
        }

        #endregion

        #region Launch

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

        #endregion

        #region Logs

        private void ButtonLogsClear_Click(object sender, EventArgs e)
        {
            logger.ClearLogs();
            logger.AddLogEntry("Logs have been cleaned");
        }

        private void ButtonLogsSave_Click(object sender, EventArgs e)
        {
            logger.SaveLogs();
            logger.AddLogEntry("Logs have been saved");
        }

        #endregion

        #region Images Plotting

        private void PlotFitsImage()
        {
            pictureBoxFits.Image = null;
            pictureBoxProfile.Image = null;

            var lowerBrightnessBorder = currentImageGStat.mean - settings.LowerBrightnessSd * currentImageGStat.sd;
            if (lowerBrightnessBorder < currentImageGStat.min) lowerBrightnessBorder = currentImageGStat.min;

            var upperBrightnessBorder = currentImageGStat.mean + settings.UpperBrightnessSd * currentImageGStat.sd;
            if (upperBrightnessBorder > currentImageGStat.max) upperBrightnessBorder = currentImageGStat.max;

            upperBrightnessBorder -= lowerBrightnessBorder;
            var colorScale = 255 / upperBrightnessBorder;

            var bitmapFits = new Bitmap(currentImage.Length, currentImage[0].Length, PixelFormat.Format24bppRgb);

            int pixelColor;
            for (ushort i = 0; i < currentImage.Length; i++)
            for (ushort j = 0; j < currentImage[i].Length; j++)
            {
                pixelColor = (int) ((currentImage[i][j] - lowerBrightnessBorder) * colorScale);
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
            if (isCurrentImageLoaded)
            {
                if (e.Button == MouseButtons.Right)
                {
                    isZoomModeActivated = !isZoomModeActivated;
                    if (isZoomModeActivated)
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

                    var xCoordinate = (int) ((double) e.X / pictureBoxFits.Width * currentImage[0].Length);
                    var yCoordinate = (int) ((double) e.Y / pictureBoxFits.Height * currentImage.Length);
                    logger.AddLogEntry($"Pixel ({xCoordinate}, {yCoordinate}) selected");

                    if (xCoordinate - settings.AnnulusOuterRadius < 0 ||
                        xCoordinate + settings.AnnulusOuterRadius > currentImage[0].Length - 1 ||
                        yCoordinate - settings.AnnulusOuterRadius < 0 ||
                        yCoordinate + settings.AnnulusOuterRadius > currentImage.Length - 1)
                    {
                        logger.AddLogEntry("WARNING Pixel is too close to the frame borders");
                    }
                    else
                    {
                        var subProfileImage = new ushort[2 * settings.AnnulusOuterRadius + 1][];
                        for (var i = 0; i < subProfileImage.Length; i++)
                        {
                            subProfileImage[i] = new ushort[2 * settings.AnnulusOuterRadius + 1];
                            for (var j = 0; j < subProfileImage[i].Length; j++)
                                subProfileImage[i][j] =
                                    currentImage[yCoordinate - settings.AnnulusOuterRadius + i]
                                        [xCoordinate - settings.AnnulusOuterRadius + j];
                        }

                        var localStat = new ProfileImageStat(subProfileImage, ref settings);
                        logger.AddLogEntry($"Maximum: {localStat.maxValue} " +
                                           $"({localStat.maxXCoordinate + xCoordinate - settings.AnnulusOuterRadius}, " +
                                           $"{localStat.maxYCoordinate + yCoordinate - settings.AnnulusOuterRadius})");
                        logger.AddLogEntry($"Background: {localStat.background:0.#}");
                        logger.AddLogEntry(
                            $"Centroid: ({localStat.centroidXCoordinate + xCoordinate - settings.AnnulusOuterRadius:0.#}, " +
                            $"{localStat.centroidYCoordinate + yCoordinate - settings.AnnulusOuterRadius:0.#})");
                        logger.AddLogEntry($"SNR: {localStat.snr:0.#}");
                        logger.AddLogEntry($"FWHM: {localStat.fwhm:0.##}");
                        PlotProfileImage(ref settings, localStat, subProfileImage);
                        logger.AddLogEntry("Profile image plotted successfully");
                    }
                }
            }
            else
            {
                logger.AddLogEntry("WARNING Image not loaded");
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
        }

        #endregion

        #region Options

        private void SettingsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var settingsForm = new SettingsForm(settings);
            settingsForm.ShowDialog();
            if (settingsForm.DialogResult == DialogResult.OK) logger.AddLogEntry("Settings changed successfully");
        }

        private void UpdateCameraSettingsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            _cameraControl.UpdateSettings();
        }

        private void LoadConfigToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (openFileDialogConfig.ShowDialog() == DialogResult.OK)
            {
                settings.LoadXmlConfig(openFileDialogConfig.FileName);
                logger.AddLogEntry($"Config file {saveFileDialogConfig.FileName} loaded successfully");
            }
        }

        private void SaveConfigToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (saveFileDialogConfig.ShowDialog() == DialogResult.OK)
            {
                settings.SaveXmlConfig(saveFileDialogConfig.FileName);
                logger.AddLogEntry($"Config file {saveFileDialogConfig.FileName} saved successfully");
            }
        }

        #endregion

        #region Debug Menu

        private void TestDLLlibrariesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var templateFits = new RpccFits("TemplateHeader.fits");
            var templateHeader = templateFits.header;
            logger.AddLogEntry($"Template DATE-OBS: {templateHeader.GetStringValue("DATE-OBS")}");

            var libVer = new StringBuilder(128);
            var len = new IntPtr(128);
            var errorLastFliCmd = NativeMethods.FLIGetLibVersion(libVer, len);
            if (errorLastFliCmd == 0)
                logger.AddLogEntry(libVer.ToString());
            else
                logger.AddLogEntry("WARNING Unable to retrieve FLI library version");
        }

        private async void LoadTestImageToolStripMenuItem_Click(object sender, EventArgs e)
        {
            isCurrentImageLoaded = false;
            var testFits = new RpccFits("TestImage.fits");
            currentImage = testFits.data;

            currentImageGStat = new GeneralImageStat(currentImage);
            logger.AddLogEntry($"Minimum: {currentImageGStat.min}");
            logger.AddLogEntry($"Maximum: {currentImageGStat.max}");
            logger.AddLogEntry($"Mean: {currentImageGStat.mean:0.##}");
            logger.AddLogEntry($"SD: {currentImageGStat.sd:0.##}");

            await Task.Run(() => PlotFitsImage());
            isCurrentImageLoaded = true;
            logger.AddLogEntry("Test image plotted successfully");
        }

        private void RestoreDefaultConfigFileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            settings.RestoreDefaultXmlConfig();
            logger.AddLogEntry("Default config file restored successfully");
        }

        #endregion

        private void reconnectSocketsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            domeSocket.DisconnectAll();
            donutsSocket.DisconnectAll();
            domeSocket.Connect();
            donutsSocket.Connect();
        }
    }
}