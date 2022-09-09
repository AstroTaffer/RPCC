using System;
using System.Windows.Forms;
using System.Drawing;

using FliProCameraLib;

using nom.tam.fits;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.TrayNotify;

namespace RPCC
{
    public partial class MainForm : Form
    {
        /// <summary>
        ///     Логика работы основной формы программы.
        ///     Обработка команд пользователя с помощью вызова готовых функций.
        /// </summary>

        internal Settings settings;
        // HACK: Check for memory leak and refreshing error - if occurs, try use this variable as a reference to another
        internal ushort[][] currentImage = null;
        internal bool isCurrentImageLoaded = false;
        // FIXME: If new image is loaded, flag must be reset to false and imageBox must be tuned accordingly
        internal bool isZoomModeActivated = false;

        public MainForm()
        {
            InitializeComponent();
            timerClock.Enabled = true;
            AddLogEntry("Application launched");

            settings = new Settings();
            try
            {
                settings.LoadXmlConfig("SettingsDefault.xml");
                AddLogEntry("Default config loaded succesfully");
            }
            catch (System.IO.FileNotFoundException)
            {
                AddLogEntry("WARNING Default config file not found");
            }
        }

        private void TimerClock_Tick(object sender, EventArgs e)
        {
            tSStatusClock.Text = DateTime.UtcNow.ToString("yyyy-MM-ddTHH-mm-ss.fff");
        }

        #region Logs
        private void AddLogEntry(string entry)
        {
            if (listBoxLogs.Items.Count >= 1024)
            {
                SaveLogs();
                listBoxLogs.Items.Clear();
                listBoxLogs.Items.Insert(0, $"{DateTime.UtcNow:G} Logs have been saved and cleaned");
            }
            listBoxLogs.Items.Insert(0, $"{DateTime.UtcNow:G} {entry}");
        }

        private void SaveLogs()
        {
            string logsFilePath = $"Logs {DateTime.UtcNow.ToString("yyyy-MM-ddTHH-mm-ss")}.txt";
            System.IO.StreamWriter sw = new System.IO.StreamWriter(logsFilePath);
            foreach (string item in listBoxLogs.Items)
            {
                sw.WriteLine(item);
            }
            sw.Close();
        }

        private void ButtonLogsClear_Click(object sender, EventArgs e)
        {
            listBoxLogs.Items.Clear();
            AddLogEntry("Logs have been cleaned");
        }

        private void ButtonLogsSave_Click(object sender, EventArgs e)
        {
            SaveLogs();
            AddLogEntry("Logs have been saved");
        }
        #endregion

        #region Images Plotting
        private void PlotFitsImage(GeneralImageStat gStat)
        {
            pictureBoxFits.Image = null;
            pictureBoxProfile.Image = null;

            double lowerBrightnessBorder = gStat.mean - settings.LowerBrightnessSd * gStat.sd;
            if (lowerBrightnessBorder < gStat.min)
            {
                lowerBrightnessBorder = gStat.min;
            }

            double upperBrightnessBorder = gStat.mean + settings.UpperBrightnessSd * gStat.sd;
            if (upperBrightnessBorder > gStat.max)
            {
                upperBrightnessBorder = gStat.max;
            }

            upperBrightnessBorder -= lowerBrightnessBorder;
            double colorScale = 255 / upperBrightnessBorder;

            Bitmap bitmapFits = new Bitmap(currentImage.Length, currentImage[0].Length, System.Drawing.Imaging.PixelFormat.Format24bppRgb);

            int pixelColor;
            for (ushort i = 0; i < currentImage.Length; i++)
            {
                for (ushort j = 0; j < currentImage[i].Length; j++)
                {
                    pixelColor = (int)((currentImage[i][j] - lowerBrightnessBorder) * colorScale);
                    if (pixelColor < 0)
                    {
                        pixelColor = 0;
                    }
                    if (pixelColor > 255)
                    {
                        pixelColor = 255;
                    }
                    bitmapFits.SetPixel(j, i, Color.FromArgb(pixelColor, pixelColor, pixelColor));
                }
            }

            pictureBoxFits.Image = bitmapFits;
            isCurrentImageLoaded = true;
        }

        private void PlotProfileImage(ref Settings settings, ProfileImageStat pStat, ushort[][] image)
        {
            // TODO: Find out why does it have to be calculated this specific way
            // TODO: Add profile image scaling and placing parameters
            Bitmap bitmapProfile = new Bitmap(pictureBoxProfile.Width, pictureBoxProfile.Height, System.Drawing.Imaging.PixelFormat.Format24bppRgb);

            double pixelFlux;
            double pixelRadius;
            int pixelBitmapXCoordinate;
            int pixelBitmapYCoordinate;
            double bitmapScaleX = pictureBoxProfile.Width / (settings.AnnulusOuterRadius * Math.Sqrt(2));
            double bitmapScaleY = (pictureBoxProfile.Height * 0.9) / (pStat.maxValue - pStat.background);
            for (int i = 0; i < image.Length; i++)
                for (int j = 0; j < image[i].Length; j++)
                {
                    pixelFlux = image[i][j] - pStat.background;
                    pixelBitmapYCoordinate = pictureBoxProfile.Height -
                        (int)Math.Round(pictureBoxProfile.Height * 0.05 + pixelFlux * bitmapScaleY);
                    
                    pixelRadius = Math.Sqrt(Math.Pow((i - pStat.centroidYCoordinate), 2) + Math.Pow(j - pStat.centroidXCoordinate, 2));
                    pixelBitmapXCoordinate = (int)Math.Round(pixelRadius * bitmapScaleX);

                    if (pixelBitmapXCoordinate >= 0 &&
                        pixelBitmapXCoordinate < pictureBoxProfile.Width &&
                        pixelBitmapYCoordinate >= 0 &&
                        pixelBitmapYCoordinate < pictureBoxProfile.Height)
                    {
                        bitmapProfile.SetPixel(pixelBitmapXCoordinate, pixelBitmapYCoordinate,
                            Color.FromArgb(255, 255, 255));
                    }
                }

            int apertureBitmapXCoordinate = (int)Math.Round(settings.ApertureRadius * bitmapScaleX);
            int annulusInnerRadiusBitmapXCoordinate = (int)Math.Round(settings.AnnulusInnerRadius * bitmapScaleX);
            for (int i = 0; i < pictureBoxProfile.Height; i++)
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

                    int xCoordinate = (int)((double)e.X / pictureBoxFits.Width * currentImage[0].Length);
                    int yCoordinate = (int)((double)e.Y / pictureBoxFits.Height * currentImage.Length);
                    AddLogEntry($"Pixel ({xCoordinate}, {yCoordinate}) selected");

                    if ((xCoordinate - settings.AnnulusOuterRadius) < 0 ||
                        (xCoordinate + settings.AnnulusOuterRadius) > (currentImage[0].Length - 1) ||
                        (yCoordinate - settings.AnnulusOuterRadius) < 0 ||
                        (yCoordinate + settings.AnnulusOuterRadius) > (currentImage.Length - 1))
                    {
                        AddLogEntry("WARNING Pixel is too close to the frame borders");
                    }
                    else
                    {
                        ushort[][] subProfileImage = new ushort[2 * settings.AnnulusOuterRadius + 1][];
                        for (int i = 0; i < subProfileImage.Length; i++)
                        {
                            subProfileImage[i] = new ushort[2 * settings.AnnulusOuterRadius + 1];
                            for (int j = 0; j < subProfileImage[i].Length; j++)
                            {
                                subProfileImage[i][j] = 
                                    currentImage[yCoordinate - settings.AnnulusOuterRadius + i]
                                    [xCoordinate - settings.AnnulusOuterRadius + j];
                            }
                        }

                        ProfileImageStat localStat = new ProfileImageStat(subProfileImage, ref settings);
                        AddLogEntry($"Maximum: {localStat.maxValue} " +
                            $"({localStat.maxXCoordinate + xCoordinate - settings.AnnulusOuterRadius}, " +
                            $"{localStat.maxYCoordinate + yCoordinate - settings.AnnulusOuterRadius})");
                        AddLogEntry($"Background: {localStat.background:0.#}");
                        AddLogEntry($"Centroid: ({localStat.centroidXCoordinate + xCoordinate - settings.AnnulusOuterRadius:0.#}, " +
                            $"{localStat.centroidYCoordinate + yCoordinate - settings.AnnulusOuterRadius:0.#})");
                        AddLogEntry($"SNR: {localStat.snr:0.#}");
                        AddLogEntry($"FWHM: {localStat.fwhm:0.##}");
                        PlotProfileImage(ref settings, localStat, subProfileImage);
                        AddLogEntry("Profile picture plotted succesfully");
                    }
                }

            }
            else
            {
                AddLogEntry("WARNING Image not loaded");
            }
        }
        #endregion

        #region Options
        private void SettingsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // TODO: Think of a way to restrict user acess to settings when busy to prevent potential bugs
            SettingsForm settingsForm = new SettingsForm(ref settings);
            settingsForm.ShowDialog();
            if (settingsForm.DialogResult == DialogResult.OK)
            {
                AddLogEntry("Settings changed succesfully");
            }
        }
        
        private void LoadConfigToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (openFileDialogConfig.ShowDialog() == DialogResult.OK)
            {
                settings.LoadXmlConfig(openFileDialogConfig.FileName);
                AddLogEntry($"Config file {saveFileDialogConfig.FileName} loaded succesfully");
            }
        }

        private void SaveConfigToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (saveFileDialogConfig.ShowDialog() == DialogResult.OK)
            {
                settings.SaveXmlConfig(saveFileDialogConfig.FileName);
                AddLogEntry($"Config file {saveFileDialogConfig.FileName} saved succesfully");
            }
        }
        #endregion

        #region Debug Menu
        private void AddTestLogEntryToolStripMenuItem_Click(object sender, EventArgs e)
        {
            AddLogEntry("Two things fill the mind with ever new and increasing admiration and awe, " +
                "the more often and steadily we reflect upon them: " +
                "the starry heavens above me and the moral law within me");
        }

        private void TestDLLlibrariesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Fits templateFits = new Fits("TemplateHeader.fits");
            Header templateHeader = templateFits.GetHDU(0).Header;
            AddLogEntry($"Template DATE-OBS: {templateHeader.GetStringValue("DATE-OBS")}");
            templateFits.Close();

            System.Text.StringBuilder libVer = new System.Text.StringBuilder(128);
            IntPtr len = new IntPtr(128);
            int errorLastFliCmd = NativeMethods.FLIGetLibVersion(libVer, len);
            if (errorLastFliCmd == 0)
            {
                AddLogEntry(libVer.ToString());
            }
            else
            {
                AddLogEntry("WARNING Unable to retrieve FLI library version");
            }
        }

        private void LoadTestImageToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Fits testFits = new Fits("TestImage.fits");
            ImageHDU testHdu = (ImageHDU)testFits.ReadHDU();
            Array[] testImageRaw = (Array[])testHdu.Kernel;
            testFits.Close();
            
            int imgHeight = testImageRaw.Length;
            int imgWidth = testImageRaw[0].Length;
            currentImage = new ushort[imgHeight][];
            for (int i=0; i < imgHeight; i++)
            {
                currentImage[i] = new ushort[imgHeight];
                for (int j=0; j < imgWidth; j++)
                {
                    // HACK: Why is this needed and how does it work?
                    int buffData = short.MaxValue + (short)testImageRaw[i].GetValue(j) + 1;
                    currentImage[i][j] = (ushort)buffData;
                }
            }
            AddLogEntry("Test image loaded succesfully");

            GeneralImageStat testStat = new GeneralImageStat(currentImage);
            AddLogEntry($"Minimum: {testStat.min}");
            AddLogEntry($"Maximum: {testStat.max}");
            AddLogEntry($"Mean: {testStat.mean:0.##}");
            AddLogEntry($"SD: {testStat.sd:0.##}");

            PlotFitsImage(testStat);
            AddLogEntry("Test image plotted succesfully");
        }

        private void RestoreDefaultConfigFileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            settings.RestoreDefaultXmlConfig();
            AddLogEntry("Default config file restored succesfully");
        }
        #endregion
    }
}

/*
 Добавить считывание координат курсора "на лету" при наведении на pictureBoxFits
 */