using System;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using System.Xml.Linq;

namespace RPCC.Utils
{
    internal class Settings
    {
        /// <summary>
        ///     Настройки приложения
        ///     Чтение и запись конфигурационных файлов
        /// </summary>

        private readonly Logger _logger;

        #region Analysis and Plotting

        /// <summary>
        ///     Настройки анализа и построения изображения
        /// </summary>

        private double _lowerBrightnessSd;
        private double _upperBrightnessSd;
        private int _apertureRadius;
        private int _annulusInnerRadius;
        private int _annulusOuterRadius;

        public double LowerBrightnessSd
        {
            get => _lowerBrightnessSd;
            set
            {
                if (value > 0.0) _lowerBrightnessSd = value;
                else throw new ArgumentException("Lower brightness SD must be greater than 0.0");
            }
        }

        public double UpperBrightnessSd
        {
            get => _upperBrightnessSd;
            set
            {
                if (value > 0.0) _upperBrightnessSd = value;
                else throw new ArgumentException("Upper brightness SD must be greater than 0.0");
            }
        }

        public int ApertureRadius
        {
            get => _apertureRadius;
            set
            {
                if (value > 0) _apertureRadius = value;
                else throw new ArgumentException("Aperture radius must be greater than 0");
            }
        }

        public int AnnulusInnerRadius
        {
            get => _annulusInnerRadius;
            set
            {
                // HACK: Stricter condition may be used - if (value > ApertureRadius)
                if (value > 0) _annulusInnerRadius = value;
                else throw new ArgumentException("Annulus inner radius must be greater than 0");
            }
        }

        public int AnnulusOuterRadius
        {
            get => _annulusOuterRadius;
            set
            {
                if (value > _annulusInnerRadius) _annulusOuterRadius = value;
                else throw new ArgumentException("Annulus outer radius must be greater than its inner radius ");
            }
        }

        #endregion

        #region Cameras
        /// <summary>
        ///     Настройки камер
        /// </summary>

        private string _snCamG;
        private string _snCamR;
        private string _snCamI;
        private int _numFlushes;
        private double _camTemp;
        private int _camBin;
        private string _camRoMode;

        public string SnCamG
        {
            get => _snCamG;
            set
            {
                if (value.Length > 0) _snCamG = value;
                else throw new ArgumentException("Camera serial number can't be an empty string");
            }
        }

        public string SnCamR
        {
            get => _snCamR;
            set
            {
                if (value.Length > 0) _snCamR = value;
                else throw new ArgumentException("Camera serial number can't be an empty string");
            }
        }

        public string SnCamI
        {
            get => _snCamI;
            set
            {
                if (value.Length > 0) _snCamI = value;
                else throw new ArgumentException("Camera serial number can't be an empty string");
            }
        }

        public int NumFlushes
        {
            get => _numFlushes;
            set
            {
                if (value >= 0 && value <= 16) _numFlushes = value;
                else throw new ArgumentException("Number of flushes must be set in range from 0 to 16");
            }
        }

        public double CamTemp
        {
            get => _camTemp;
            set
            {
                if (value >= -55.0 && value <= 45.0) _camTemp = value;
                else throw new ArgumentException("Camera temperature must be set in range from -55 to +45 degrees Celsius");
            }
        }

        public int CamBin
        {
            get => _camBin;
            set
            {
                if (value >= 1 && value <= 16) _camBin = value;
                else throw new ArgumentException("Camera bin factor must be set in range from 1 to 16");
            }
        }

        public string CamRoMode
        {
            get => _camRoMode;
            set
            {
                if (value == "2.0 MHz" || value == "500KHz") _camRoMode = value;
                else throw new ArgumentException("Camera mode must be set to \"2.0 MHz\" or \"500KHz\"");
            }
        }
        #endregion

        #region Survey
        /// <summary>
        ///     Настройки съёмки
        /// </summary>

        private string _outImgsFolder;

        public string OutImgsFolder
        {
            get => _outImgsFolder;
            set
            {
                if (Directory.Exists(value)) _outImgsFolder = value;
                else throw new ArgumentException("Selected output images folder does not exists");
                // Alternative - if (!Exists) CreateDirectory
            }
        }
        #endregion

        #region Comms
        /// <summary>
        ///     Настройки связей между другими приложениями, управляющими телескопом
        /// </summary>
        ///

        private int _focusComId;
        private int _meteoDomeTcpIpPort;
        private int _donutsTcpIpPort;
        private int _siTechExeTcpIpPort;

        public int FocusComId
        {
            get => _focusComId;
            set
            {
                if (value >= 1 || value <= 256) _focusComId = value;
                else throw new ArgumentException("Invalid COM port ID");
            }
        }

        public int MeteoDomeTcpIpPort
        {
            get => _meteoDomeTcpIpPort;
            set
            {
                if (value >= 0 || value <= 65535) _meteoDomeTcpIpPort = value;
                else throw new ArgumentException("Invalid TCP/IP port");
            }
        }

        public int DonutsTcpIpPort
        {
            get => _donutsTcpIpPort;
            set
            {
                if (value >= 0 || value <= 65535) _donutsTcpIpPort = value;
                else throw new ArgumentException("Invalid TCP/IP port");
            }
        }

        public int SiTechExeTcpIpPort
        {
            get => _siTechExeTcpIpPort;
            set
            {
                if (value >= 0 || value <= 65535) _siTechExeTcpIpPort = value;
                else throw new ArgumentException("Invalid TCP/IP port");
            }
        }

        #endregion

        internal Settings(Logger logger)
        {
            _logger = logger;
        }

        #region ConfigIO
        internal void LoadXmlConfig(string fileName)
        {
            try
            {
                var config = XDocument.Load(fileName);

                if (!(config.Root.Elements("image_analysis").Any() &&
                      config.Root.Elements("image_analysis").Elements("lowerBrightnessSd").Any() &&
                      config.Root.Elements("image_analysis").Elements("upperBrightnessSd").Any() &&
                      config.Root.Elements("image_analysis").Elements("apertureRadius").Any() &&
                      config.Root.Elements("image_analysis").Elements("annulusInnerRadius").Any() &&
                      config.Root.Elements("image_analysis").Elements("annulusOuterRadius").Any() &&

                      config.Root.Elements("cameras").Any() &&
                      config.Root.Elements("cameras").Elements("snCamG").Any() &&
                      config.Root.Elements("cameras").Elements("snCamR").Any() &&
                      config.Root.Elements("cameras").Elements("snCamI").Any() &&
                      config.Root.Elements("cameras").Elements("numFlushes").Any() &&
                      config.Root.Elements("cameras").Elements("camTemp").Any() &&
                      config.Root.Elements("cameras").Elements("camBin").Any() &&
                      config.Root.Elements("cameras").Elements("сamRoMode").Any() &&

                      config.Root.Elements("survey").Any() &&
                      config.Root.Elements("survey").Elements("outImgsFolder").Any() &&

                      config.Root.Elements("comms").Any() &&
                      config.Root.Elements("comms").Elements("focusComId").Any() &&
                      config.Root.Elements("comms").Elements("meteoDomeTcpIpPort").Any() &&
                      config.Root.Elements("comms").Elements("donutsTcpIpPort").Any() &&
                      config.Root.Elements("comms").Elements("siTechExeTcpIpPort").Any()))
                    throw new NullReferenceException();

                LowerBrightnessSd = (double)config.Root.Element("image_analysis").Element("lowerBrightnessSd");
                UpperBrightnessSd = (double)config.Root.Element("image_analysis").Element("upperBrightnessSd");
                ApertureRadius = (int)config.Root.Element("image_analysis").Element("apertureRadius");
                AnnulusInnerRadius = (int)config.Root.Element("image_analysis").Element("annulusInnerRadius");
                AnnulusOuterRadius = (int)config.Root.Element("image_analysis").Element("annulusOuterRadius");

                SnCamG = (string)config.Root.Element("cameras").Element("snCamG");
                SnCamR = (string)config.Root.Element("cameras").Element("snCamR");
                SnCamI = (string)config.Root.Element("cameras").Element("snCamI");
                NumFlushes = (int)config.Root.Element("cameras").Element("numFlushes");
                CamTemp = (double)config.Root.Element("cameras").Element("camTemp");
                CamBin = (int)config.Root.Element("cameras").Element("camBin");
                var buff = config.Root.Element("cameras");
                CamRoMode = (string)config.Root.Element("cameras").Element("сamRoMode");

                OutImgsFolder = (string)config.Root.Element("survey").Element("outImgsFolder");

                FocusComId = (int)config.Root.Element("comms").Element("focusComId");
                MeteoDomeTcpIpPort = (int)config.Root.Element("comms").Element("meteoDomeTcpIpPort");
                DonutsTcpIpPort = (int)config.Root.Element("comms").Element("donutsTcpIpPort");
                SiTechExeTcpIpPort = (int)config.Root.Element("comms").Element("siTechExeTcpIpPort");

                _logger.AddLogEntry($"Config file {fileName} loaded");
            }
            catch (FileNotFoundException)
            {
                if (fileName == "SettingsDefault.xml")
                {
                    DialogResult result = MessageBox.Show("Default config file not found.\n" +
                        "Do you want to restore it (\"YES\")\n" +
                        "or just close the application (\"NO\")?",
                        "Config not found",
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Error,
                        MessageBoxDefaultButton.Button1);
                    _logger.AddLogEntry($"WARNING Default config file not found");
                    if (result == DialogResult.Yes)
                    {
                        RestoreDefaultXmlConfig();
                        LoadXmlConfig("SettingsDefault.xml");
                    }
                    else Environment.Exit(1);
                }
                else
                {
                    MessageBox.Show($"Config file {fileName} not found.",
                        "Config not found",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error,
                        MessageBoxDefaultButton.Button1);
                    _logger.AddLogEntry($"WARNING Config file {fileName} not found");
                }
            }
            catch (NullReferenceException)
            {
                if (fileName == "SettingsDefault.xml")
                {
                    DialogResult result = MessageBox.Show("Default config file has invalid structure.\n" +
                        "Do you want to restore it (\"YES\")\n" +
                        "or just close the application (\"NO\")?",
                        "Invalid config structure",
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Error,
                        MessageBoxDefaultButton.Button1);
                    _logger.AddLogEntry($"WARNING Default config file has invalid structure");
                    if (result == DialogResult.Yes)
                    {
                        RestoreDefaultXmlConfig();
                        LoadXmlConfig("SettingsDefault.xml");
                    }
                    else Environment.Exit(1);
                }
                else
                {
                    MessageBox.Show($"Config file {fileName} has invalid structure.\n" +
                        $"Reverting to default config.",
                        "Invalid config structure",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error,
                        MessageBoxDefaultButton.Button1);
                    _logger.AddLogEntry($"WARNING Config file {fileName} has invalid structure");
                    LoadXmlConfig("SettingsDefault.xml");
                }
            }
            catch (Exception ex) when (ex is ArgumentException || ex is FormatException)
            {
                if (fileName == "SettingsDefault.xml")
                {
                    DialogResult result = MessageBox.Show("Default config file has invalid parameters:\n" +
                        $"{ex.Message}.\n" +
                        "Do you want to restore it (\"YES\")\n" +
                        "or just close the application (\"NO\")?",
                        "Invalid config parameters",
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Error,
                        MessageBoxDefaultButton.Button1);
                    _logger.AddLogEntry($"WARNING Default config file has invalid parameters");
                    if (result == DialogResult.Yes)
                    {
                        RestoreDefaultXmlConfig();
                        LoadXmlConfig("SettingsDefault.xml");
                    }
                    else Environment.Exit(1);
                }
                else
                {
                    MessageBox.Show($"Config file {fileName} has invalid parameters:\n" +
                        $"{ex.Message}.\n" +
                        $"Reverting to default config.",
                        "Invalid config parameters",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error,
                        MessageBoxDefaultButton.Button1);
                    _logger.AddLogEntry($"WARNING Config file {fileName} has invalid parameters");
                    LoadXmlConfig("SettingsDefault.xml");
                }
            }
        }

        internal void SaveXmlConfig(string fileName)
        {
            var config = new XDocument(new XElement("settings",
                new XElement("image_analysis",
                    new XElement("lowerBrightnessSd", LowerBrightnessSd),
                    new XElement("upperBrightnessSd", UpperBrightnessSd),
                    new XElement("apertureRadius", ApertureRadius),
                    new XElement("annulusInnerRadius", AnnulusInnerRadius),
                    new XElement("annulusOuterRadius", AnnulusOuterRadius)),
                
                new XElement("cameras",
                    new XElement("snCamG", SnCamG),
                    new XElement("snCamR", SnCamR),
                    new XElement("snCamI", SnCamI),
                    new XElement("numFlushes", NumFlushes),
                    new XElement("camTemp", CamTemp),
                    new XElement("camBin", CamBin),
                    new XElement("сamRoMode", CamRoMode)),
                
                new XElement("survey",
                    new XElement("outImgsFolder", OutImgsFolder)),

                new XElement("comms",
                    new XElement("focusComId", FocusComId),
                    new XElement("meteoDomeTcpIpPort", MeteoDomeTcpIpPort),
                    new XElement("donutsTcpIpPort", DonutsTcpIpPort),
                    new XElement("siTechExeTcpIpPort", SiTechExeTcpIpPort))
            ));

            config.Save(fileName);
            _logger.AddLogEntry($"Config file {fileName} saved");
        }

        internal void RestoreDefaultXmlConfig()
        {
            var config = new XDocument(new XElement("settings",
                new XElement("image_analysis",
                    new XElement("lowerBrightnessSd", 1.0),
                    new XElement("upperBrightnessSd", 2.0),
                    new XElement("apertureRadius", 6),
                    new XElement("annulusInnerRadius", 10),
                    new XElement("annulusOuterRadius", 15)),
                
                new XElement("cameras",
                    new XElement("snCamG", "ML0892515"),
                    new XElement("snCamR", "ML0882515"),
                    new XElement("snCamI", "ML0742515"),
                    new XElement("numFlushes", 5),
                    new XElement("camTemp", -5.0),
                    new XElement("camBin", 3),
                    new XElement("сamRoMode", "500KHz")),
                
                new XElement("survey",
                    new XElement("outImgsFolder", Directory.GetCurrentDirectory())),

                new XElement("comms",
                    new XElement("focusComId", 10),
                    new XElement("meteoDomeTcpIpPort", 8085),
                    new XElement("donutsTcpIpPort", 3030),
                    new XElement("siTechExeTcpIpPort", 8079))
            ));

            config.Save("SettingsDefault.xml");
            _logger.AddLogEntry("Default config file restored");
        }
        #endregion
    }
}