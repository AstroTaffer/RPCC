using System;
using System.Collections.Generic;
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
        ///     Работа с файлом конфигурации
        /// </summary>
        /// 

        static Settings()
        {
            cameraSettings = new Dictionary<string, CameraSettingsCollector>();
        }

        #region Cameras
        /// <summary>
        ///     Настройки камер
        /// </summary>
        
        private static Dictionary<string, CameraSettingsCollector> cameraSettings;

        internal static CameraSettingsCollector GetCameraSettingsSet(string id)
        {
            if (cameraSettings.ContainsKey(id))
                return cameraSettings[id];
            else
            {
                Logger.AddLogEntry($"WARNING Unable to find settings set for camera: {id}");
                return cameraSettings["DEFAULT"];
            }
        }
        #endregion

        #region Survey
        /// <summary>
        ///     Настройки съемки
        /// </summary>

        private static string _mainOutputFolder;
        public static string MainOutputFolder
        {
            get => _mainOutputFolder;
            set
            {
                if (Directory.Exists(value)) _mainOutputFolder = value;
                else throw new ArgumentException($"Folder does not exist: {value}");
            }
        }

        private static int _taskPrepDuration;
        public static int TaskPrepDuration /// [min]
        {
            get => _taskPrepDuration;
            set
            {
                if (value > 0) _taskPrepDuration = value;
                else throw new ArgumentException($"Invalid task preparation duration: {value}");
            }
        }

        private static int _clbTasksTimeout;
        public static int ClbTasksTimeout /// [d]
        {
            get => _clbTasksTimeout;
            set
            {
                if (value > 0) _clbTasksTimeout = value;
                else throw new ArgumentException($"Invalid calibration tasks timeout: {value}");
            }
        }

        private static int _clbTasksFramesNum;
        public static int ClbTasksFramesNum
        {
            get => _clbTasksFramesNum;
            set
            {
                if (value > 0) _clbTasksFramesNum = value;
                else throw new ArgumentException($"Invalid calibration tasks frames number: {value}");
            }
        }

        public static int[] DarkExps { get; set; } /// [sec]

        public static int[] FlatExps { get; set; } /// [sec]
        #endregion

        #region Guide
        /// <summary>
        ///     Настройки коррекции гидирования
        /// </summary>

        private static double _pulseGuideVelocityRa;
        public static double PulseGuideVelocityRa /// [arcsec/sec]
        {
            get => _pulseGuideVelocityRa;
            set
            {
                if (value >= 0.0) _pulseGuideVelocityRa = value;
                else throw new ArgumentException($"Invalid PulseGuideVelocityRa: {value}");
            }
        }

        private static double _pulseGuideVelocityDec;
        public static double PulseGuideVelocityDec /// [arcsec/sec]
        {
            get => _pulseGuideVelocityDec;
            set
            {
                if (value >= 0.0) _pulseGuideVelocityDec = value;
                else throw new ArgumentException($"Invalid PulseGuideVelocityDec: {value}");
            }
        }

        public static double Kp { get; set; }
        
        public static double Ki { get; set; }
        
        public static double Kd { get; set; }
        #endregion

        #region Comms
        /// <summary>
        ///     Настройки связи с другими приложениями программного комплекса
        /// </summary>

        private static int _focusComPort;
        public static int FocusComPort
        {
            get => _focusComPort;
            set
            {
                if (value >= 1 && value <= 256) _focusComPort = value;
                else throw new ArgumentException($"Invalid focus COM port ID: {value}");
            }
        }

        private static int _meteoDomeTcpIpPort;
        public static int MeteoDomeTcpIpPort
        {
            get => _meteoDomeTcpIpPort;
            set
            {
                if (value >= 0 && value <= 65535) _meteoDomeTcpIpPort = value;
                else throw new ArgumentException($"Invalid MeteoDome TCP/IP port: {value}");
            }
        }

        private static int _siTechExeTcpIpPort;
        public static int SiTechExeTcpIpPort
        {
            get => _siTechExeTcpIpPort;
            set
            {
                if (value >= 0 && value <= 65535) _siTechExeTcpIpPort = value;
                else throw new ArgumentException($"Invalid SiTechExe TCP/IP port: {value}");
            }
        }

        private static string _dbPort;
        public static string DbPort
        {
            get => _dbPort;
            set
            {
                int parsedValue = int.Parse(value);

                if (parsedValue >= 0 && parsedValue <= 65535) _dbPort = value;
                else throw new ArgumentException($"Invalid DB TCP/IP port: {value}");
            }
        }

        private static string _dbUserId;
        public static string DbUserId {
            get => _dbUserId;
            set
            {
                if (!string.IsNullOrEmpty(value)) _dbUserId = value;
                else throw new ArgumentException("DB user ID is null or empty");
            }
        }

        private static string _dbPassword;
        public static string DbPassword
        {
            get => _dbPassword;
            set
            {
                if (!string.IsNullOrEmpty(value)) _dbPassword = value;
                else throw new ArgumentException("DB password is null or empty");
            }
        }

        private static string _database;
        public static string Database
        {
            get => _database;
            set
            {
                if (!string.IsNullOrEmpty(value)) _database = value;
                else throw new ArgumentException("DB name is null or empty");
            }
        }
        #endregion

        #region XmlIO
        internal static void LoadXmlConfig()
        {
            bool isConfigBad = false;
            DialogResult isResetRequested = DialogResult.No;
            cameraSettings.Clear();

            try
            {
                var config = XDocument.Load("Settings.xml");
                XElement baseElem;

                // Read <Cameras> settings
                bool hasDefaultCamSettings = false;
                baseElem = config.Root.Element("Cameras");
                cameraSettings = baseElem.Elements("Camera").ToDictionary(
                    set =>
                    {
                        string sn = (string)set.Attribute("SerialNumber");
                        if (string.IsNullOrEmpty(sn))
                            throw new ArgumentException($"Invalid camera SerialNumber: {sn}");
                        if (sn == "DEFAULT") hasDefaultCamSettings = true;
                        return sn;
                    },
                    set => new CameraSettingsCollector(set)
                    );

                if (!hasDefaultCamSettings)
                    throw new ArgumentNullException("No DEFAULT camera settings present");

                // Read <Survey> settings
                baseElem = config.Root.Element("Survey");
                MainOutputFolder = (string)baseElem.Element("MainOutputFolder");
                TaskPrepDuration = (int)baseElem.Element("TaskPrepDuration");
                ClbTasksTimeout = (int)baseElem.Element("ClbTasksTimeout");
                ClbTasksFramesNum = (int)baseElem.Element("ClbTasksFramesNum");
                DarkExps = baseElem.Element("DarkExps").Elements("Exp").Select(e => (int)e).ToArray();
                FlatExps = baseElem.Element("FlatExps").Elements("Exp").Select(e => (int)e).ToArray();

                // Read <Guide> settings
                baseElem = config.Root.Element("Guide");
                PulseGuideVelocityRa = (double)baseElem.Element("PulseGuideVelocityRa");
                PulseGuideVelocityDec = (double)baseElem.Element("PulseGuideVelocityDec");
                Kp = (double)baseElem.Element("Kp");
                Ki = (double)baseElem.Element("Ki");
                Kd = (double)baseElem.Element("Kd");

                // Read <Comms> settings
                baseElem = config.Root.Element("Comms");
                FocusComPort = (int)baseElem.Element("FocusComPort");
                MeteoDomeTcpIpPort = (int)baseElem.Element("MeteoDomeTcpIpPort");
                SiTechExeTcpIpPort = (int)baseElem.Element("SiTechExeTcpIpPort");
                DbPort = (string)baseElem.Element("DbPort");
                DbUserId = (string)baseElem.Element("DbUserId");
                DbPassword = (string)baseElem.Element("DbPassword");
                Database = (string)baseElem.Element("Database");

            }
            catch (FileNotFoundException)
            {
                isConfigBad = true;
                isResetRequested = MessageBox.Show("Config file not found.\n"
                    + "Would you like to recreate it (\"YES\")\n"
                    + "or just close the application (\"NO\")?",
                    "Config not found",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Error,
                    MessageBoxDefaultButton.Button1);
                Logger.AddLogEntry($"WARNING Config file not found");
            }
            catch (Exception ex) when (ex is System.Xml.XmlException
                                    || ex is NullReferenceException)
            {
                isConfigBad = true;
                isResetRequested = MessageBox.Show("Config file has invalid structure.\n"
                    + "Would you like to reset it (\"YES\")\n"
                    + "or just close the application (\"NO\")?",
                    "Invalid config structure",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Error,
                    MessageBoxDefaultButton.Button1);
                Logger.AddLogEntry($"WARNING Config file has invalid structure");
            }
            catch (Exception ex) when (ex is ArgumentNullException
                                    || ex is FormatException
                                    || ex is ArgumentException)
            {
                isConfigBad = true;
                isResetRequested = MessageBox.Show("Config file has invalid parameters:\n"
                    + $"{ex.Message}.\n"
                    + "Would you like to reset it (\"YES\")\n"
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
                        default:
                            Environment.Exit(1);
                            break;
                    }
                }
                else
                    Logger.AddLogEntry("Config file loaded successfully");
            }
        }

        internal static void ResetXmlConfig()
        {
            var config = new XElement("Settings",
                new XElement("Cameras",
                    new XComment(" [Gain] = e/ADU, [Rate] = kPix/sec, [RdNoise] = e "),
                    new XElement("Camera",
                        new XAttribute("SerialNumber", "DEFAULT"),
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
                    new XElement("MainOutputFolder", Directory.GetCurrentDirectory()),
                    new XElement("TaskPrepDuration", 5),
                    new XElement("ClbTasksTimeout", 1),
                    new XElement("ClbTasksFramesNum", 10),
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

    internal class CameraSettingsCollector
    {
        /// <summary>
        ///     Набор настроек для экземпляра ICameraDevice
        /// </summary>
        
        internal CameraSettingsCollector(XElement xmlCameraElement)
        {
            Filter = (string)xmlCameraElement.Element("Filter");
            TempSetpoint = (double)xmlCameraElement.Element("TempSetpoint");
            Bin = (int)xmlCameraElement.Element("Bin");
            ExpStartFlushesNum = (int)xmlCameraElement.Element("ExpStartFlushesNum");
            Model = (string)xmlCameraElement.Element("Model");
            Detector = (string)xmlCameraElement.Element("Detector");
            Gain = (double)xmlCameraElement.Element("Gain");
            Rate = (double)xmlCameraElement.Element("Rate");
            RdNoise = (double)xmlCameraElement.Element("RdNoise");
        }

        private string _filter;
        public string Filter
        {
            get => _filter;
            set
            {
                if (!string.IsNullOrEmpty(value)) _filter = value;
                else throw new ArgumentException("Camera filter is null or empty");
            }
        }

        public double TempSetpoint {  get; set; }

        private int _bin;
        public int Bin
        {
            get => _bin;
            set
            {
                if (value > 0) _bin = value;
                else throw new ArgumentException($"Invalid camera bin: {value}");
            }
        }

        private int _expStartFlushesNum;
        public int ExpStartFlushesNum
        {
            get => _expStartFlushesNum;
            set
            {
                if (value >= 1) _expStartFlushesNum = value;
                else throw new ArgumentException($"Invalid ExpStartFlushesNum: {value}");
            }
        }

        private string _model;
        public string Model
        {
            get => _model;
            set
            {
                if (!string.IsNullOrEmpty(value)) _model = value;
                else throw new ArgumentException("Camera model is null or empty");
            }
        }

        private string _detector;
        public string Detector
        {
            get => _detector;
            set
            {
                if (!string.IsNullOrEmpty(value)) _detector = value;
                else throw new ArgumentException("Camera detector is null or empty");
            }
        }

        private double _gain;
        public double Gain /// [e/ADU]
        {
            get => _gain;
            set
            {
                if (value > 0.0) _gain = value;
                else throw new ArgumentException($"Invalid camera gain: {value}");
            }
        }

        private double _rate;
        public double Rate /// [kPix/sec]
        {
            get => _rate;
            set
            {
                if (value > 0.0) _rate = value;
                else throw new ArgumentException($"Invalid camera rate: {value}");
            }
        }

        private double _rdNoise;
        public double RdNoise /// [e]
        {
            get => _rdNoise;
            set
            {
                if (value > 0.0) _rdNoise = value;
                else throw new ArgumentException($"Invalid camera readout noise: {value}");
            }
        }
    }
}