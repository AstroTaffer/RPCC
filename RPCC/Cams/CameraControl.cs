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

        private static readonly Timer _camsTimer = new Timer(1000);

        internal static ObservationTask loadedTask = new ObservationTask();
        internal static CameraDevice[] cams = Array.Empty<CameraDevice>();

        #region Connect & Disconnect
        static internal void ReconnectCameras()
        {
            DisconnectCameras();

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

            if (cams.Length > 0) InitializeCameras();
        }
        
        static private void InitializeCameras()
        {
            int errorLastFliCmd;

            // imageArea = [ul_x, ul_y, lr_x, lr_y]
            // Note that ul_x and ul_y are absolute (don't take hbin or vbin into account)
            // but lr_x and lr_y are relative (take hbin and vbin into account, but only after ul_x and ul_y)
            int[] imageArea = new int[4];

            for (int i = 0; i < cams.Length;i++)
            {
                errorLastFliCmd = NativeMethods.FLIOpen(out cams[i].handle, cams[i].fileName, _camDomain);
                if (errorLastFliCmd != 0)
                {
                    Logger.AddLogEntry($"WARNING Unable to connect to camera {i + 1}");
                    // -1 = FLI_INVALID_DEVICE
                    cams[i].handle = -1;
                    continue;
                }

                var camSn = new StringBuilder(128);
                var len = new IntPtr(128);
                errorLastFliCmd = NativeMethods.FLIGetSerialString(cams[i].handle, camSn, len);
                if (errorLastFliCmd != 0)
                {
                    Logger.AddLogEntry($"WARNING Unable to get camera {i + 1} serial number");
                    cams[i].serialNumber = "ERROR";
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
                if (errorLastFliCmd != 0) Logger.AddLogEntry($"WARNING Unable to turn on camera {i + 1} fan");

                // 1 = FLI_MODE_16BIT
                errorLastFliCmd = NativeMethods.FLISetBitDepth(cams[i].handle, 1);
                if (errorLastFliCmd != 0) Logger.AddLogEntry($"WARNING Unable to set camera {i + 1} bit depth");

                // 0x0001 = FLI_BGFLUSH_START
                errorLastFliCmd = NativeMethods.FLIControlBackgroundFlush(cams[i].handle, 0x0001);
                if (errorLastFliCmd != 0) Logger.AddLogEntry($"WARNING Unable to turn on camera {i + 1} background flush");

                errorLastFliCmd = NativeMethods.FLISetNFlushes(cams[i].handle, Settings.NumFlushes);
                if (errorLastFliCmd != 0) Logger.AddLogEntry($"WARNING Unable to set camera {i + 1} number of flushes");

                errorLastFliCmd = NativeMethods.FLISetTemperature(cams[i].handle, Settings.CamTemp);
                if (errorLastFliCmd != 0) Logger.AddLogEntry($"WARNING Unable to set camera {i + 1} temperature");

                errorLastFliCmd = NativeMethods.FLISetVBin(cams[i].handle, Settings.CamBin);
                if (errorLastFliCmd != 0) Logger.AddLogEntry($"WARNING Unable to set camera {i + 1} vbin");
                errorLastFliCmd = NativeMethods.FLISetHBin(cams[i].handle, Settings.CamBin);
                if (errorLastFliCmd != 0) Logger.AddLogEntry($"WARNING Unable to set camera {i + 1} hbin");
                
                if (i == 0)
                {
                    errorLastFliCmd = NativeMethods.FLIGetVisibleArea(cams[i].handle, out imageArea[0], out imageArea[1], out imageArea[2], out imageArea[3]);
                    if (errorLastFliCmd != 0)
                    {
                        Logger.AddLogEntry("WARNING Unable to get camera 1 visible area, using default values");
                        imageArea[0] = 50;
                        imageArea[1] = 2;
                        imageArea[2] = 2098;
                        imageArea[3] = 2050;
                    }

                    imageArea[2] = imageArea[0] + (imageArea[2] - imageArea[0]) / Settings.CamBin;
                    imageArea[3] = imageArea[1] + (imageArea[3] - imageArea[1]) / Settings.CamBin;
                }

                errorLastFliCmd = NativeMethods.FLISetImageArea(cams[i].handle, imageArea[0], imageArea[1], imageArea[2], imageArea[3]);
                if (errorLastFliCmd != 0) Logger.AddLogEntry($"WARNING Unable to set camera {i + 1} image area");

                switch (Settings.CamRoMode)
                {
                    // 0 = 2.0 MHz
                    case "2.0 MHz":
                        errorLastFliCmd = NativeMethods.FLISetCameraMode(cams[i].handle, 0);
                        break;
                    // 1 = 500KHz
                    case "500KHz":
                        errorLastFliCmd = NativeMethods.FLISetCameraMode(cams[i].handle, 1);
                        break;
                }
                if (errorLastFliCmd != 0) Logger.AddLogEntry($"WARNING Unable to set camera {i + 1} readout mode");
            }

            _camsTimer.Elapsed += CamsTimerTick;
            _camsTimer.Start();
        }

        static internal void DisconnectCameras()
        {
            _camsTimer.Stop();
            _camsTimer.Elapsed -= CamsTimerTick;

            int errorLastFliCmd;
            for (int i = 0; i < cams.Length; i++)
            {
                errorLastFliCmd = NativeMethods.FLICancelExposure(cams[i].handle);
                if (errorLastFliCmd != 0) Logger.AddLogEntry($"WARNING Unable to cancel camera {i + 1} exposure");
                
                // 0x00 = FLI_FAN_SPEED_OFF
                errorLastFliCmd = NativeMethods.FLISetFanSpeed(cams[i].handle, 0x00);
                if (errorLastFliCmd != 0) Logger.AddLogEntry($"WARNING Unable to stop camera {i + 1} cooler");
                
                errorLastFliCmd = NativeMethods.FLIClose(cams[i].handle);
                if (errorLastFliCmd != 0) Logger.AddLogEntry($"WARNING Unable to close camera {i + 1} handle");
            }
        }
        #endregion

        #region Status
        static private void CamsTimerTick(object sender, ElapsedEventArgs e)
        {

        }

        static internal void GetCamsStatus()
        {
            int errorStatus;

            foreach (var cam in cams)
            {
                errorStatus = 0;
                cam.remTime = 0;

                // 0x0000 = FLI_TEMPERATURE_CCD
                errorStatus = NativeMethods.FLIReadTemperature(cam.handle, 0x0000, out cam.ccdTemp);

                // 0x0001 = FLI_TEMPERATURE_BASE
                errorStatus += NativeMethods.FLIReadTemperature(cam.handle, 0x0001, out cam.baseTemp);

                errorStatus += NativeMethods.FLIGetCoolerPower(cam.handle, out cam.coolerPwr);

                errorStatus += NativeMethods.FLIGetDeviceStatus(cam.handle, out int deviceStatus);

                // I don't know why it's done this way. Someday I'll run corresponding tests and find out. Someday.
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
                        break;
                }

                if (errorStatus != 0)
                {
                    cam.status = "ERROR";
                }
            }
        }
        #endregion

        // It just works
        #region eliotg's code
        static private DeviceName[] EnumerateCameras(int domain)
        {
            IntPtr NamesHandle;

            // first, get the data, using an opaque token for the string array
            int errorLastFliCmd = NativeMethods.FLIList(domain, out NamesHandle);
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
                DeviceName dn = new DeviceName();
                dn.FileName = (-1 == DelimPos ? s : s.Substring(0, DelimPos));
                dn.ModelName = (-1 == DelimPos ? null : s.Substring(DelimPos + 1, s.Length - (DelimPos + 1)));
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
        /// Used by List() to store a list of enumerated devices
        /// </summary>
        private class DeviceName
        {
            /// <summary>
            /// Formal device name needed by Open()
            /// </summary>
            public string FileName;

            /// <summary>
            /// Model name or user assigned device name
            /// </summary>
            public string ModelName;
        }

        /// <summary>
        /// Internal struct used for marshaling strings
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
        internal bool isExposing;
        internal DateTime expStartDt;
        internal double expStartJd;
    }
}