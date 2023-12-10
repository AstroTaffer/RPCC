using System;
using System.IO;
using ASCOM.Tools;
using nom.tam.fits;
using nom.tam.util;
using RPCC.Comms;
using RPCC.Focus;
using RPCC.Tasks;
using RPCC.Utils;

namespace RPCC.Cams
{
    internal class RpccFits
    {
        internal ushort[][] data;
        internal Header header;

        // Create new empty RpccFits
        internal RpccFits()
        {
            // When reading and saving captured images RpccFits.header will always be empty
            // Because in all implementations that I can think of we'll end up generating
            // false or useless information. All thanks to ushort[][] and short[][] difference.
        }

        // Create RpccFits by reading an existing FITS file
        internal RpccFits(string fitsFileName)
        {
            var fitsFile = new Fits(fitsFileName);
            var fitsHdu = (ImageHDU)fitsFile.ReadHDU();
            header = fitsHdu.Header;
            var fitsDataRaw = (Array[])fitsHdu.Kernel;
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
                    var buff = short.MaxValue + (short)fitsDataRaw[i].GetValue(j) + 1;
                    data[i][j] = (ushort)buff;
                }
            }
        }

        internal string SaveFitsFile(CameraDevice cam)
        {
            short[][] convertedData = new short[data.Length][];
            for (var i = 0; i < convertedData.Length; i++)
            {
                convertedData[i] = new short[data[i].Length];
                for (var j = 0; j < convertedData[i].Length; j++)
                    // Convert ushort (used in LibFli and RpccFits) to short (used in nom.tam.fits)
                    convertedData[i][j] = (short)(data[i][j] - short.MaxValue - 1);
            }

            var newFits = new Fits();
            newFits.AddHDU(FitsFactory.HDUFactory(convertedData));
            var newHeader = ((ImageHDU)newFits.GetHDU(0)).Header;
            
            FillInHeader(cam, newHeader);

            DateTime outDateTime = DateTime.Now.AddHours(-12);
            string outDir = $"{Settings.MainOutFolder}\\{outDateTime.Year}\\{outDateTime:yyyy-MM-dd}\\" +
                $"{(string.IsNullOrEmpty(CameraControl.loadedTask.Object) ? "UNKNOWN" : CameraControl.loadedTask.Object)}\\";
            switch (CameraControl.loadedTask.FrameType)
            {
                case "Object":
                    outDir += $"RAW\\{(cam.filter == "UNKNOWN" ? "UNKNOWN_" + cam.serialNumber : cam.filter)}";
                    break;
                case "Bias":
                    outDir += $"BIAS\\{(cam.filter == "UNKNOWN" ? "UNKNOWN_" + cam.serialNumber : cam.filter)}";
                    break;
                case "Dark":
                    outDir += $"DARK\\{(cam.filter == "UNKNOWN" ? "UNKNOWN_" + cam.serialNumber : cam.filter)}";
                    break;
                case "Flat":
                    outDir += $"FLAT\\{(cam.filter == "UNKNOWN" ? "UNKNOWN_" + cam.serialNumber : cam.filter)}";
                    break;
                case "Test":
                    outDir += "TEST";
                    break;
                case "Focus":
                    outDir += "FOCUS";
                    break;
                default:
                    outDir += "EXTRA";
                    break;
            }
            if (!Directory.Exists(outDir)) Directory.CreateDirectory(outDir);

            string outName;
            switch (CameraControl.loadedTask.FrameType)
            {
                case "Object":
                    outName = $"{(string.IsNullOrEmpty(CameraControl.loadedTask.Object) ? "UNKNOWN" : CameraControl.loadedTask.Object)}_" +
                        $"{cam.filter}_{cam.expStartDt:yyyy-MM-ddTHH-mm-ss}.fits";
                    break;
                case "Bias":
                    goto case "Dark";
                case "Flat":
                    goto case "Dark";
                case "Dark":
                    // If you'll need more precise output for CamTemp, make sure to set decimal separator (use Culture?)
                    outName = $"{CameraControl.loadedTask.FrameType}_{cam.expStartDt:yyyy-MM-ddTHH-mm-ss}_" +
                        $"XB={CameraControl.loadedTask.Xbin}_YB={CameraControl.loadedTask.Ybin}" +
                        $"_F={cam.filter}_E={CameraControl.loadedTask.Exp}_T={Settings.CamTemp:F0}.fits";
                    break;
                default:
                    outName = $"{CameraControl.loadedTask.FrameType}_{cam.filter}_" +
                        $"{cam.expStartDt:yyyy-MM-ddTHH-mm-ss}.fits";
                    break;
            }

            string outFilePath = $"{outDir}\\{outName}";
            BufferedDataStream outStream = new BufferedDataStream(new FileStream(outFilePath,
                FileMode.Create));
            newFits.Write(outStream);
            // I once caught a bug in next line - said something like "can't access closed file".
            // If encountered again, remove the next two lines.
            outStream.Flush();
            outStream.Close();

            DbCommunicate.AddFrameToDb(CameraControl.loadedTask, outFilePath, 
                MountDataCollector.RightAsc, MountDataCollector.Declination,
                cam.filter, cam.expStartDt, WeatherDataCollector.Extinction, cam.ccdTemp, cam.serialNumber);
            return outFilePath;
        }

        internal void FillInHeader(CameraDevice cam, Header head)
        {
            // When filling in header keys with Add function, CSharpFITS tends to shuffle keys.
            // Though it can be fixed, I can't think of a simple and clean way to do so.
            // Therefore, it's a feature now.

            var cursor = head.GetCursor();
            cursor.Key = "END";

            cursor.Add(new HeaderCard("COMMENT",
                "Conforms to FITS Version 4.0: updated 2016 July 22", false));

            #region Observation keywords
            cursor.Add(new HeaderCard("DATE-OBS",
                cam.expStartDt.ToString("yyyy-MM-ddTHH:mm:ss.fff"),
                "date-time at the beginning of exposure"));
            cursor.Add(new HeaderCard("JD-OBS", cam.expStartJd, "JD at the beginning of exposure"));
            cursor.Add(new HeaderCard("COUNT",
                CameraControl.loadedTask.DoneFrames + 1, "frame number in sequence"));
            cursor.Add(new HeaderCard("MAXCNT",
                CameraControl.loadedTask.AllFrames, "length of sequence"));
            cursor.Add(new HeaderCard("EXPTIME",
                CameraControl.loadedTask.Exp, "actual integration time [sec]"));
            cursor.Add(new HeaderCard("IMAGETYP",
                CameraControl.loadedTask.FrameType, "Object, Flat, Dark, Bias, Focus, Test"));
            cursor.Add(new HeaderCard("XBINNING",
                CameraControl.loadedTask.Xbin, "binning factor in width"));
            cursor.Add(new HeaderCard("XPIXSZ",
                13.5 * CameraControl.loadedTask.Xbin, "pixel width (after binning) [micron]"));
            cursor.Add(new HeaderCard("YBINNING",
                CameraControl.loadedTask.Ybin, "binning factor in height"));
            cursor.Add(new HeaderCard("YPIXSZ",
                13.5 * CameraControl.loadedTask.Ybin, "pixel height (after binning) [micron]"));
            switch (CameraControl.loadedTask.FrameType)
            {
                case "Object":
                    cursor.Add(string.IsNullOrEmpty(CameraControl.loadedTask.Object)
                        ? new HeaderCard("OBJNAME", "UNKNOWN", "object name")
                        : new HeaderCard("OBJNAME", CameraControl.loadedTask.Object, "object name"));
                    goto case "Flat";
                case "Flat":
                    cursor.Add(new HeaderCard("ALPHA", MountDataCollector.RightAsc, "scope RA [h]"));
                    cursor.Add(new HeaderCard("DELTA", MountDataCollector.Declination, "scope DEC [deg]"));
                    cursor.Add(new HeaderCard("EQUINOX", 2000.0, "equinox of ALPHA and DELTA [yr]"));
                    break;
            }
            cursor.Add(string.IsNullOrEmpty(CameraControl.loadedTask.Observer)
                        ? new HeaderCard("OBSERVER", "UNKNOWN", "object name")
                        : new HeaderCard("OBSERVER", CameraControl.loadedTask.Observer, "observer"));
            #endregion

            #region Instrument keywords
            cursor.Add(new HeaderCard("ORIGIN", "URFU", "organization responsible for the data"));
            cursor.Add(new HeaderCard("TELESCOP", "APM-RoboPhot", "telescope"));
            cursor.Add(new HeaderCard("INSTRUME", "3CHP", "instrument"));
            cursor.Add(new HeaderCard("CAMERA", "FLI ML4240 MB", "camera name"));
            cursor.Add(new HeaderCard("DETECTOR", "E2V CCD42-40-1-368 MB", "CCD Detector"));
            cursor.Add(new HeaderCard("SERNUM", cam.serialNumber,
                "serial number"));
            cursor.Add(new HeaderCard("FILTER", cam.filter, "SDSS filter"));
            cursor.Add(new HeaderCard("CCD-TEMP", cam.ccdTemp,
                "CCD temperature [C]"));
            cursor.Add(new HeaderCard("SET-TEMP", Settings.CamTemp, "CCD temperature setpoint [C]"));
            cursor.Add(new HeaderCard("HEATSINK", cam.baseTemp,
                "heatsink temperature [C]"));
            cursor.Add(new HeaderCard("COOLPOWR", cam.coolerPwr,
                "cooler power [%]"));
            switch (CameraControl.loadedTask.FrameType)
            {
                case "Focus":
                    cursor.Add(new HeaderCard("RATE", 2000.0, "horizontal readout rate [kPix/sec]"));
                    cursor.Add(new HeaderCard("RDNOISE", 14.0, "datasheet readnoise [e]"));
                    break;
                default:
                    cursor.Add(new HeaderCard("RATE", 500.0, "horizontal readout rate [kPix/sec]"));
                    cursor.Add(new HeaderCard("RDNOISE", 9.0, "datasheet readnoise [e]"));
                    break;
            }
            cursor.Add(new HeaderCard("GAIN", 1.4, "typical gain [e/ADU]"));
            cursor.Add(new HeaderCard("BZERO", short.MaxValue + 1.0,
                "offset data range to that of unsigned short"));
            cursor.Add(new HeaderCard("BSCALE", 1.0, "default scaling factor"));
            cursor.Add(new HeaderCard("FOCUS", SerialFocus.CurrentPosition, "focus position"));
            #endregion

            #region Observatory keywords
            cursor.Add(new HeaderCard("OBSERVAT", "KAO", "observatory name"));
            cursor.Add(new HeaderCard("OBSERVID", 168, "observatory code"));
            cursor.Add(new HeaderCard("LONG", "03:58:11", "observatory longitude [hh:mm:ss]"));
            cursor.Add(new HeaderCard("LONGDEG", 59.545735, "observatory longitude [deg]"));
            cursor.Add(new HeaderCard("LAT", "57:02:11", "observatory latitude [dd:mm:ss]"));
            cursor.Add(new HeaderCard("LATDEG", 57.036537, "observatory latitude [deg]"));
            cursor.Add(new HeaderCard("ALTITUDE", 290.0, "observatory altitude [meters above SL]"));
            #endregion

            #region Weather keywords
            cursor.Add(new HeaderCard("AIRMASS",
                MountDataCollector.Airmass, "airmass at the end of exposure"));
            switch (WeatherDataCollector.Sky)
            {
                case -1.0:
                    // Disconnected
                    goto case 0.0;
                case 0.0:
                    // Old data
                    cursor.Add(new HeaderCard("SKY-TEMP", "UNKNOWN",
                        "sky temperature from MLX-90614 sensor [C]"));
                    break;
                default:
                    cursor.Add(new HeaderCard("SKY-TEMP", WeatherDataCollector.Sky,
                        "sky temperature from MLX-90614 sensor [C]"));
                    break;
            }
            switch (WeatherDataCollector.Extinction)
            {
                case -1.0:
                    // Disconnected
                    goto case 0.0;
                case 0.0:
                    // Old data
                    cursor.Add(new HeaderCard("EXTINCT", "UNKNOWN", "relative extinction [Vmag]"));
                    break;
                default:
                    cursor.Add(new HeaderCard("EXTINCT",
                        WeatherDataCollector.Extinction, "relative extinction [Vmag]"));
                    break;
            }
            switch (WeatherDataCollector.Seeing)
            {
                case -1.0:
                    // Disconnected
                    goto case 0.0;
                case 0.0:
                    // Old data
                    cursor.Add(new HeaderCard("SEEING", "UNKNOWN", "seeing [arcsec]"));
                    break;
                default:
                    cursor.Add(new HeaderCard("SEEING", WeatherDataCollector.Seeing, "seeing [arcsec]"));
                    break;
            }
            switch (WeatherDataCollector.Wind)
            {
                case -1.0:
                    // Disconnected
                    goto case 100;
                case 100.0:
                    // Old data
                    cursor.Add(new HeaderCard("WIND", "UNKNOWN", "wind speed [m/s]"));
                    break;
                default:
                    cursor.Add(new HeaderCard("WIND", WeatherDataCollector.Wind, "wind speed [m/s]"));
                    break;
            }
            cursor.Add(new HeaderCard("SUN-ZD", WeatherDataCollector.Sun, "Sun zenith distance [deg]"));
            cursor.Add(new HeaderCard("MOONPHAS", 
                CoordinatesManager.MoonIllumination, "illuminated fraction of the Moon"));
            
            switch (CameraControl.loadedTask.FrameType)
            {
                case "Object":
                    
                    goto case "Flat";
                case "Flat":
                    cursor.Add(new HeaderCard("MOONANGL",
                        CoordinatesManager.ObjectDistance2Moon,
                        "angle between target and Moon [deg]"));
                    
                    break;
            }
            #endregion
        }
    }
}