﻿using ASCOM.Tools;
using RPCC.Focus;
using RPCC.Tasks;
using RPCC.Utils;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace RPCC.Cams
{
    internal static class CameraControl
    {
        // camerasDomain = bitwise OR of 0x02 (USB interface) and 0x100 (Camera device)
        private const int _camDomain = 0x02 | 0x100;
        private static readonly int[] _imageArea = new int[4];
        private static readonly object _camsLocker;

        private static readonly Timer _camsTimer = new Timer(1000);
        internal delegate void CallMainForm();
        internal static CallMainForm resetUi;
        private static readonly List<Task> _readyImagesProcessList = new List<Task>();

        internal static CameraDevice[] cams = Array.Empty<CameraDevice>();
        internal static ObservationTask loadedTask = new ObservationTask();

        internal static bool isConnected = false;
        private static bool _isExposing = false;
        private static int _readyCamNum;

        #region Connect & Disconnect
        static internal bool ReconnectCameras()
        {
            bool isAllGood = true;

            if (isConnected) isAllGood = DisconnectCameras();

            lock (_camsLocker)
            {
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

            lock (_camsLocker)
            {
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
        static private void CamsTimerTick(object sender, ElapsedEventArgs e)
        {
            _camsTimer.Stop();
            lock (_camsLocker)
            {
                GetCamsStatus();

                _readyCamNum = 0;
                for (int i = 0; i < cams.Length; i++)
                {
                    switch (cams[i].status)
                    {
                        case "DATA READY":
                            _readyImagesProcessList.Add(Task.Run(() => ProcessCapturedImage(i)));
                            goto case "IDLE";
                        case "IDLE":
                            _readyCamNum++;
                            break;
                    }
                }
                Task.WaitAll(_readyImagesProcessList.ToArray());
                if (_readyImagesProcessList.Count > 0) _readyImagesProcessList.Clear();

                if (_isExposing && _readyCamNum == cams.Length)
                {
                    _isExposing = false;
                    switch (loadedTask.FrameType)
                    {
                        case "Focus":
                            CameraFocus.CamFocusCallBack();
                            break;
                        default:
                            Head.CamCallback();
                            break;
                    }
                }
            }
            if (isConnected) _camsTimer.Start();
        }

        static private void GetCamsStatus()
        {
            int errorStatus;
            int deviceStatus;

            for (int i = 0; i < cams.Length; i++)
            {
                // 0x0000 = FLI_TEMPERATURE_CCD
                errorStatus = NativeMethods.FLIReadTemperature(cams[i].handle, 0x0000, out cams[i].ccdTemp);

                // 0x0001 = FLI_TEMPERATURE_BASE
                errorStatus += NativeMethods.FLIReadTemperature(cams[i].handle, 0x0001, out cams[i].baseTemp);

                errorStatus += NativeMethods.FLIGetCoolerPower(cams[i].handle, out cams[i].coolerPwr);

                errorStatus += NativeMethods.FLIGetDeviceStatus(cams[i].handle, out deviceStatus);

                // TODO: Further testing required. In case of bugs use GetCamsStatusAlt() instead.
                switch (deviceStatus)
                {
                    case 0x00:
                        cams[i].status = "IDLE";
                        break;
                    case 0x01:
                        cams[i].status = "WF TRIGGER";
                        break;
                    case 0x02:
                        cams[i].status = "EXPOSING";
                        errorStatus += NativeMethods.FLIGetExposureStatus(cams[i].handle, out cams[i].remTime);
                        break;
                    case -2147483645:
                        cams[i].status = "READING CCD";
                        break;
                    case -2147483648:
                        cams[i].status = "DATA READY";
                        break;
                    default:
                        cams[i].status = "UNKNOWN";
                        Logger.AddLogEntry($"WARNING Unknown status {deviceStatus}");
                        break;
                }

                if (errorStatus != 0) cams[i].status = "ERROR";
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
            }

            if (isAllGood) loadedTask = task;
            return isAllGood;
        }
        #endregion

        #region Expose & Read Frames
        internal static bool StartExposure()
        {
            bool isAllGood = true;
            int errorLastFliCmd;

            lock (_camsLocker)
            {
                for (int i = 0; i < cams.Length; i++)
                {
                    cams[i].latestImageFilename = "";

                    if (!cams[i].isSelected) continue;

                    errorLastFliCmd = NativeMethods.FLIExposeFrame(cams[i].handle);
                    cams[i].expStartDt = DateTime.UtcNow;
                    cams[i].expStartJd = Utilities.JulianDateFromDateTime(cams[i].expStartDt);
                    if (errorLastFliCmd != 0)
                    {
                        Logger.AddLogEntry($"WARNING Unable to start camera {i + 1} exposure");
                        isAllGood = false;
                    }
                }
                _isExposing = true;
            }

            return isAllGood;

        }

        private static void ProcessCapturedImage(int camId)
        {
            // Read image
            // latestImageData

            // Save image
            //latestImageFilename

            // Plot image (here?)
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
        internal DateTime expStartDt;
        internal double expStartJd;
        internal string latestImageFilename;
        internal ushort[][] latestImageData;
    }
}