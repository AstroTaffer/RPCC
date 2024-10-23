using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using ASCOM.Tools;
using RPCC.Comms;
using RPCC.Focus;
using RPCC.Tasks;
using RPCC.Utils;
using Timer = System.Timers.Timer;

namespace RPCC.Cams
{
    internal static class CameraControl
    {
        // _camsDomain = bitwise OR of 0x02 (USB interface) and 0x100 (Camera device)
        private const int _camsDomain = 0x02 | 0x100;
        private static readonly int[] _imageArea = new int[4];
        public static readonly object _camsLocker = new();   

        private static readonly Timer _camsTimer = new(1000);
        private static readonly List<Task> _readyImagesProcessList = new List<Task>();
        internal delegate void ResetUi();
        internal static ResetUi resetUi;
        internal delegate void ResetPics();
        internal static ResetPics resetPics;

        internal static CameraDevice[] cams = Array.Empty<CameraDevice>();
        internal static ObservationTask loadedTask = new ObservationTask();

        internal static bool IsConnected = false;
        private static bool _isCallbackRequired = false;
        private static int _readyCamNum;
        internal static int displayCamIndex = -1;

        #region Connect & Disconnect
        static internal bool ReconnectCameras()
        {
            bool isAllGood = true;

            if (IsConnected) isAllGood = DisconnectCameras();

            lock (_camsLocker)
            {
                DeviceName[] camerasNames = EnumerateCameras(_camsDomain);

                cams = new CameraDevice[camerasNames.Length];
                for (int i = 0; i < cams.Length; i++)
                {
                    cams[i] = new CameraDevice
                    {
                        fileName = camerasNames[i].FileName,
                        modelName = camerasNames[i].ModelName,
                        latestImageData = null,
                        latestImageFilename = null,
                        latestImageBitmap = null
                    };
                }
                Logger.AddLogEntry($"{cams.Length} cameras found");

                if (cams.Length > 0)
                {
                    if (!InitializeCameras()) isAllGood = false;
                    _camsTimer.Elapsed += CamsTimerTickAlt;
                    _camsTimer.Start();
                    resetUi();
                    IsConnected = true;
                    return isAllGood;
                }
            }

            return false;
        }

        static private bool InitializeCameras()
        {
            int errorLastFliCmd;
            bool isAllGood = true;

            // imageArea = [ul_x, ul_y, lr_x, lr_y]
            // Note that ul_x and ul_y are absolute (don't take hbin or vbin into account)
            // but lr_x and lr_y are relative (take hbin and vbin into account, but only after ul_x and ul_y)

            for (int i = 0; i < cams.Length; i++)
            {
                errorLastFliCmd = NativeMethods.FLIOpen(out cams[i].handle, cams[i].fileName, _camsDomain);
                if (errorLastFliCmd != 0)
                {
                    Logger.AddLogEntry($"WARNING Unable to connect to camera {i + 1}");
                    // -1 = FLI_INVALID_DEVICE
                    cams[i].handle = -1;
                    isAllGood = false;
                    continue;
                }

                var camSn = new StringBuilder(128);
                var len = new IntPtr(128);
                errorLastFliCmd = NativeMethods.FLIGetSerialString(cams[i].handle, camSn, len);
                if (errorLastFliCmd != 0)
                {
                    Logger.AddLogEntry($"WARNING Unable to get camera {i + 1} serial number");
                    cams[i].serialNumber = "ERROR";
                    isAllGood = false;
                }
                else cams[i].serialNumber = camSn.ToString();

                if (cams[i].serialNumber == Settings.SnCamG) cams[i].filter = "g";
                else if (cams[i].serialNumber == Settings.SnCamR) cams[i].filter = "r";
                else if (cams[i].serialNumber == Settings.SnCamI) cams[i].filter = "i";
                else
                {
                    Logger.AddLogEntry($"WARNING Unable to identify camera {i + 1} filter");
                    cams[i].filter = "UNKNOWN";
                }

                Logger.AddLogEntry($"Camera {i + 1}: Handle {cams[i].handle} | " +
                    $"Filename {cams[i].fileName} | " +
                    $"Model {cams[i].modelName} | Serial Number {cams[i].serialNumber} | " +
                    $"Filter {cams[i].filter}");

                // unchecked((int)0xffffffff) = FLI_FAN_SPEED_ON
                errorLastFliCmd = NativeMethods.FLISetFanSpeed(cams[i].handle, unchecked((int)0xffffffff));
                if (errorLastFliCmd != 0)
                {
                    Logger.AddLogEntry($"WARNING Unable to turn on camera {i + 1} fan");
                    isAllGood = false;
                }

                // 1 = FLI_MODE_16BIT
                errorLastFliCmd = NativeMethods.FLISetBitDepth(cams[i].handle, 1);
                if (errorLastFliCmd != 0)
                {
                    Logger.AddLogEntry($"WARNING Unable to set camera {i + 1} bit depth");
                    isAllGood = false;
                }

                // 0x0001 = FLI_BGFLUSH_START
                errorLastFliCmd = NativeMethods.FLIControlBackgroundFlush(cams[i].handle, 0x0001);
                if (errorLastFliCmd != 0)
                {
                    Logger.AddLogEntry($"WARNING Unable to turn on camera {i + 1} background flush");
                    isAllGood = false;
                }

                errorLastFliCmd = NativeMethods.FLISetNFlushes(cams[i].handle, Settings.NumFlushes);
                if (errorLastFliCmd != 0)
                {
                    Logger.AddLogEntry($"WARNING Unable to set camera {i + 1} number of flushes");
                    isAllGood = false;
                }

                errorLastFliCmd = NativeMethods.FLISetTemperature(cams[i].handle, Settings.CamTemp);
                if (errorLastFliCmd != 0)
                {
                    Logger.AddLogEntry($"WARNING Unable to set camera {i + 1} temperature");
                    isAllGood = false;
                }
            }

            // Sort cameras by ascending filters' wavelengths
            string[] filterOrder = { "g", "r", "i" };
            cams = cams.OrderBy(cam =>
            {
                int index = Array.IndexOf(filterOrder, cam.filter);
                // Console.WriteLine($"Sorting! Filer {cam.filter} => Index {index}");
                if (index == -1) return int.MaxValue;
                return index;
            }).ToArray();

            return isAllGood;
        }

        static internal bool DisconnectCameras()
        {
            bool isAllGood = true;

            lock (_camsLocker)
            {
                _camsTimer.Stop();
                _camsTimer.Elapsed -= CamsTimerTickAlt;
                IsConnected = false;

                int errorLastFliCmd;
                for (int i = 0; i < cams.Length; i++)
                {
                    errorLastFliCmd = NativeMethods.FLICancelExposure(cams[i].handle);
                    if (errorLastFliCmd != 0)
                    {
                        Logger.AddLogEntry($"WARNING Unable to cancel camera {i + 1} exposure");
                        isAllGood = false;
                    }

                    // 0x0000 = FLI_BGFLUSH_STOP
                    NativeMethods.FLIControlBackgroundFlush(cams[i].handle, 0x0000);
                    if (errorLastFliCmd != 0)
                    {
                        Logger.AddLogEntry($"WARNING Unable to turn off camera {i + 1} background flush");
                        isAllGood = false;
                    }

                    // 0x00 = FLI_FAN_SPEED_OFF
                    //errorLastFliCmd = NativeMethods.FLISetFanSpeed(cams[i].handle, 0x00);
                    //if (errorLastFliCmd != 0)
                    //{
                    //    Logger.AddLogEntry($"WARNING Unable to stop camera {i + 1} cooler");
                    //    isAllGood = false;
                    //}

                    errorLastFliCmd = NativeMethods.FLIClose(cams[i].handle);
                    if (errorLastFliCmd != 0)
                    {
                        Logger.AddLogEntry($"WARNING Unable to close camera {i + 1} handle");
                        isAllGood = false;
                    }
                }
                cams = Array.Empty<CameraDevice>();
                resetUi();
            }

            return isAllGood;
        }
        #endregion

        #region Status
        // To use these functions you must implement isExposing bool flag in each CameraDevice
        // and _isCallbackRequired bool flag in CameraControl
        static private void CamsTimerTickAlt(object sender, ElapsedEventArgs e)
        {
            _camsTimer.Stop();
            lock (_camsLocker)
            {
                GetCamsStatusAlt();

                _isCallbackRequired = false;
                _readyCamNum = 0;
                for (int i = 0; i < cams.Length; i++)
                {
                    int indx = i;
                    if (cams[i].status == "IDLE")
                    {
                        _readyCamNum++;
                        if (cams[indx].isExposing)
                        {
                            // Image ready
                            _readyImagesProcessList.Add(Task.Run(() => ProcessCapturedImage(cams[indx])));
                            cams[indx].isExposing = false;
                            _isCallbackRequired = true;
                        }
                    }
                }
                if (_readyImagesProcessList.Count > 0)
                {
                    Task.WaitAll(_readyImagesProcessList.ToArray());

                    if (_isCallbackRequired && _readyCamNum == cams.Length)
                    {
                        switch (loadedTask.FrameType)
                        {
                            case "Focus":
                                CameraFocus.CamFocusCallback();
                                break;
                            default:
                                Head.CamCallback();
                                break;
                        }

                        _readyImagesProcessList.Clear();
                        for (int i = 0; i < cams.Length; i++)
                        {
                            int indx = i;
                            if (!string.IsNullOrEmpty(cams[indx].latestImageFilename))
                            {
                                _readyImagesProcessList.Add(Task.Run(() => ConstructBitmap(cams[indx])));
                            }
                        }
                        Task.WaitAll(_readyImagesProcessList.ToArray());

                        resetPics();

                        for (int i = 0; i < cams.Length; i++)
                        {
                            cams[i].latestImageFilename = null;
                            cams[i].latestImageData = null; // Will be needed for Profile Image plotting
                            cams[i].latestImageBitmap = null;
                        }
                    }

                    _readyImagesProcessList.Clear();
                }
            }
            if (IsConnected) _camsTimer.Start();
        }

        static private void GetCamsStatusAlt()
        {
            int errorStatus;

            for (int i = 0; i < cams.Length; i++)
            {
                // 0x0000 = FLI_TEMPERATURE_CCD
                errorStatus = NativeMethods.FLIReadTemperature(cams[i].handle, 0x0000, out cams[i].ccdTemp);

                // 0x0001 = FLI_TEMPERATURE_BASE
                errorStatus += NativeMethods.FLIReadTemperature(cams[i].handle, 0x0001, out cams[i].baseTemp);

                errorStatus += NativeMethods.FLIGetCoolerPower(cams[i].handle, out cams[i].coolerPwr);

                errorStatus += NativeMethods.FLIGetDeviceStatus(cams[i].handle, out int deviceStatus);

                // There is no proper documentation on how to use FLIGetDeviceStatus command
                // But this solution have been working so far, so I'm leaving it here as a backup
                deviceStatus &= 0x03;
                switch (deviceStatus)
                {
                    // 0x00 = FLI_CAMERA_STATUS_IDLE
                    case 0x00:
                        cams[i].status = "IDLE";
                        break;
                    // 0x01 = FLI_CAMERA_STATUS_WAITING_FOR_TRIGGER
                    case 0x01:
                        cams[i].status = "WF TRIGGER";
                        break;
                    // 0x02 = FLI_CAMERA_STATUS_EXPOSING
                    case 0x02:
                        cams[i].status = "EXPOSING";
                        errorStatus += NativeMethods.FLIGetExposureStatus(cams[i].handle, out cams[i].remTime);
                        break;
                    // 0x03 = FLI_CAMERA_STATUS_READING_CCD
                    case 0x03:
                        cams[i].status = "READING CCD";
                        break;
                    default:
                        cams[i].status = "UNKNOWN";
                        Logger.AddLogEntry($"WARNING Unknown status {deviceStatus}");
                        break;
                }

                if (errorStatus != 0)
                {
                    cams[i].status = "ERROR";
                    Logger.AddLogEntry($"ERROR cam {cams[i].filter}");
                    Thread.Sleep(5000);
                    if (ReconnectCameras()) continue;
                    Logger.AddLogEntry($"ERROR cam can't reconect, stop cam timer");
                    _camsTimer.Stop();
                    Logger.SaveLogs();
                }
            }
        }
        #endregion

        #region Expose Frames
        internal static bool PrepareToObs(ObservationTask task, bool isCheck = false)
        {   
            
            string[] validFrameTypes = { "Object", "Bias", "Dark", "Flat", "Focus", "Test" };

            if (!Array.Exists(validFrameTypes, element => element == task.FrameType))
            {
                Logger.AddLogEntry($"WARNING Unknown frame type {task.FrameType}");
                return false;
            }

            // Safety checks
            switch (task.FrameType)
            {
                case "Bias":
                    if (task.Exp != 0)
                    {
                        Logger.AddLogEntry($"WARNING Incorrect exposure time {task.Exp} in bias-type observation task (expected 0)");
                    }
                    break;
                // case "Focus":
                    // if (task.Xbin != 1)
                    // {
                    //     Logger.AddLogEntry($"WARNING Incorrect Xbin factor {task.Xbin} in focus-type observation task (expected 1)");
                    // }
                    // if (task.Ybin != 1)
                    // {
                    //     Logger.AddLogEntry($"WARNING Incorrect Ybin factor {task.Ybin} in focus-type observation task (expected 1)");
                    // }
                    // break;
            }

            bool isAllGood = true;
            int errorLastFliCmd;

            int selCamsNum = 0;
            string[] selFilters = task.Filters.Split(' ');

            lock (_camsLocker)
            {
                for (int i = 0; i < cams.Length; i++)
                {
                    if (!Array.Exists(selFilters, element => element == cams[i].filter))
                    {
                        cams[i].isSelected = false;
                        continue;
                    }
                    else
                    {
                        cams[i].isSelected = true;
                        selCamsNum++;
                    }

                    // Frame type (shutter control)
                    switch (task.FrameType)
                    {
                        case "Bias":
                            goto case "Dark";
                        case "Dark":
                            // 1 = FLI_FRAME_TYPE_DARK
                            errorLastFliCmd = NativeMethods.FLISetFrameType(cams[i].handle, 1);
                            if (errorLastFliCmd != 0)
                            {
                                Logger.AddLogEntry($"WARNING Unable to set camera {i + 1} frame type");
                                isAllGood = false;
                            }
                            break;
                        default:
                            // 0 = FLI_FRAME_TYPE_NORMAL
                            errorLastFliCmd = NativeMethods.FLISetFrameType(cams[i].handle, 0);
                            if (errorLastFliCmd != 0)
                            {
                                Logger.AddLogEntry($"WARNING Unable to set camera {i + 1} frame type");
                                isAllGood = false;
                            }
                            break;
                    }

                    // Exposure
                    errorLastFliCmd = NativeMethods.FLISetExposureTime(cams[i].handle, task.Exp * 1000);
                    if (errorLastFliCmd != 0)
                    {
                        Logger.AddLogEntry($"WARNING Unable to set camera {i + 1} exposure time");
                        isAllGood = false;
                    }

                    // VBin, HBin and corresponding VisibleArea
                    errorLastFliCmd = NativeMethods.FLISetVBin(cams[i].handle, task.Xbin);
                    if (errorLastFliCmd != 0)
                    {
                        Logger.AddLogEntry($"WARNING Unable to set camera {i + 1} vbin");
                        isAllGood = false;
                    }
                    errorLastFliCmd = NativeMethods.FLISetHBin(cams[i].handle, task.Ybin);
                    if (errorLastFliCmd != 0)
                    {
                        Logger.AddLogEntry($"WARNING Unable to set camera {i + 1} hbin");
                        isAllGood = false;
                    }
                    errorLastFliCmd = NativeMethods.FLIGetVisibleArea(cams[i].handle,
                        out _imageArea[0], out _imageArea[1], out _imageArea[2], out _imageArea[3]);
                    if (errorLastFliCmd != 0)
                    {
                        Logger.AddLogEntry($"WARNING Unable to get camera {i + 1} visible area, using default values");
                        _imageArea[0] = 50;
                        _imageArea[1] = 2;
                        _imageArea[2] = 2098;
                        _imageArea[3] = 2050;
                    }
                    _imageArea[2] = _imageArea[0] + (_imageArea[2] - _imageArea[0]) / task.Xbin;
                    _imageArea[3] = _imageArea[1] + (_imageArea[3] - _imageArea[1]) / task.Ybin;
                    errorLastFliCmd = NativeMethods.FLISetImageArea(cams[i].handle,
                        _imageArea[0], _imageArea[1], _imageArea[2], _imageArea[3]);
                    if (errorLastFliCmd != 0)
                    {
                        Logger.AddLogEntry($"WARNING Unable to set camera {i + 1} image area");
                        isAllGood = false;
                    }

                    // Readout mode
                    switch (task.FrameType)
                    {
                        case "Focus":
                            // 0 = 2.0 MHz (Speed)
                            errorLastFliCmd = NativeMethods.FLISetCameraMode(cams[i].handle, 0);
                            if (errorLastFliCmd != 0)
                            {
                                Logger.AddLogEntry($"WARNING Unable to set camera {i + 1} readout mode");
                                isAllGood = false;
                            }
                            break;
                        default:
                            // 1 = 500KHz (Quality)
                            errorLastFliCmd = NativeMethods.FLISetCameraMode(cams[i].handle, 1);
                            if (errorLastFliCmd != 0)
                            {
                                Logger.AddLogEntry($"WARNING Unable to set camera {i + 1} readout mode");
                                isAllGood = false;
                            }
                            break;
                    }
                }
                if (selCamsNum == 0)
                {
                    Logger.AddLogEntry($"WARNING No cameras with specified filters {task.Filters}");
                    return false;
                }
            }

            if (isAllGood)
            {
                loadedTask = task;
            }
            else if (isCheck)
            {
                Head.IsThinking = false;
                Head.ThinkingTimer.Stop();
            }
            else
            {
                if (ReconnectCameras() & PrepareToObs(task, true))
                {
                    loadedTask = task;
                }
                else
                {
                    Head.IsThinking = false;
                    Head.ThinkingTimer.Stop();
                }
            }
            return isAllGood;
        }
        
        internal static bool StartExposure()
        {
            bool isAllGood = true;
            int errorLastFliCmd;

            lock (_camsLocker)
            {
                var dt = DateTime.UtcNow;
                var jd = AstroUtilities.JulianDateFromDateTime(dt);
                CoordinatesManager.CalculateObjectDistance2Moon(loadedTask);
                CoordinatesManager.MoonIllumination = AstroUtilities.MoonIllumination(jd);
                for (int i = 0; i < cams.Length; i++)
                {
                    if (!cams[i].isSelected) continue;

                    errorLastFliCmd = NativeMethods.FLIExposeFrame(cams[i].handle);
                    Logger.AddLogEntry($"Exposing {cams[i].filter}");
                    cams[i].expStartDt = dt;
                    cams[i].expStartJd = jd;
                    if (errorLastFliCmd != 0)
                    {
                        Logger.AddLogEntry($"WARNING Unable to start camera {i + 1} exposure");
                        isAllGood = false;
                    }
                    else cams[i].isExposing = true;  // Use with CamsTimerTickAlt
                }
                //if (cams.Length > 0) _isExposing = true;  // Use with CamsTimerTick
            }

            return isAllGood;
        }
        #endregion

        #region Read Frames
        private static void ProcessCapturedImage(CameraDevice cam)
        {
            // TODO: Add skipping image processing if something goes bad

            RpccFits latestImage = ReadImage(cam);
            cam.latestImageData = latestImage.data;
            cam.latestImageFilename = latestImage.SaveFitsFile(cam);
        }

        private static RpccFits ReadImage(CameraDevice cam)
        {
            Logger.AddDebugLogEntry($"Start read image from {cam.filter}");
            bool isAllGood = true;
            int imageWidth = _imageArea[2] - _imageArea[0];
            int imageHeight = _imageArea[3] - _imageArea[1];

            RpccFits imageFits = new RpccFits
            {
                data = new ushort[imageHeight][]
            };

            // There is an FLIGrabFrame command. Would be nice to try it out.
            for (int i = 0; i < imageHeight; i++)
            {
                imageFits.data[i] = new ushort[imageWidth];
                if (ReadImageRow(cam.handle, imageFits.data[i]) != 0) isAllGood = false;
            }
            if (!isAllGood) Logger.AddLogEntry($"ERROR Unable to read frame from {cam.serialNumber} camera");

            // Mirror image
            switch (cam.filter)
            {
                case "g":
                    if (MountDataCollector.IsLookingEast) imageFits.data = Rotate(imageFits.data);
                    break;
                case "r":
                    if (!MountDataCollector.IsLookingEast) imageFits.data = Rotate(imageFits.data);
                    break;
                case "i":
                    imageFits.data = MountDataCollector.IsLookingEast ? FlipV(imageFits.data) : FlipH(imageFits.data);
                    break;
            }

            return imageFits;
        }

        private static int ReadImageRow(int camHandle, ushort[] buff)
        {
            IntPtr buffWidth = new IntPtr(buff.Length);
            return NativeMethods.FLIGrabRow(camHandle, buff, buffWidth);
        }

        private static void ConstructBitmap(CameraDevice cam)
        {
            GeneralImageStat stat = new GeneralImageStat();
            stat.Calculate(cam.latestImageData);

            cam.latestImageBitmap = new Bitmap(cam.latestImageData.Length, cam.latestImageData[0].Length,
                PixelFormat.Format24bppRgb);

            int pixelColor;
            for (ushort i = 0; i < cam.latestImageData.Length; i++)
                for (ushort j = 0; j < cam.latestImageData[i].Length; j++)
                {
                    pixelColor = (int)((cam.latestImageData[i][j] - stat.dnrStart) * stat.dnrColorScale);
                    if (pixelColor < 0) pixelColor = 0;
                    if (pixelColor > 255) pixelColor = 255;
                    cam.latestImageBitmap.SetPixel(j, i, Color.FromArgb(pixelColor, pixelColor, pixelColor));
                }
        }
        #endregion

        #region Matrix rotate and flip

        private static ushort[][] Rotate(ushort[][] matrix)
        {
            // (I2, I1) = (I1, I2); 
            var xUp = matrix.GetUpperBound(0);
            for(var x = 0; x <= xUp; x++)
            {
                var yUp = matrix[0].GetUpperBound(0);
                for (var y = 0; y <= yUp/2; y++)
                {
                    var newX = xUp - x;
                    var newY = yUp - y;
                    if(newX == x & newY == y) continue;
                    (matrix[x][y], matrix[newX][newY]) = 
                        (matrix[newX][newY], matrix[x][y]);
                }   
            }
            return matrix;
        }

        private static ushort[][] FlipV(ushort[][] matrix)
        {
            var yUp = matrix[0].GetUpperBound(0);
            for(var y = 0; y <= yUp; y++)
            {
                var xUp = matrix.GetUpperBound(0);
                for (var x = 0; x <= xUp / 2; x++)
                {
                    var newX = xUp - x;
                    if(newX == x) continue;
                    (matrix[x][y], matrix[newX][y]) =
                        (matrix[newX][y], matrix[x][y]);
                }
            }

            return matrix;
        }

        private static ushort[][] FlipH(ushort[][] matrix)
        {
            var xUp = matrix.GetUpperBound(0);
            for(var x = 0; x <= xUp; x++)
            {
                var yUp = matrix[0].GetUpperBound(0);
                for (var y = 0; y <= yUp/2; y++)
                {
                    var newY = yUp - y;
                    if(newY == y) continue;
                    (matrix[x][y], matrix[x][newY]) =
                        (matrix[x][newY], matrix[x][y]);
                }
            }
            return matrix;
        }

        #endregion

        // It just works
        #region eliotg's code
        static private DeviceName[] EnumerateCameras(int domain)
        {
            // first, get the data, using an opaque token for the string array
            int errorLastFliCmd = NativeMethods.FLIList(domain, out IntPtr NamesHandle);
            if (errorLastFliCmd != 0)
            {
                Logger.AddLogEntry("WARNING Can't get list of FLIDevices");
                return new DeviceName[0];
            }

            // now marshal the string array into the return type we actually want
            List<DeviceName> NameList = new List<DeviceName>();
            IntPtr p = NamesHandle;
            string s;
            while (IntPtr.Zero != p)
            {
                // manually bring the string into managed memory
                s = ((StringWrapper)Marshal.PtrToStructure(p, typeof(StringWrapper))).s;
                if (null == s)
                    break;

                // parse it according to FLI SDK spec
                int DelimPos = s.IndexOf(';');
                DeviceName dn = new DeviceName
                {
                    FileName = (-1 == DelimPos ? s : s.Substring(0, DelimPos)),
                    ModelName = (-1 == DelimPos ? null : s.Substring(DelimPos + 1, s.Length - (DelimPos + 1)))
                };
                // and accumulate into our list
                NameList.Add(dn);

                // move to the next pointer
                p += sizeof(int);
            }

            // don't bother the caller with memory management now that we've made our own copy
            NativeMethods.FLIFreeList(NamesHandle);

            // render the result to the caller!
            return NameList.ToArray();
        }

        /// <summary>
        ///     Used by List() to store a list of enumerated devices
        /// </summary>
        private class DeviceName
        {
            /// <summary>
            ///     Formal device name needed by Open()
            /// </summary>
            public string FileName;

            /// <summary>
            ///     Model name or user assigned device name
            /// </summary>
            public string ModelName;
        }

        /// <summary>
        ///     Internal struct used for marshaling strings
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        private class StringWrapper
        {
            public string s;
        }
        #endregion
    }

    internal class CameraDevice
    {
        // id
        internal int handle;
        internal string fileName;
        internal string modelName;
        internal string serialNumber;
        internal string filter;

        // status
        internal double ccdTemp;
        internal double baseTemp;
        internal double coolerPwr;
        internal string status;
        internal int remTime;

        // exposure
        internal bool isSelected;
        internal bool isExposing;
        internal DateTime expStartDt;
        internal double expStartJd;

        // latest image
        internal string latestImageFilename;
        internal ushort[][] latestImageData;
        internal Bitmap latestImageBitmap;
    }
}