using System;
using System.Xml.Linq;

namespace RPCC
{
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

        private double lowerBrightnessSd;
        private double upperBrightnessSd;
        private int apertureRadius;
        private int annulusInnerRadius;
        private int annulusOuterRadius;

        public double LowerBrightnessSd
        {
            get { return lowerBrightnessSd; }
            set
            {
                if (value > 0.0)
                {
                    lowerBrightnessSd = value;
                }
                else
                {
                    throw new ArgumentException("Lower brightness SD must be greater than 0.0");
                }
            }
        }
        public double UpperBrightnessSd
        {
            get { return upperBrightnessSd; }
            set
            {
                if (value > 0.0)
                {
                    upperBrightnessSd = value;
                }
                else
                {
                    throw new ArgumentException("Upper brightness SD must be greater than 0.0");
                }
            }
        }
        public int ApertureRadius
        {
            get { return apertureRadius; }
            set
            {
                if (value > 0)
                {
                    apertureRadius = value;
                }
                else
                {
                    throw new ArgumentException("Aperture radius must be greater than 0");
                }
            }
        }
        public int AnnulusInnerRadius
        {
            get { return annulusInnerRadius; }
            set
            {
                // HACK: Stricter condition may be used - if (value > ApertureRadius)
                if (value > 0)
                {
                    annulusInnerRadius = value;
                }
                else
                {
                    throw new ArgumentException("Annulus inner radius must be greater than 0");
                }
            }
        }
        public int AnnulusOuterRadius
        {
            get { return annulusOuterRadius; }
            set
            {
                if (value > annulusInnerRadius)
                {
                    annulusOuterRadius = value;
                }
                else
                {
                    throw new ArgumentException("Annulus outer radius must be greater than its inner radius ");
                }
            }
        }
        #endregion

        #region ConfigIO
        internal void LoadXmlConfig(string fileName)
        {
            // FIXME: Handle error when attempting to access unexisting node
            // Apparently, using LINQ to XML was a mistake. JSON or simple hand-made .txt parser would be more convenient
            XDocument config = XDocument.Load(fileName);

            LowerBrightnessSd = (double)config.Root.Element("analysis_and_plotting").Element("lowerBrightnessSd");
            UpperBrightnessSd = (double)config.Root.Element("analysis_and_plotting").Element("upperBrightnessSd");
            ApertureRadius = (int)config.Root.Element("analysis_and_plotting").Element("apertureRadius");
            AnnulusInnerRadius = (int)config.Root.Element("analysis_and_plotting").Element("annulusInnerRadius");
            AnnulusOuterRadius = (int)config.Root.Element("analysis_and_plotting").Element("annulusOuterRadius");
        }

        internal void SaveXmlConfig(string fileName)
        {
            XDocument config = new XDocument(new XElement("settings",
                new XElement("analysis_and_plotting",
                    new XElement("lowerBrightnessSd", LowerBrightnessSd),
                    new XElement("upperBrightnessSd", UpperBrightnessSd),
                    new XElement("apertureRadius", ApertureRadius),
                    new XElement("annulusInnerRadius", AnnulusInnerRadius),
                    new XElement("annulusOuterRadius", AnnulusOuterRadius)
                    )
                ));

            config.Save(fileName);
        }

        internal void RestoreDefaultXmlConfig()
        {
            XDocument config = new XDocument(new XElement("settings",
                new XElement("analysis_and_plotting",
                    new XElement("lowerBrightnessSd", 1.0),
                    new XElement("upperBrightnessSd", 2.0),
                    new XElement("apertureRadius", 6),
                    new XElement("annulusInnerRadius", 10),
                    new XElement("annulusOuterRadius", 15)
                    )
                ));

            config.Save("SettingsDefault.xml");
        }
        #endregion
    }
}