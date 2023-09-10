using System;
using System.IO;
using System.Security.Claims;
using System.Security.Policy;
using nom.tam.fits;
using nom.tam.util;
using RPCC.Comms;
using RPCC.Focus;
using RPCC.Tasks;
using RPCC.Utils;
using static alglib;

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

        internal void SaveFitsFile(CameraControl camCtrl, int focusPos, int camNum)
        {
            short[][] convertedData = new short[data.Length][];
            for (var i = 0; i < convertedData.Length; i++)
            {
                convertedData[i] = new short[data[i].Length];
                for (var j = 0; j < convertedData[i].Length; j++)
                {
                    // Convert ushort (used in LibFli and RpccFits) to short (used in nom.tam.fits)
                    convertedData[i][j] = (short)(data[i][j] - short.MaxValue - 1);
                }
            }

            Fits newFits = new Fits();
            newFits.AddHDU(FitsFactory.HDUFactory(convertedData));
            Header newHeader = ((ImageHDU)newFits.GetHDU(0)).Header;

            // When filling in header keys with Add function, CSharpFITS tends to shuffle keys.
            // Though it can be fixed, I can't think of a simple and clean way to do so.
            // Therefore, it's a feature now.
            Cursor newCursor = newHeader.GetCursor();
            newCursor.Key = "END";

            newCursor.Add(new HeaderCard("COMMENT",
                "Conforms to FITS Version 4.0: updated 2016 July 22", false));

            // Survey keywords
            newCursor.Add(new HeaderCard("DATE-OBS",
                camCtrl.cameras[camNum].expStartDt.ToString("yyyy-MM-ddTHH:mm:ss.fff"),
                "date at the begining of exposure"));
            newCursor.Add(new HeaderCard("EXPTIME", camCtrl.task.framesExpTime / 1000.0,
                "actual integration time [sec]"));
            switch (camCtrl.task.framesType)
            {
                case "Object":
                    newCursor.Add(string.IsNullOrEmpty(camCtrl.task.objectName)
                        ? new HeaderCard("OBJNAME", "UNKNOWN", "object name")
                        : new HeaderCard("OBJNAME", camCtrl.task.objectName, "object name"));
                    break;
                default:
                    newCursor.Add(new HeaderCard("OBJNAME", camCtrl.task.framesType, "object name"));
                    break;
            }
            newCursor.Add(string.IsNullOrEmpty(camCtrl.task.objectRa)
                ? new HeaderCard("ALPHA", "UNKNOWN", "target RA [hh mm ss.s]")
                : new HeaderCard("ALPHA", camCtrl.task.objectRa, "target RA [hh mm ss.s]"));
            newCursor.Add(string.IsNullOrEmpty(camCtrl.task.objectDec)
                ? new HeaderCard("DELTA", "UNKNOWN", "target DEC [dd mm ss.s]")
                : new HeaderCard("DELTA", camCtrl.task.objectDec, "target DEC [dd mm ss.s]"));
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
            newCursor.Add(new HeaderCard("SET-TEMP", Settings.CamTemp, "CCD temperature setpoint [C]"));
            newCursor.Add(new HeaderCard("HEATSINK", camCtrl.cameras[camNum].baseTemp,
                "heatsink temperature [C]"));
            newCursor.Add(new HeaderCard("COOLPOWR", camCtrl.cameras[camNum].coolerPwr,
                "cooler power [%]"));
            switch (Settings.CamRoMode)
            {
                case "2.0MHz":
                    newCursor.Add(new HeaderCard("RATE", 2000.0, "horizontal readout rate [kPix/sec]"));
                    newCursor.Add(new HeaderCard("RDNOISE", 14.0, "datasheet readnoise [e]"));
                    break;
                case "500KHz":
                    newCursor.Add(new HeaderCard("RATE", 500.0, "horizontal readout rate [kPix/sec]"));
                    newCursor.Add(new HeaderCard("RDNOISE", 9.0, "datasheet readnoise [e]"));
                    break;
            }
            newCursor.Add(new HeaderCard("GAIN", 1.4, "typical gain [e/ADU]"));
            newCursor.Add(new HeaderCard("BINNING", Settings.CamBin, "binning factor"));
            newCursor.Add(new HeaderCard("PIXSZ", 13.5 * Settings.CamBin,
                "pixel size in microns (after binning)"));
            newCursor.Add(new HeaderCard("BZERO", short.MaxValue + 1.0,
                "offset data range to that of unsigned short"));
            newCursor.Add(new HeaderCard("BSCALE", 1.0, "default scaling factor"));
            newCursor.Add(new HeaderCard("FOCUS", focusPos, "focus position"));

            // Observatory keywords
            newCursor.Add(new HeaderCard("OBSERVAT", "KAO", "observatory name"));
            newCursor.Add(new HeaderCard("OBSERVID", 168, "observatory code"));
            newCursor.Add(new HeaderCard("LONG", "03:58:11", "observatory longitude [hh:mm:ss]"));
            newCursor.Add(new HeaderCard("LONGDEG", 59.545735, "observatory longitude [deg]"));
            newCursor.Add(new HeaderCard("LAT", "57:02:11", "observatory latitude [dd:mm:ss]"));
            newCursor.Add(new HeaderCard("LATDEG", 57.036537, "observatory latitude [deg]"));
            newCursor.Add(new HeaderCard("ALTITUDE", 290.0, "observatory altitude [meters above SL]"));

            // Weather keywords
            // TODO: Implement or discard the rest of the keywords
            // Grab Nabat by neck and review this section (and DataCollector) together
            switch (WeatherDataCollector.Sky)
            {
                case -1.0:
                    // Disconnected
                    goto case 0.0;
                case 0.0:
                    // Old data
                    newCursor.Add(new HeaderCard("SKY-TEMP", "UNKNOWN", "sky temperature from MLX-90614 sensor [deg]"));
                    break;
                default:
                    newCursor.Add(new HeaderCard("SKY-TEMP", WeatherDataCollector.Sky, "sky temperature from MLX-90614 sensor [deg]"));
                    break;
            }
            //SkyStd  SKY-STD
            switch (WeatherDataCollector.Extinction)
            {
                case -1.0:
                    // Disconnected
                    goto case 0.0;
                case 0.0:
                    // Old data
                    newCursor.Add(new HeaderCard("EXTINCT", "UNKNOWN", "relative extinction [Vmag]"));
                    break;
                default:
                    newCursor.Add(new HeaderCard("EXTINCT", WeatherDataCollector.Extinction, "relative extinction [Vmag]"));
                    break;
            }
            //ExtinctionStd
            switch (WeatherDataCollector.Seeing)
            {
                case -1.0:
                    // Disconnected
                    goto case 0.0;
                case 0.0:
                    // Old data
                    newCursor.Add(new HeaderCard("SEEING", "UNKNOWN", "seeing [arcsec]"));
                    break;
                default:
                    newCursor.Add(new HeaderCard("SEEING", WeatherDataCollector.Seeing, "seeing [arcsec]"));
                    break;
            }
            //SeeingExtinction
            switch (WeatherDataCollector.Wind)
            {
                case -1.0:
                    // Disconnected
                    goto case 100;
                case 100.0:
                    // Old data
                    newCursor.Add(new HeaderCard("WIND", "UNKNOWN", "wind speed [m/s]"));
                    break;
                default:
                    newCursor.Add(new HeaderCard("WIND", WeatherDataCollector.Wind, "wind speed [m/s]"));
                    break;
            }
            newCursor.Add(new HeaderCard("SUN-ZD", WeatherDataCollector.Sun, "Sun zenith distance"));

            // FIXME: Use MJD or something like that instead
            string outDirectory = $"{Settings.MainOutFolder}\\{DateTime.Now.AddHours(-12):yyyy-MM-dd} " +
                $"{(string.IsNullOrEmpty(camCtrl.task.objectName) ? "UNKNOWN" : camCtrl.task.objectName)}\\";
            switch (camCtrl.task.framesType)
            {
                case "Object":
                    outDirectory += $"RAW\\{(camCtrl.cameras[camNum].filter == "UNKNOWN" ? "UNKNOWN_" + camCtrl.cameras[camNum].serialNumber : camCtrl.cameras[camNum].filter)}";
                    break;
                case "Bias":
                    outDirectory += $"BIAS\\{(camCtrl.cameras[camNum].filter == "UNKNOWN" ? "UNKNOWN_" + camCtrl.cameras[camNum].serialNumber : camCtrl.cameras[camNum].filter)}";
                    break;
                case "Dark":
                    outDirectory += $"DARK\\{(camCtrl.cameras[camNum].filter == "UNKNOWN" ? "UNKNOWN_" + camCtrl.cameras[camNum].serialNumber : camCtrl.cameras[camNum].filter)}";
                    break;
                case "Flat":
                    outDirectory += $"FLAT\\{(camCtrl.cameras[camNum].filter == "UNKNOWN" ? "UNKNOWN_" + camCtrl.cameras[camNum].serialNumber : camCtrl.cameras[camNum].filter)}";
                    break;
                case "Test":
                    outDirectory += "TEST";
                    break;
                case "Focus":
                    outDirectory += "FOCUS";
                    break;
                default:
                    outDirectory += "EXTRA";
                    break;
            }
            if (!Directory.Exists(outDirectory)) Directory.CreateDirectory(outDirectory);
            
            string outName;
            switch (camCtrl.task.framesType)
            {
                case "Object":
                    outName = $"{(string.IsNullOrEmpty(camCtrl.task.objectName) ? "UNKNOWN" : camCtrl.task.objectName)}_" +
                        $"{camCtrl.cameras[camNum].filter}_{camCtrl.cameras[camNum].expStartDt:yyyy-MM-ddTHH-mm-ss}.fits";
                    break;
                case "Bias":
                    goto case "Dark";
                case "Flat":
                    goto case "Dark";
                case "Dark":
                    // If you'll need more precise output for CamTemp, make sure to set decimal separator (use Culture?)
                    outName = $"{camCtrl.task.framesType}_{camCtrl.cameras[camNum].expStartDt:yyyy-MM-ddTHH-mm-ss}_" +
                        $"B={Settings.CamBin}_F={camCtrl.cameras[camNum].filter}_E={camCtrl.task.framesExpTime}_T={Settings.CamTemp:F0}.fits";
                    break;
                default:
                    outName = $"{camCtrl.task.framesType}_{camCtrl.cameras[camNum].filter}_{camCtrl.cameras[camNum].expStartDt:yyyy-MM-ddTHH-mm-ss}.fits";
                    break;
            }

            BufferedDataStream outStream = new BufferedDataStream(new FileStream($"{outDirectory}\\{outName}", FileMode.Create));
            newFits.Write(outStream);
            // FIXME: Suddenly caught a bug in next line - said something like "can't access closed file".
            // Check it and, if needed, remove the next two lines.
            outStream.Flush();
            outStream.Close();
        }
    }
}