using RPCC.Tasks;
using RPCC.Utils;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Timers;

namespace RPCC.Cams
{
    internal static class CameraControl
    {
        // camerasDomain = bitwise OR of 0x02 (USB interface) and 0x100 (Camera device)
        private const int camDomain = 0x02 | 0x100;

        private static readonly Timer camsTimer = new Timer(1000);

        internal static ObservationTask loadedTask = new ObservationTask();
        internal static CameraDevice[] cams = Array.Empty<CameraDevice>();

        #region Connect & Disconnect
        static internal void ReconnectCameras()
        {
            DisconnectCameras();

            DeviceName[] camerasNames = EnumerateCameras(camDomain);

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

            for (int i = 0; i < cams.Length;i++)
            {
                errorLastFliCmd = NativeMethods.FLIOpen(out cams[i].handle, cams[i].fileName, camDomain);
                if (errorLastFliCmd != 0)
                {
                    Logger.AddLogEntry($"WARNING Unable to connect to camera {i + 1}");
                    // -1 = FLI_INVALID_DEVICE
                    cams[i].handle = -1;
                    continue;
                }


            }

            camsTimer.Elapsed += CallCameras;
            camsTimer.Start();
        }

        static internal void DisconnectCameras()
        {
            camsTimer.Stop();
            camsTimer.Elapsed -= CallCameras;

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

        #region Timer
        static private void CallCameras(object sender, ElapsedEventArgs e)
        {

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
    }
}