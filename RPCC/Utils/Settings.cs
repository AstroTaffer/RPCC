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
            DialogResult isResetRequested = DialogResult.No;

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
                SnCamV = (string)config.Root.Element("cameras").Element("snCamV");
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
                isResetRequested = MessageBox.Show("Config file not found.\n"
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
                isResetRequested = MessageBox.Show("Config file has invalid structure.\n"
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
                isResetRequested = MessageBox.Show("Config file has invalid parameters:\n"
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
                    switch (isResetRequested)
                    {
                        case DialogResult.Yes:
                            ResetXmlConfig();
                            LoadXmlConfig();
                            break;
                        case DialogResult.No:
                            Environment.Exit(1);
                            break;
                    }
                }
            }
        }

        internal static void ResetXmlConfig()
        {
            var config = new XElement("Settings",
                new XElement("Cameras",
                    new XComment(" [Gain] = e/ADU, [Rate] = kPix/sec, [RdNoise] = e "),
                    new XElement("Camera",
                        new XAttribute("SerialNumber", "UNKNOWN"),
                        new XComment(" This is the default/fallback settings set. Do not delete it! "),
                        new XElement("Filter", "UNKNOWN"),
                        new XElement("TempSetpoint", -20.0),
                        new XElement("Bin", 1),
                        new XElement("ExpStartFlushesNum", 5),
                        new XElement("Model", "UNKNOWN"),
                        new XElement("Detector", "UNKNOWN"),
                        new XElement("Gain", 1.4),
                        new XElement("Rate", 500.0),
                        new XElement("RdNoise", 14.0)
                        )
                    ),
                new XElement("Survey",
                    new XComment(" [TaskPrepDuration] = min, [ClbTasksTimeout] = d, [Exp] = sec "),
                    new XElement("MainOutputFolder", Directory.Exists("D:")
                        ? "D:"
                        : Directory.GetCurrentDirectory()),
                    new XElement("TaskPrepDuration", 5),
                    new XElement("ClbTasksTimeout", 1),
                    new XElement("ClbTaskFramesNum", 10),
                    new XElement("DarkExps",
                        new XElement("Exp", 2),
                        new XElement("Exp", 5),
                        new XElement("Exp", 10),
                        new XElement("Exp", 15),
                        new XElement("Exp", 20),
                        new XElement("Exp", 30),
                        new XElement("Exp", 50),
                        new XElement("Exp", 80),
                        new XElement("Exp", 120),
                        new XElement("Exp", 180)
                        ),
                    new XElement("FlatExps",
                        new XElement("Exp", 2)
                        )
                    ),
                new XElement("Guide",
                    new XComment(" [PulseGuideVelocity] = arcsec/sec "),
                    new XElement("PulseGuideVelocityRa", 6.0),
                    new XElement("PulseGuideVelocityDec", 2.0),
                    new XElement("Kp", 1.2),
                    new XElement("Ki", 0.00005),
                    new XElement("Kd", 25)
                    ),
                new XElement("Comms",
                    new XElement("FocusComPort", 4),
                    new XElement("MeteoDomeTcpIpPort", 8085),
                    new XElement("DonutsTcpIpPort", 3030),
                    new XElement("SiTechExeTcpIpPort", 8079),
                    new XElement("DbPort", "5432"),
                    new XElement("DbUserId", "remote_user"),
                    new XElement("DbPassword", "remote_user"),
                    new XElement("Database", "postgres")
                    )
                );

            config.Save("Settings.xml");
            Logger.AddLogEntry("Config file reset");
        }
        #endregion
    }
}