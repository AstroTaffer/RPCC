using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

using FliProCameraLib;

namespace RPCC
{
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

    internal class CameraTask
    {
        internal int framesNum;
        internal string framesType;
        internal int framesExpTime;
        internal int viewCamIndex;
    }
    internal class CameraControl
    {
        // camerasDomain = bitwise OR of 0x02 (USB interface) and 0x100 (Camera device)
        private const int camDomain = 0x02 | 0x100;
        private int[] imageAreaAbsolute;
        private readonly Logger logger;
        private readonly Settings settings;

        internal CameraDevice[] cameras;
        internal CameraTask task;

        internal CameraControl(Logger logger, Settings settings)
        {
            this.logger = logger;
            this.settings = settings;
            task = new CameraTask();
            cameras = new CameraDevice[0];
        }

        internal void LaunchCameras()
        {
            DisconnectCameras();

            DeviceName[] camerasNames = EnumerateCameras(camDomain);

            cameras = new CameraDevice[cameras.Length];
            for (int i = 0; i < cameras.Length; i++)
            {
                cameras[i].fileName = camerasNames[i].FileName;
                // HACK: Check if this really a viable model name
                cameras[i].modelName = camerasNames[i].ModelName;
            }
            logger.AddLogEntry($"{cameras.Length} cameras found");

            if (cameras.Length > 0)
            {
                SetupCameras();
            }
        }

        // HACK: I have no idea what is going on in here
        // and I can't find it out without losing my sanity
        private DeviceName[] EnumerateCameras(int domain)
        {
            IntPtr NamesHandle;

            // first, get the data, using an opaque token for the string array
            int errorLastFliCmd = NativeMethods.FLIList(domain, out NamesHandle);
            if (errorLastFliCmd != 0)
            {
                logger.AddLogEntry("WARNING Can't get list of FLIDevices");
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

        private void SetupCameras()
        {
            int errorLastFliCmd;
            var imageAreaRelativeLrPoint = new int[2];

            for (int i = 0; i < cameras.Length; i++)
            {
                logger.AddLogEntry($"Connecting to camera {i + 1}");

                errorLastFliCmd = NativeMethods.FLIOpen(out cameras[i].handle, cameras[i].fileName, camDomain);
                if (errorLastFliCmd != 0)
                {
                    logger.AddLogEntry($"WARNING Unable to connect to camera {i + 1}");
                    // -1 = FLI_INVALID_DEVICE
                    cameras[i].handle = -1;
                    continue;
                }

                var camSn = new StringBuilder(128);
                var len = new IntPtr(128);
                errorLastFliCmd = NativeMethods.FLIGetSerialString(cameras[i].handle, camSn, len);
                if (errorLastFliCmd != 0)
                {
                    logger.AddLogEntry($"WARNING Unable to get camera {i + 1} serial number");
                    cameras[i].serialNumber = "ERROR";
                }
                else cameras[i].serialNumber = camSn.ToString();

                if (cameras[i].serialNumber == settings.SnCamG) cameras[i].filter = "g";
                else if (cameras[i].serialNumber == settings.SnCamR) cameras[i].filter = "r";
                else if (cameras[i].serialNumber == settings.SnCamI) cameras[i].filter = "i";
                else
                {
                    logger.AddLogEntry($"WARNING Unable to identify camera {i + 1} filter");
                    cameras[i].filter = "ERROR";
                }

                logger.AddLogEntry($"Camera {i + 1}: Handle {cameras[i].handle} | " +
                    $"Filename {cameras[i].fileName} | " +
                    $"Model {cameras[i].modelName} | Serial Number {cameras[i].serialNumber} | " +
                    $"Filter {cameras[i].filter}");

                // unchecked((int)0xffffffff) = FLI_FAN_SPEED_ON
                errorLastFliCmd = NativeMethods.FLISetFanSpeed(cameras[i].handle, unchecked((int)0xffffffff));
                if (errorLastFliCmd != 0) logger.AddLogEntry($"WARNING Unable to turn on camera {i + 1} fan");

                // 1 = FLI_MODE_16BIT
                errorLastFliCmd = NativeMethods.FLISetBitDepth(cameras[i].handle, 1);
                if (errorLastFliCmd != 0) logger.AddLogEntry($"WARNING Unable to set camera {i + 1} bit depth");

                // FIXME: Do our cameras support background flush?
                // 0x0001 = FLI_BGFLUSH_START
                errorLastFliCmd = NativeMethods.FLIControlBackgroundFlush(cameras[i].handle, 0x0001);
                if (errorLastFliCmd != 0) logger.AddLogEntry($"WARNING Unable to turn on camera {i + 1} background flush");

                errorLastFliCmd = NativeMethods.FLISetNFlushes(cameras[i].handle, settings.NumFlushes);
                if (errorLastFliCmd != 0) logger.AddLogEntry($"WARNING Unable to set camera {i + 1} number of flushes");

                errorLastFliCmd = NativeMethods.FLISetTemperature(cameras[i].handle, settings.CamTemp);
                if (errorLastFliCmd != 0) logger.AddLogEntry($"WARNING Unable to set camera {i + 1} temperature");


                if (i == 0)
                {
                    imageAreaAbsolute = new int[4];
                    errorLastFliCmd = NativeMethods.FLISetVBin(cameras[i].handle, 1);
                    if (errorLastFliCmd != 0) logger.AddLogEntry("WARNING Unable to reset camera 1 vbin");
                    errorLastFliCmd = NativeMethods.FLISetHBin(cameras[i].handle, 1);
                    if (errorLastFliCmd != 0) logger.AddLogEntry("WARNING Unable to reset camera 1 hbin");
                    errorLastFliCmd = NativeMethods.FLIGetVisibleArea(cameras[i].handle, out imageAreaAbsolute[0], out imageAreaAbsolute[1], out imageAreaAbsolute[2], out imageAreaAbsolute[3]);
                    if (errorLastFliCmd != 0)
                    {
                        // HACK: Maybe we should just use predefined constants of 2048 x 2048 pixels
                        logger.AddLogEntry("WARNING Unable to get camera 1 visible area, using predefined values");
                        imageAreaAbsolute[0] = 0;
                        imageAreaAbsolute[1] = 0;
                        imageAreaAbsolute[2] = 2048;
                        imageAreaAbsolute[3] = 2048;
                    }
                    imageAreaRelativeLrPoint[0] = imageAreaAbsolute[0] + (imageAreaAbsolute[2] - imageAreaAbsolute[0]) / settings.CamBin;
                    imageAreaRelativeLrPoint[1] = imageAreaAbsolute[1] + (imageAreaAbsolute[3] - imageAreaAbsolute[1]) / settings.CamBin;
                    // TODO: In future we may need to use img_height and img_width
                }
                errorLastFliCmd = NativeMethods.FLISetVBin(cameras[i].handle, settings.CamBin);
                if (errorLastFliCmd != 0) logger.AddLogEntry($"WARNING Unable to set camera {i + 1} vbin");
                errorLastFliCmd = NativeMethods.FLISetHBin(cameras[i].handle, settings.CamBin);
                if (errorLastFliCmd != 0) logger.AddLogEntry($"WARNING Unable to set camera {i + 1} hbin");
                errorLastFliCmd = NativeMethods.FLISetImageArea(cameras[i].handle, imageAreaAbsolute[0], imageAreaAbsolute[1], imageAreaRelativeLrPoint[0], imageAreaRelativeLrPoint[1]);
                if (errorLastFliCmd != 0) logger.AddLogEntry($"WARNING Unable to set camera {i + 1} image area");

                if (i == 0)
                {
                    var readoutMode = new StringBuilder(128);
                    var readoutModeLen = new IntPtr(128);
                    int modeIndex = 0;
                    while (true)
                    {
                        errorLastFliCmd = NativeMethods.FLIGetCameraModeString(cameras[i].handle, modeIndex, readoutMode, readoutModeLen);
                        if (errorLastFliCmd == 0)
                        {
                            settings.camRoModes.Add(readoutMode.ToString());
                        }
                        else
                        {
                            break;
                        }
                    }
                }

                errorLastFliCmd = NativeMethods.FLISetCameraMode(cameras[i].handle, settings.CamRoModeIndex);
                if (errorLastFliCmd != 0) logger.AddLogEntry($"WARNING Unable to set camera {i + 1} readout mode");
            }
        }

        internal void GetStatus()
        {
            int FliStatusError;
            int deviceStatus;

            foreach (var cam in cameras)
            {
                FliStatusError = 0;
                deviceStatus = 0;
                cam.remTime = 0;

                // 0x0000 = FLI_TEMPERATURE_CCD
                FliStatusError = NativeMethods.FLIReadTemperature(cam.handle, 0x0000, out cam.ccdTemp);

                // 0x0001 = FLI_TEMPERATURE_BASE
                FliStatusError += NativeMethods.FLIReadTemperature(cam.handle, 0x0001, out cam.baseTemp);

                FliStatusError += NativeMethods.FLIGetCoolerPower(cam.handle, out cam.coolerPwr);

                FliStatusError += NativeMethods.FLIGetDeviceStatus(cam.handle, out deviceStatus);
                // HACK: I don't know why it is done this way
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
                        FliStatusError += NativeMethods.FLIGetExposureStatus(cam.handle, out cam.remTime);
                        break;
                    // 0x03 = FLI_CAMERA_STATUS_READING_CCD
                    case 0x03:
                        cam.status = "READING CCD";
                        break;
                }

                if (FliStatusError != 0)
                {
                    cam.status = "ERROR";
                }
            }
        }
        
        internal void UpdateSettings()
        {
            int errorLastFliCmd;
            var imageAreaRelativeLrPoint = new int[2];

            for (int i = 0; i < cameras.Length; i++)
            {
                errorLastFliCmd = NativeMethods.FLISetNFlushes(cameras[i].handle, settings.NumFlushes);
                if (errorLastFliCmd != 0) logger.AddLogEntry($"WARNING Unable to set camera {i + 1} number of flushes");

                errorLastFliCmd = NativeMethods.FLISetTemperature(cameras[i].handle, settings.CamTemp);
                if (errorLastFliCmd != 0) logger.AddLogEntry($"WARNING Unable to set camera {i + 1} temperature");


                if (i == 0)
                {
                    imageAreaRelativeLrPoint[0] = imageAreaAbsolute[0] + (imageAreaAbsolute[2] - imageAreaAbsolute[0]) / settings.CamBin;
                    imageAreaRelativeLrPoint[1] = imageAreaAbsolute[1] + (imageAreaAbsolute[3] - imageAreaAbsolute[1]) / settings.CamBin;
                    // TODO: In future we may need to use img_height and img_width
                }
                errorLastFliCmd = NativeMethods.FLISetVBin(cameras[i].handle, settings.CamBin);
                if (errorLastFliCmd != 0) logger.AddLogEntry($"WARNING Unable to set camera {i + 1} vbin");
                errorLastFliCmd = NativeMethods.FLISetHBin(cameras[i].handle, settings.CamBin);
                if (errorLastFliCmd != 0) logger.AddLogEntry($"WARNING Unable to set camera {i + 1} hbin");
                errorLastFliCmd = NativeMethods.FLISetImageArea(cameras[i].handle, imageAreaAbsolute[0], imageAreaAbsolute[1], imageAreaRelativeLrPoint[0], imageAreaRelativeLrPoint[1]);
                if (errorLastFliCmd != 0) logger.AddLogEntry($"WARNING Unable to set camera {i + 1} image area");
                errorLastFliCmd = NativeMethods.FLISetCameraMode(cameras[i].handle, settings.CamRoModeIndex);
                if (errorLastFliCmd != 0) logger.AddLogEntry($"WARNING Unable to set camera {i + 1} readout mode");
            }

            logger.AddLogEntry("Cameras settings updated");
        }

        internal void SetSurveySettings()
        {
            // In FLI3GUI exposure settings were set before every frame
            // Though I'm sure that was superfluous, I want to mention it anyway
            // Just in case it could cause some bugs
            int errorLastFliCmd;

            switch (task.framesType)
            {
                case "Object":
                    for (int i = 0; i < cameras.Length; i++)
                    {
                        // 0 = FLI_FRAME_TYPE_NORMAL
                        errorLastFliCmd = NativeMethods.FLISetFrameType(cameras[i].handle, 0);
                        if (errorLastFliCmd != 0) logger.AddLogEntry($"WARNING Unable to set camera {i + 1} frame type");
                        errorLastFliCmd = NativeMethods.FLISetExposureTime(cameras[i].handle, task.framesExpTime);
                        if (errorLastFliCmd != 0) logger.AddLogEntry($"WARNING Unable to set camera {i + 1} exposure time");
                    }
                    break;
                case "Bias":
                    for (int i = 0; i < cameras.Length; i++)
                    {
                        // 1 = FLI_FRAME_TYPE_DARK
                        errorLastFliCmd = NativeMethods.FLISetFrameType(cameras[i].handle, 1);
                        if (errorLastFliCmd != 0) logger.AddLogEntry($"WARNING Unable to set camera {i + 1} frame type");
                        // Exposure time is explisitly set to 0 ms as a security feature
                        errorLastFliCmd = NativeMethods.FLISetExposureTime(cameras[i].handle, 0);
                        if (errorLastFliCmd != 0) logger.AddLogEntry($"WARNING Unable to set camera {i + 1} exposure time");
                    }
                    break;
                case "Dark":
                    for (int i = 0; i < cameras.Length; i++)
                    {
                        // 1 = FLI_FRAME_TYPE_DARK
                        errorLastFliCmd = NativeMethods.FLISetFrameType(cameras[i].handle, 1);
                        if (errorLastFliCmd != 0) logger.AddLogEntry($"WARNING Unable to set camera {i + 1} frame type");
                        errorLastFliCmd = NativeMethods.FLISetExposureTime(cameras[i].handle, task.framesExpTime);
                        if (errorLastFliCmd != 0) logger.AddLogEntry($"WARNING Unable to set camera {i + 1} exposure time");
                    }
                    break;
                case "Flat":
                    // 0 = FLI_FRAME_TYPE_NORMAL
                    goto case "Object";
                case "Test":
                    // 0 = FLI_FRAME_TYPE_NORMAL
                    errorLastFliCmd = NativeMethods.FLISetFrameType(cameras[task.viewCamIndex].handle, 0);
                    if (errorLastFliCmd != 0) logger.AddLogEntry($"WARNING Unable to set camera {task.viewCamIndex + 1} frame type");
                    errorLastFliCmd = NativeMethods.FLISetExposureTime(cameras[task.viewCamIndex].handle, task.framesExpTime);
                    if (errorLastFliCmd != 0) logger.AddLogEntry($"WARNING Unable to set camera {task.viewCamIndex + 1} exposure time");
                    break;
            }
        }

        internal void StartExposure()
        {
            int errorLastFliCmd;

            switch (task.framesType)
            {
                case "Object":
                    for (int i = 0; i < cameras.Length; i++)
                    {
                        errorLastFliCmd = NativeMethods.FLIExposeFrame(cameras[i].handle);
                        // Getting time just after exposure start, for better accuracy
                        cameras[i].expStartDt = DateTime.UtcNow;
                        if (errorLastFliCmd != 0) logger.AddLogEntry($"WARNING Unable to start camera {i + 1} exposure");
                        cameras[i].isExposing = true;
                    }
                    break;
                case "Bias":
                    goto case "Object";
                case "Dark":
                    goto case "Object";
                case "Flat":
                    goto case "Object";
                case "Test":
                    errorLastFliCmd = NativeMethods.FLIExposeFrame(cameras[task.viewCamIndex].handle);
                    // Getting time just after exposure start, for better accuracy
                    cameras[task.viewCamIndex].expStartDt = DateTime.UtcNow;
                    if (errorLastFliCmd != 0) logger.AddLogEntry($"WARNING Unable to start camera {task.viewCamIndex + 1} exposure");
                    cameras[task.viewCamIndex].isExposing = true;
                    break;
            }
        }

        internal void CancelSurvey()
        {
            // If FLICancelExposure returns error when canceling exposure on non-exposing cameras
            // Use switch(frameType) to distinguish "Test" survey
            // If "Test" then call FLICancelExposure for specific camera and disable View Radiobutton for survey time
            // If anything else then simply call FLICancelExposure on every camera
            // Don't think FLICancelExposure will cause any trouble though

            int errorLastFliCmd;

            for (int i = 0; i < cameras.Length; i++)
            {
                errorLastFliCmd = NativeMethods.FLICancelExposure(cameras[i].handle);
                if (errorLastFliCmd != 0) logger.AddLogEntry($"WARNING Unable to cancel camera {i + 1} exposure");
                cameras[i].isExposing = false;
            }
        }

        internal void DisconnectCameras()
        {
            foreach(var cam in cameras)
            {
                NativeMethods.FLIClose(cam.handle);
            }
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
    }
}