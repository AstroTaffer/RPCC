using RPCC.Tasks;
using RPCC.Utils;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using System.Timers;

namespace RPCC.Cams
{
    internal static class CameraControl
    {
        // camerasDomain = bitwise OR of 0x02 (USB interface) and 0x100 (Camera device)
        private const int _camDomain = 0x02 | 0x100;
        private static readonly int[] _imageArea = new int[4];

        private static readonly Timer _camsTimer = new Timer(1000);
        internal delegate void CallMainForm();
        internal static CallMainForm resetUi;

        internal static bool isConnected = false;
        internal static bool isFocusing = false;

        private static ObservationTask _loadedTask = new ObservationTask();
        internal static CameraDevice[] cams = Array.Empty<CameraDevice>();

        #region Connect & Disconnect
        static internal bool ReconnectCameras()
        {
            bool isAllGood = true;

            if (isConnected) isAllGood = DisconnectCameras();

            DeviceName[] camerasNames = EnumerateCameras(_camDomain);

            cams = new CameraDevice[camerasNames.Length];
            for (int i = 0; i < cams.Length; i++)
            {
                cams[i] = new CameraDevice
                {
                    fileName = camerasNames[i].FileName,
                    modelName = camerasNames[i].ModelName
                };
            }
            Logger.AddLogEntry($"{cams.Length} cameras found");

            if (cams.Length > 0)
            {
                if (!InitializeCameras()) isAllGood = false;
                _camsTimer.Elapsed += CamsTimerTick;
                _camsTimer.Start();
                resetUi();
                isConnected = true;
                return isAllGood;
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

            for (int i = 0; i < cams.Length;i++)
            {
                errorLastFliCmd = NativeMethods.FLIOpen(out cams[i].handle, cams[i].fileName, _camDomain);
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

            return isAllGood;
        }

        static internal bool DisconnectCameras()
        {
            bool isAllGood = true;

            _camsTimer.Stop();
            _camsTimer.Elapsed -= CamsTimerTick;
            isConnected = false;

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
                errorLastFliCmd = NativeMethods.FLISetFanSpeed(cams[i].handle, 0x00);
                if (errorLastFliCmd != 0)
                {
                    Logger.AddLogEntry($"WARNING Unable to stop camera {i + 1} cooler");
                    isAllGood = false;
                }
                
                errorLastFliCmd = NativeMethods.FLIClose(cams[i].handle);
                if (errorLastFliCmd != 0)
                {
                    Logger.AddLogEntry($"WARNING Unable to close camera {i + 1} handle");
                    isAllGood = false;
                }
            }
            cams = Array.Empty<CameraDevice>();
            resetUi();

            return isAllGood;
        }
        #endregion

        #region Status
        static private void CamsTimerTick(object sender, ElapsedEventArgs e)
        {
            _camsTimer.Stop();
            GetCamsStatus();
            _camsTimer.Start();
        }

        static internal void GetCamsStatus()
        {
            int errorStatus;

            foreach (var cam in cams)
            {
                errorStatus = 0;

                // 0x0000 = FLI_TEMPERATURE_CCD
                errorStatus = NativeMethods.FLIReadTemperature(cam.handle, 0x0000, out cam.ccdTemp);

                // 0x0001 = FLI_TEMPERATURE_BASE
                errorStatus += NativeMethods.FLIReadTemperature(cam.handle, 0x0001, out cam.baseTemp);

                errorStatus += NativeMethods.FLIGetCoolerPower(cam.handle, out cam.coolerPwr);

                errorStatus += NativeMethods.FLIGetDeviceStatus(cam.handle, out int deviceStatus);


                // TODO: Further testing required. In case of bugs use GetCamsStatusAlt() instead.
                switch (deviceStatus)
                {
                    case 0x00:
                        cam.status = "IDLE";
                        break;
                    case 0x01:
                        cam.status = "WF TRIGGER";
                        break;
                    case 0x02:
                        cam.status = "EXPOSING";
                        errorStatus += NativeMethods.FLIGetExposureStatus(cam.handle, out cam.remTime);
                        break;
                    case -2147483645:
                        cam.status = "READING CCD";
                        break;
                    case -2147483648:
                        cam.status = "DATA READY";
                        break;
                    default:
                        cam.status = "UNKNOWN";
                        Logger.AddLogEntry($"WARNING Unknown status {deviceStatus}");
                        break;
                }

                if (errorStatus != 0)
                {
                    cam.status = "ERROR";
                }
            }
        }

        static private void CamsTimerTickAlt(object sender, ElapsedEventArgs e)
        {
            _camsTimer.Stop();
            GetCamsStatusAlt();
            _camsTimer.Start();
        }

        static internal void GetCamsStatusAlt()
        {
            int errorStatus;

            foreach (var cam in cams)
            {
                errorStatus = 0;

                // 0x0000 = FLI_TEMPERATURE_CCD
                errorStatus = NativeMethods.FLIReadTemperature(cam.handle, 0x0000, out cam.ccdTemp);

                // 0x0001 = FLI_TEMPERATURE_BASE
                errorStatus += NativeMethods.FLIReadTemperature(cam.handle, 0x0001, out cam.baseTemp);

                errorStatus += NativeMethods.FLIGetCoolerPower(cam.handle, out cam.coolerPwr);

                errorStatus += NativeMethods.FLIGetDeviceStatus(cam.handle, out int deviceStatus);

                // There is no proper documentation on how to use FLIGetDeviceStatus command
                // But this solution have been working so far, so I'm leaving it here as a backup
                deviceStatus &= 0x03;
                switch (deviceStatus)
                {
                    // 0x00 = FLI_CAMERA_STATUS_IDLE
                    case 0x00:
                        cam.status = "IDLE";
                        break;
                    // 0x01 = FLI_CAMERA_STATUS_WAITING_FOR_TRIGGER
                    case 0x01:
                        cam.status = "WF TRIGGER";
                        break;
                    // 0x02 = FLI_CAMERA_STATUS_EXPOSING
                    case 0x02:
                        cam.status = "EXPOSING";
                        errorStatus += NativeMethods.FLIGetExposureStatus(cam.handle, out cam.remTime);
                        break;
                    // 0x03 = FLI_CAMERA_STATUS_READING_CCD
                    case 0x03:
                        cam.status = "READING CCD";
                        break;
                    default:
                        cam.status = "UNKNOWN";
                        Logger.AddLogEntry($"WARNING Unknown status {deviceStatus}");
                        break;
                }

                if (errorStatus != 0)
                {
                    cam.status = "ERROR";
                }
            }
        }
        #endregion

        #region Prepare
        internal static bool PrepareToObs(ObservationTask task)
        {
            string[] validFrameTypes = { "Object", "Bias", "Dark", "Flat", "Focus", "Test" };

            if (!Array.Exists(validFrameTypes, element => element == task.FrameType))
            {
                Logger.AddLogEntry($"WARNING Unknown frame type {task.FrameType}");
                return false;
            }

            bool isAllGood = true;
            int errorLastFliCmd;

            int selCamsNum = 0;
            string[] selFilters = task.Filters.Split(' ');

            LockCameras();
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
                switch (task.FrameType)
                {
                    case "Bias":
                        // Safety measure: exposure is explicitly set to 0 ms
                        errorLastFliCmd = NativeMethods.FLISetExposureTime(cams[i].handle, 0);
                        if (errorLastFliCmd != 0)
                        {
                            Logger.AddLogEntry($"WARNING Unable to set camera {i + 1} exposure time");
                            isAllGood = false;
                        }
                        break;
                    default:
                        errorLastFliCmd = NativeMethods.FLISetExposureTime(cams[i].handle, task.Exp * 1000);
                        if (errorLastFliCmd != 0)
                        {
                            Logger.AddLogEntry($"WARNING Unable to set camera {i + 1} exposure time");
                            isAllGood = false;
                        }
                        break;
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
            UnlockCameras();

            if (isAllGood) _loadedTask = task;
            return isAllGood;
        }
        #endregion

        #region Expose & Read Frames
        #endregion

        #region Lock & Unlock cameras
        // Technically if method "1" manages to lock camera "A" and method "2" manages to lock camera "B"
        // (almost) simultaneously, it would lead to a deadlock. But since foreach loop guarantees specific order
        // this should not be possible to achive even in the worst scenario, right? Right? Anyway, if a deadlock occurs,
        // create your own single object to lock() to and go in peace.
        private static void LockCameras()
        {
            foreach (var cam in cams)
            {
                NativeMethods.FLILockDevice(cam.handle);
            }
        }
        private static void UnlockCameras()
        {
            foreach (var cam in cams)
            {
                NativeMethods.FLIUnlockDevice(cam.handle);
            }
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
    }
}