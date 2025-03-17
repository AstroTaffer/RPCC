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

        #region Cameras
        /// <summary>
        ///     Настройки камер
        /// </summary>

        private static string _snCamG;
        private static string _snCamR;
        private static string _snCamI;
        private static string _snCamV;
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
        
        public static string SnCamV
        {
            get => _snCamV;
            set
            {
                if (value.Length > 0) _snCamV = value;
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

        #region XmlIO
        internal static void LoadXmlConfig()
        {
            bool isConfigBad = false;
            DialogResult isRegenRequested = DialogResult.No;

            try
            {
                var config = XDocument.Load("Settings.xml");

                if (!(config.Root.Elements("cameras").Any() &&
                      config.Root.Elements("cameras").Elements("snCamG").Any() &&
                      config.Root.Elements("cameras").Elements("snCamR").Any() &&
                      config.Root.Elements("cameras").Elements("snCamI").Any() &&
                      config.Root.Elements("cameras").Elements("snCamV").Any() &&
                      config.Root.Elements("cameras").Elements("numFlushes").Any() &&
                      config.Root.Elements("cameras").Elements("camTemp").Any() &&

                      config.Root.Elements("survey").Any() &&
                      config.Root.Elements("survey").Elements("mainOutFolder").Any() &&

                      config.Root.Elements("comms").Any() &&
                      config.Root.Elements("comms").Elements("focusComId").Any() &&
                      config.Root.Elements("comms").Elements("meteoDomeTcpIpPort").Any() &&
                      config.Root.Elements("comms").Elements("donutsTcpIpPort").Any() &&
                      config.Root.Elements("comms").Elements("siTechExeTcpIpPort").Any()))
                    throw new NullReferenceException();

                SnCamG = (string)config.Root.Element("cameras").Element("snCamG");
                SnCamR = (string)config.Root.Element("cameras").Element("snCamR");
                SnCamI = (string)config.Root.Element("cameras").Element("snCamI");
                NumFlushes = (int)config.Root.Element("cameras").Element("numFlushes");
                CamTemp = (double)config.Root.Element("cameras").Element("camTemp");

                MainOutFolder = (string)config.Root.Element("survey").Element("mainOutFolder");

                FocusComId = (int)config.Root.Element("comms").Element("focusComId");
                MeteoDomeTcpIpPort = (int)config.Root.Element("comms").Element("meteoDomeTcpIpPort");
                DonutsTcpIpPort = (int)config.Root.Element("comms").Element("donutsTcpIpPort");
                SiTechExeTcpIpPort = (int)config.Root.Element("comms").Element("siTechExeTcpIpPort");

                Logger.AddLogEntry($"Config file loaded");
            }
            catch (FileNotFoundException)
            {
                isConfigBad = true;
                isRegenRequested = MessageBox.Show("Config file not found.\n"
                    + "Would you like to regenerate it (\"YES\")\n"
                    + "or just close the application (\"NO\")?",
                    "Config not found",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Error,
                    MessageBoxDefaultButton.Button1);
                Logger.AddLogEntry($"WARNING Config file not found");
            }
            catch (NullReferenceException)
            {
                isConfigBad = true;
                isRegenRequested = MessageBox.Show("Config file has invalid structure.\n"
                    + "Would you like to regenerate it (\"YES\")\n"
                    + "or just close the application (\"NO\")?",
                    "Invalid config structure",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Error,
                    MessageBoxDefaultButton.Button1);
                Logger.AddLogEntry($"WARNING Config file has invalid structure");
            }
            catch (Exception ex) when (ex is ArgumentException || ex is FormatException)
            {
                isConfigBad = true;
                isRegenRequested = MessageBox.Show("Config file has invalid parameters:\n"
                    + $"{ex.Message}.\n"
                    + "Would you like to regenerate it (\"YES\")\n"
                    + "or just close the application (\"NO\")?",
                    "Invalid config parameters",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Error,
                    MessageBoxDefaultButton.Button1);
                Logger.AddLogEntry($"WARNING Config file has invalid parameters");
            }
            finally
            {
                if (isConfigBad)
                {
                    switch (isRegenRequested)
                    {
                        case DialogResult.Yes:
                            RegeneratetXmlConfig();
                            LoadXmlConfig();
                            break;
                        case DialogResult.No:
                            Environment.Exit(1);
                            break;
                    }
                }
            }
        }

        internal static void RegeneratetXmlConfig()
        {
            var config = new XDocument(new XElement("settings",                
                new XElement("cameras",
                    new XElement("snCamG", "ML0882515"),
                    new XElement("snCamR", "ML0892515"),
                    new XElement("snCamI", "ML0742515"),
                    new XElement("snCamV", "Alta-U6"), // TODO find sn
                    new XElement("numFlushes", 5),
                    new XElement("camTemp", -20.0)),
                
                new XElement("survey",
                  new XElement("mainOutFolder", Directory.Exists("D:") ? 
                  "D:" : Directory.GetCurrentDirectory())),

                new XElement("comms",
                    new XElement("focusComId", 4),  // for server
                    new XElement("meteoDomeTcpIpPort", 8085),
                    new XElement("donutsTcpIpPort", 3030),
                    new XElement("siTechExeTcpIpPort", 8079)),
                
                new XElement("database",
                    new XElement("dbIP", "192.168.240.5"),
                    new XElement("dbPort", "5432"),
                    new XElement("dbUserId", "remote_user"),
                    new XElement("dbPassword", "remote_user"),
                    new XElement("database", "postgres")
                    )
            ));

            config.Save("SettingsDefault.xml");
            Logger.AddLogEntry("Default config file restored");
        }
        #endregion
    }
}