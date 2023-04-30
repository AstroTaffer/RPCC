using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Linq;

namespace RPCC.Utils
{
    // TODO: Instead of throwing exceptions it may be better to display error provider message box
    internal class Settings
    {
        /// <summary>
        ///     Настройки приложения.
        ///     Чтение и запись конфигурационных файлов.
        /// </summary>

        #region Analysis and Plotting

        /// <summary>
        ///     Настройки анализа и построения изображения.
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
        ///     Настройки камер.
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
                _snCamG = value;
            }
        }

        public string SnCamR
        {
            get => _snCamR;
            set
            {
                _snCamR = value;
            }
        }

        public string SnCamI
        {
            get => _snCamI;
            set
            {
                _snCamI = value;
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
        ///     Настройки съёмки.
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

        #region ConfigIO

        internal void LoadXmlConfig(string fileName)
        {
            // FIXME: Handle error when attempting to access unexisting node
            // Apparently, using LINQ to XML was a mistake. JSON or simple hand-made .txt parser would be more convenient
            var config = XDocument.Load(fileName);

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
            CamRoMode = (string)config.Root.Element("cameras").Element("сamRoMode");

            OutImgsFolder = (string)config.Root.Element("survey").Element("outImgsFolder");
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
                    new XElement("camRoMode", CamRoMode)),
                
                new XElement("survey",
                    new XElement("outImgsFolder", OutImgsFolder))
            ));

            config.Save(fileName);
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
                    new XElement("outImgsFolder", Directory.GetCurrentDirectory()))
            ));

            config.Save("SettingsDefault.xml");
        }

        #endregion
    }
}