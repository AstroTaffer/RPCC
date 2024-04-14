using System;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using System.Xml.Linq;

namespace RPCC.Utils
{
    internal static class Settings
    {
        /// <summary>
        ///     Настройки приложения
        ///     Чтение и запись конфигурационных файлов
        /// </summary>

        #region Analysis and Plotting

        /// <summary>
        ///     Настройки анализа и построения изображения
        /// </summary>

        private static double _lowerBrightnessSd;
        private static double _upperBrightnessSd;
        private static int _apertureRadius;
        private static int _annulusInnerRadius;
        private static int _annulusOuterRadius;

        public static double LowerBrightnessSd
        {
            get => _lowerBrightnessSd;
            set
            {
                if (value > 0.0) _lowerBrightnessSd = value;
                else throw new ArgumentException("Lower brightness SD must be greater than 0.0");
            }
        }

        public static double UpperBrightnessSd
        {
            get => _upperBrightnessSd;
            set
            {
                if (value > 0.0) _upperBrightnessSd = value;
                else throw new ArgumentException("Upper brightness SD must be greater than 0.0");
            }
        }

        public static int ApertureRadius
        {
            get => _apertureRadius;
            set
            {
                if (value > 0) _apertureRadius = value;
                else throw new ArgumentException("Aperture radius must be greater than 0");
            }
        }

        public static int AnnulusInnerRadius
        {
            get => _annulusInnerRadius;
            set
            {
                // HACK: Stricter condition may be used - if (value > ApertureRadius)
                if (value > 0) _annulusInnerRadius = value;
                else throw new ArgumentException("Annulus inner radius must be greater than 0");
            }
        }

        public static int AnnulusOuterRadius
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

        private static string _snCamG;
        private static string _snCamR;
        private static string _snCamI;
        private static int _numFlushes;
        private static double _camTemp;

        public static string SnCamG
        {
            get => _snCamG;
            set
            {
                if (value.Length > 0) _snCamG = value;
                else throw new ArgumentException("Camera serial number can't be an empty string");
            }
        }

        public static string SnCamR
        {
            get => _snCamR;
            set
            {
                if (value.Length > 0) _snCamR = value;
                else throw new ArgumentException("Camera serial number can't be an empty string");
            }
        }

        public static string SnCamI
        {
            get => _snCamI;
            set
            {
                if (value.Length > 0) _snCamI = value;
                else throw new ArgumentException("Camera serial number can't be an empty string");
            }
        }

        public static int NumFlushes
        {
            get => _numFlushes;
            set
            {
                if (value >= 0 && value <= 16) _numFlushes = value;
                else throw new ArgumentException("Number of flushes must be set in range from 0 to 16");
            }
        }

        public static double CamTemp
        {
            get => _camTemp;
            set
            {
                if (value >= -55.0 && value <= 45.0) _camTemp = value;
                else throw new ArgumentException("Camera temperature must be set in range from -55 to +45 degrees Celsius");
            }
        }
        #endregion

        #region Survey
        /// <summary>
        ///     Настройки съёмки
        /// </summary>

        private static string _mainOutFolder;
        private static DateTime _lastDarksTime;
        private static DateTime _lastFlatsTime;

        public static string MainOutFolder
        {
            get => _mainOutFolder;
            set
            {
                if (Directory.Exists(value)) _mainOutFolder = value;
                else throw new ArgumentException("Selected output images folder does not exists");
                // Alternative - if (!Exists) CreateDirectory
            }
        }

        public static DateTime LastDarksTime
        {
            get => _lastDarksTime;
            set
            {
                _lastDarksTime = value;
            }
        }

        public static DateTime LastFlatsTime
        {
            get => _lastFlatsTime;
            set
            {
                _lastFlatsTime = value;
            }
        }
        #endregion

        #region Comms
        /// <summary>
        ///     Настройки связей между другими приложениями, управляющими телескопом
        /// </summary>
        ///

        private static int _focusComId;
        private static int _meteoDomeTcpIpPort;
        private static int _donutsTcpIpPort;
        private static int _siTechExeTcpIpPort;

        public static int FocusComId
        {
            get => _focusComId;
            set
            {
                if (value >= 1 || value <= 256) _focusComId = value;
                else throw new ArgumentException("Invalid COM port ID");
            }
        }

        public static int MeteoDomeTcpIpPort
        {
            get => _meteoDomeTcpIpPort;
            set
            {
                if (value >= 0 || value <= 65535) _meteoDomeTcpIpPort = value;
                else throw new ArgumentException("Invalid TCP/IP port");
            }
        }

        public static int DonutsTcpIpPort
        {
            get => _donutsTcpIpPort;
            set
            {
                if (value >= 0 || value <= 65535) _donutsTcpIpPort = value;
                else throw new ArgumentException("Invalid TCP/IP port");
            }
        }

        public static int SiTechExeTcpIpPort
        {
            get => _siTechExeTcpIpPort;
            set
            {
                if (value >= 0 || value <= 65535) _siTechExeTcpIpPort = value;
                else throw new ArgumentException("Invalid TCP/IP port");
            }
        }

        #endregion

        #region ConfigIO
        internal static void LoadXmlConfig(string fileName)
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

                      config.Root.Elements("survey").Any() &&
                      config.Root.Elements("survey").Elements("mainOutFolder").Any() &&
                      config.Root.Elements("survey").Elements("lastDarksTime").Any() &&
                      config.Root.Elements("survey").Elements("lastFlatsTime").Any() &&

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

                MainOutFolder = (string)config.Root.Element("survey").Element("mainOutFolder");
                LastDarksTime = (DateTime)config.Root.Element("survey").Element("lastDarksTime");
                LastFlatsTime = (DateTime)config.Root.Element("survey").Element("lastFlatsTime");

                FocusComId = (int)config.Root.Element("comms").Element("focusComId");
                MeteoDomeTcpIpPort = (int)config.Root.Element("comms").Element("meteoDomeTcpIpPort");
                DonutsTcpIpPort = (int)config.Root.Element("comms").Element("donutsTcpIpPort");
                SiTechExeTcpIpPort = (int)config.Root.Element("comms").Element("siTechExeTcpIpPort");

                Logger.AddLogEntry($"Config file {fileName} loaded");
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
                    Logger.AddLogEntry($"WARNING Default config file not found");
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
                    Logger.AddLogEntry($"WARNING Config file {fileName} not found");
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
                    Logger.AddLogEntry($"WARNING Default config file has invalid structure");
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
                    Logger.AddLogEntry($"WARNING Config file {fileName} has invalid structure");
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
                    Logger.AddLogEntry($"WARNING Default config file has invalid parameters");
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
                    Logger.AddLogEntry($"WARNING Config file {fileName} has invalid parameters");
                    LoadXmlConfig("SettingsDefault.xml");
                }
            }
        }

        internal static void SaveXmlConfig(string fileName)
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
                    new XElement("camTemp", CamTemp)),
                
                new XElement("survey",
                    new XElement("mainOutFolder", MainOutFolder),
                    new XElement("lastDarksTime", LastDarksTime), 
                    new XElement("lastFlatsTime", LastFlatsTime)),

                new XElement("comms",
                    new XElement("focusComId", FocusComId),
                    new XElement("meteoDomeTcpIpPort", MeteoDomeTcpIpPort),
                    new XElement("donutsTcpIpPort", DonutsTcpIpPort),
                    new XElement("siTechExeTcpIpPort", SiTechExeTcpIpPort))
            ));

            config.Save(fileName);
            Logger.AddLogEntry($"Config file {fileName} saved");
        }

        internal static void RestoreDefaultXmlConfig()
        {
            var config = new XDocument(new XElement("settings",
                new XElement("image_analysis",
                    new XElement("lowerBrightnessSd", 1.0),
                    new XElement("upperBrightnessSd", 2.0),
                    new XElement("apertureRadius", 6),
                    new XElement("annulusInnerRadius", 10),
                    new XElement("annulusOuterRadius", 15)),
                
                new XElement("cameras",
                    new XElement("snCamG", "ML0882515"),
                    new XElement("snCamR", "ML0892515"),
                    new XElement("snCamI", "ML0742515"),
                    new XElement("numFlushes", 5),
                    new XElement("camTemp", -20.0)),
                
                new XElement("survey",
                  new XElement("mainOutFolder", Directory.Exists("D:") ? 
                  "D:" : Directory.GetCurrentDirectory()),
                  new XElement("lastDarksTime", LastDarksTime), 
                  new XElement("lastFlatsTime", LastFlatsTime)),

                new XElement("comms",
                    //new XElement("focusComId", 10),  // for monoblock
                    new XElement("focusComId", 4),  // for server
                    new XElement("meteoDomeTcpIpPort", 8085),
                    new XElement("donutsTcpIpPort", 3030),
                    new XElement("siTechExeTcpIpPort", 8079))
            ));

            config.Save("SettingsDefault.xml");
            Logger.AddLogEntry("Default config file restored");
        }
        #endregion
    }
}