using System;
using System.IO;
using nom.tam.fits;
using nom.tam.util;
using RPCC.Utils;

namespace RPCC.Cams
{
    internal class RpccFits
    {
        internal ushort[][] data;

        internal Header header;
        // TODO: When reading and saving image from camera RpccFits.header will always be empty
        // Because right now there is no use in creating one

        // TODO: To completely remove the stutter, think of a way to async reading a fits file
        // Create RpccFits by reading an existing FITS file
        internal RpccFits(string fitsFileName)
        {
            var fitsFile = new Fits(fitsFileName);
            var fitsHdu = (ImageHDU) fitsFile.ReadHDU();
            header = fitsHdu.Header;
            var fitsDataRaw = (Array[]) fitsHdu.Kernel;
            fitsFile.Close();

            var dataHeight = fitsDataRaw.Length;
            var dataWidth = fitsDataRaw[0].Length;
            data = new ushort[dataHeight][];
            for (var i = 0; i < dataHeight; i++)
            {
                data[i] = new ushort[dataWidth];
                for (var j = 0; j < dataWidth; j++)
                {
                    // Convert short (used in nom.tam.fits) to ushort (used in LibFli and RpccFits)
                    var buff = short.MaxValue + (short) fitsDataRaw[i].GetValue(j) + 1;
                    data[i][j] = (ushort) buff;
                }
            }
        }

        // Create new empty RpccFits
        internal RpccFits()
        {

        }

        internal void SaveFitsFile(Settings settings, CameraControl camCtrl, int camNum)
        {
            short[][] convertedData = new short[data.Length][];
            for (var i = 0; i < convertedData.Length; i++)
            {
                convertedData[i] = new short[data[i].Length];
                for (var j = 0;j < convertedData[i].Length; j++)
                {
                    // Convert ushort (used in LibFli and RpccFits) to short (used in nom.tam.fits)
                    convertedData[i][j] = (short)(data[i][j] - short.MaxValue - 1);
                }
            }

            Fits newFits = new Fits();
            newFits.AddHDU(FitsFactory.HDUFactory(convertedData));
            Header newHeader = ((ImageHDU)newFits.GetHDU(0)).Header;

            Cursor newCursor = newHeader.GetCursor();
            newCursor.Key = "END";

            newCursor.Add(new HeaderCard("COMMENT",
                "FITS(Flexible Image Transport System) format defined in Astronomy and", false));
            newCursor.Add(new HeaderCard("COMMENT",
                "Astrophysics Supplement Series v44 / p363, v44 / p371, v73 / p359, v73 / p365.", false));

            // Survey keywords
            newCursor.Add(new HeaderCard("DATE-OBS",
                camCtrl.cameras[camNum].expStartDt.ToString("yyyy-MM-ddTHH:mm:ss.fff"),
                "date at the begining of exposure"));
            newCursor.Add(new HeaderCard("EXPTIME", camCtrl.task.framesExpTime / 1000.0,
                "actual integration time [sec]"));
            switch (camCtrl.task.framesType)
            {
                case "Object":
                    // TODO: Add proper object name
                    // OBJECT = '' / object name
                    newCursor.Add(new HeaderCard("OBJECT", "Object", "object name"));
                    break;
                default:
                    newCursor.Add(new HeaderCard("OBJECT", camCtrl.task.framesType, "object name"));
                    break;
            }
            newCursor.Add(new HeaderCard("IMAGETYP", camCtrl.task.framesType,
                "Object, Flat, Dark, Bias, Test, Focus"));
            // RA      = '21:51:12.055'       / target RA
            // DEC     = '+28:51:38.72'       / target DEC

            // Instrument keywords
            newCursor.Add(new HeaderCard("ORIGIN", "URFU", "organization responsible for the data"));
            newCursor.Add(new HeaderCard("TELESCOP", "APM-RoboPhot", "telescope"));
            newCursor.Add(new HeaderCard("INSTRUME", "3CHP", "instrument"));
            newCursor.Add(new HeaderCard("CAMERA", "FLI ML4240 MB", "camera name"));
            newCursor.Add(new HeaderCard("DETECTOR", "E2V CCD42-40-1-368 MB", "CCD Detector"));
            newCursor.Add(new HeaderCard("SERNUM", camCtrl.cameras[camNum].serialNumber,
                "serial number"));
            newCursor.Add(new HeaderCard("FILTER", camCtrl.cameras[camNum].filter, "SDSS filter"));
            newCursor.Add(new HeaderCard("CCD-TEMP", camCtrl.cameras[camNum].ccdTemp,
                "CCD temperature [C]"));
            newCursor.Add(new HeaderCard("SET-TEMP", settings.CamTemp, "CCD temperature setpoint [C]"));
            newCursor.Add(new HeaderCard("HEATSINK", camCtrl.cameras[camNum].baseTemp,
                "heatsink temperature [C]"));
            newCursor.Add(new HeaderCard("COOLPOWR", camCtrl.cameras[camNum].coolerPwr,
                "cooler power [%]"));
            switch (settings.CamRoModeIndex) // TODO: Implement properly after testing camera readout modes
            {
                case 0: // 2.0MHz
                    newCursor.Add(new HeaderCard("RATE", 2000.0, "horizontal readout rate [kPix/sec]"));
                    newCursor.Add(new HeaderCard("RDNOISE", 14.0, "datasheet readnoise [e]"));
                    break;
                case 1: // 500KHz
                    newCursor.Add(new HeaderCard("RATE", 500.0, "horizontal readout rate [kPix/sec]"));
                    newCursor.Add(new HeaderCard("RDNOISE", 9.0, "datasheet readnoise [e]"));
                    break;
            }
            newCursor.Add(new HeaderCard("GAIN", 1.4, "typical gain [e/ADU]"));
            newCursor.Add(new HeaderCard("BINNING", settings.CamBin, "binning factor"));
            newCursor.Add(new HeaderCard("PIXSZ", 13.5 * settings.CamBin,
                "pixel size in microns (after binning)"));
            newCursor.Add(new HeaderCard("BZERO", short.MaxValue + 1.0,
                "offset data range to that of unsigned short"));
            newCursor.Add(new HeaderCard("BSCALE", 1.0, "default scaling factor"));

            // Observatory keywords
            newCursor.Add(new HeaderCard("OBSERVAT", "KAO", "observatory name"));
            newCursor.Add(new HeaderCard("OBSERVID", 168, "observatory code"));
            newCursor.Add(new HeaderCard("LONG", "03:58:11", "observatory longitude [hh:mm:ss]"));
            newCursor.Add(new HeaderCard("LONGDEG", 59.545735, "observatory longitude [deg]"));
            newCursor.Add(new HeaderCard("LAT", "57:02:11", "observatory latitude [dd:mm:ss]"));
            newCursor.Add(new HeaderCard("LATDEG", 57.036537, "observatory latitude [deg]"));
            newCursor.Add(new HeaderCard("ALTITUDE", 290.0, "observatory altitude [meters above SL]"));

            string outDirectory = $"{settings.OutImgsFolder}\\{camCtrl.cameras[camNum].filter}";
            if (!Directory.Exists(outDirectory)) Directory.CreateDirectory(outDirectory);
            string outName;
            switch (camCtrl.task.framesType)
            {
                case "Object":
                    // TODO: Add proper object name
                    outName = $"Object-{camCtrl.cameras[camNum].expStartDt:yyyy-MM-ddThh-mm-ss}.fits";
                    break;
                default:
                    outName = $"{camCtrl.task.framesType}-" +
                        $"{camCtrl.cameras[camNum].expStartDt:yyyy-MM-ddThh-mm-ss}.fits";
                    break;
            }

            BufferedDataStream outStream = new BufferedDataStream(new FileStream($"{outDirectory}\\" +
                $"{outName}", FileMode.Create));
            newFits.Write(outStream);
            outStream.Flush();
            outStream.Close();
        }
    }
}