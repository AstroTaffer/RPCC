using System;
using System.Drawing;
using RPCC.Tasks;
using RPCC.Utils;

namespace RPCC.Cams;

internal class FliCameraDevice : ICameraDevice
{
    // _camsDomain = bitwise OR of 0x02 (USB interface) and 0x100 (Camera device)
    // public int CamsDomain { get; set; } = 0x02 | 0x100;
    private readonly Fli _cam;
    public int[] ImageArea { get; set; } = new int[4];
    // public int Handle { get; set; }
    public string FileName { get; set; }
    public string ModelName { get; set; }
    public string SerialNumber { get; set; }
    public double PixelSizeX { get; set; }
    public double PixelSizeY { get; set; }
    public string Filter { get; set; }
    public double CcdTemp { get; set; }
    public double BaseTemp { get; set; }
    public double CoolerPwr { get; set; }
    public string Status { get; set; }
    public int RemTime { get; set; }
    // public bool IsSelected { get; set; }
    public bool IsExposing { get; set; }
    public DateTime ExpStartDt { get; set; }
    public double ExpStartJd { get; set; }
    public string LatestImageFilename { get; set; }
    public ushort[,] LatestImageData { get; set; }
    public Bitmap LatestImageBitmap { get; set; }
    
    // public bool GrabRow(ushort[] buff)
    // {
    //     var buffWidth = new IntPtr(buff.Length);
    //     return NativeMethods.FLIGrabRow(Handle, buff, buffWidth) == 0;
    // }

    public RpccFits GetRpccFits()
    {
        var imageWidth = ImageArea[2] - ImageArea[0];
        var imageHeight = ImageArea[3] - ImageArea[1];
        var buff = new ushort[imageHeight, imageWidth];
        try
        {
            _cam.GrabFrame(buff);
        }
        catch (Exception e)
        {
            Logger.AddError("grab frame", e, this);
            return null;
        }
        
        return new RpccFits{Data = buff};
    }
    
    public FliCameraDevice(Fli.DeviceName name)
    {
        FileName = name.FileName;
        ModelName = name.ModelName;
        try
        {
            _cam = new Fli(name.FileName, Fli.DOMAIN.CAMERA | Fli.DOMAIN.USB);
        }
        catch (Exception e)
        {
            Logger.AddError("connect to fli camera", e, this);
            return;
        }
        
        LatestImageData = null;
        LatestImageFilename = null;
        LatestImageBitmap = null;
        
        try
        {
            SerialNumber = _cam.GetSerialString();
        
        
            if (SerialNumber == Settings.SnCamG)
            {
                Filter = StringHolder.FilG;
            }
            else if (SerialNumber == Settings.SnCamR)
            {
                Filter = StringHolder.FilR;
            }
            else if (SerialNumber == Settings.SnCamI)
            {
                Filter = StringHolder.FilI;
            }
            else if (SerialNumber == Settings.SnCamV)
            {
                Filter = StringHolder.FilV;
            }
            else
            {
                Logger.AddLogEntry("WARNING Unable to identify fli camera filter");
                Filter = StringHolder.Unknown;
            }

            _cam.GetPixelSize(out var xbuf, out var ybuf);
            PixelSizeX = xbuf;
            PixelSizeY = ybuf;
            
            _cam.SetFanSpeed(Fli.FAN_SPEED.ON);
            _cam.SetBitDepth(Fli.BIT_DEPTH.MODE_16BIT);
            _cam.ControlBackgroundFlush(Fli.BGFLUSH.START);
            // _cam.SetNFlushes(Settings.NumFlushes);
            _cam.SetTemperature(Settings.CamTemp);
        }
        catch (Exception e)
        {
            Logger.AddError("initialize fli cam", e, this);
            SerialNumber = StringHolder.Error;
            Filter = StringHolder.Error;
            return;
        }
        
        Logger.AddLogEntry($"Connect camera: Filename {FileName} | " +
                           $"Model {ModelName} | Serial Number {SerialNumber} | " +
                           $"Filter {Filter} | PixelSizeX {PixelSizeX} | PixelSizeY {PixelSizeY}");
    }

    public bool Close()
    {
        try
        {
            _cam.CancelExposure();
            _cam.ControlBackgroundFlush(Fli.BGFLUSH.STOP);
            _cam.Close();
        }
        catch (Exception e)
        {
            Logger.AddError("fli close", e, this);
            return false;
        }
        return true;
    }
        
    public bool GetCamStatusAlt()
        {
            try
            {
                CcdTemp = _cam.ReadTemperature(Fli.CHANNEL.CCD);
                BaseTemp = _cam.ReadTemperature(Fli.CHANNEL.BASE);
                CoolerPwr = _cam.GetCoolerPower();

                // There is no proper documentation on how to use FLIGetDeviceStatus command
                // But this solution have been working so far, so I'm leaving it here as a backup
                // deviceStatus &= 0x03;
                var deviceStatus = _cam.GetDeviceStatus();
                switch (deviceStatus)
                {
                    // 0x00 = FLI_CAMERA_STATUS_IDLE
                    case Fli.STATUS.CAMERA_STATUS_IDLE:
                        Status = StringHolder.Idle;
                        break;
                    // 0x01 = FLI_CAMERA_STATUS_WAITING_FOR_TRIGGER
                    case Fli.STATUS.CAMERA_STATUS_WAITING_FOR_TRIGGER:
                        Status = StringHolder.Wft;
                        break;
                    // 0x02 = FLI_CAMERA_STATUS_EXPOSING
                    case Fli.STATUS.CAMERA_STATUS_EXPOSING:
                        Status = StringHolder.Exposing;
                        // int buff;
                        // errorStatus += NativeMethods.FLIGetExposureStatus(Handle, out buff);
                        RemTime = _cam.GetExposureStatus();
                        break;
                    // 0x03 = FLI_CAMERA_STATUS_READING_CCD
                    case Fli.STATUS.CAMERA_STATUS_READING_CCD:
                        Status = StringHolder.Reading;
                        break;
                    case Fli.STATUS.CAMERA_STATUS_UNKNOWN:
                    case Fli.STATUS.CAMERA_DATA_READY:
                    case Fli.STATUS.FOCUSER_STATUS_HOMING:
                    case Fli.STATUS.FOCUSER_STATUS_MOVING_MASK:
                    case Fli.STATUS.FOCUSER_STATUS_HOME:
                    case Fli.STATUS.FOCUSER_STATUS_LIMIT:
                    case Fli.STATUS.FOCUSER_STATUS_LEGACY:
                    case Fli.STATUS.FILTER_WHEEL_PHYSICAL:
                    case Fli.STATUS.FILTER_WHEEL_RIGHT:
                    case Fli.STATUS.FILTER_POSITION_UNKNOWN:
                    case Fli.STATUS.FILTER_POSITION_CURRENT:
                    case Fli.STATUS.FILTER_STATUS_HOME_SUCCEEDED:
                    default:
                        Status = StringHolder.Unknown;
                        Logger.AddLogEntry($"WARNING Unknown status {deviceStatus}");
                        break;
                }
            }
            catch (Exception e)
            {
                Status = StringHolder.Error;
                Logger.AddError("get cam status", e, this);
                return false;
            }
            return true;
        }

    // public bool SetFrameType(int frameType)
    // {
    //     // 0 = FLI_FRAME_TYPE_NORMAL
    //     // 1 = FLI_FRAME_TYPE_DARK
    //     try
    //     {
    //         switch (frameType)
    //         {
    //             case 0:
    //                 _cam.SetFrameType(Fli.FRAME_TYPE.NORMAL);
    //                 break;
    //             case 1:
    //                 _cam.SetFrameType(Fli.FRAME_TYPE.DARK);
    //                 break;
    //         }
    //     }
    //     catch (Exception e)
    //     {
    //         Logger.AddError("set frame type", e, this);
    //         return false;
    //     }
    //
    //     return true;
    // }

    // public bool SetExposureTime(int exposureTime)
    // {
    //     try
    //     {
    //         _cam.SetExposureTime(exposureTime * 1000);
    //     }
    //     catch (Exception e)
    //     {
    //         Logger.AddError("set exp time", e, this);
    //         return false;
    //     }
    //
    //     return true;
    // }

    public bool SetBin(int vBin, int hBin)
    {
        try
        {
            _cam.SetHBin(hBin);
            _cam.SetVBin(vBin);
        }
        catch (Exception e)
        {
            Logger.AddError("set bin", e, this);
            return false;
        }

        return true;
    }

    public bool SetImageArea(int xbin, int ybin)    
    {
        try
        {
            _cam.GetVisibleArea(out ImageArea[0], out ImageArea[1], out ImageArea[2], out ImageArea[3]);
            ImageArea[2] = ImageArea[0] + (ImageArea[2] - ImageArea[0]) / xbin;
            ImageArea[3] = ImageArea[1] + (ImageArea[3] - ImageArea[1]) / ybin;
        }
        catch (Exception e)
        {
            ImageArea[0] = 50;
            ImageArea[1] = 2;
            ImageArea[2] = 2098;
            ImageArea[3] = 2050;
            Logger.AddError("get visible area", e, this);
        }
    
        try
        {
            _cam.SetImageArea(ImageArea[0], ImageArea[1], 
                ImageArea[2], ImageArea[3]);
        }
        catch (Exception e)
        {
            Logger.AddError("set image area", e, this);
            return false;
        }
        
        return true;
    }

    // public bool SetCameraReadoutMode(int mode)
    // {
    //     // 0 = 2.0 MHz (Speed)
    //     // 1 = 500KHz (Quality)
    //     try
    //     {
    //         _cam.SetCameraMode(mode);
    //     }
    //     catch (Exception e)
    //     {
    //         Logger.AddError("set fli cam mode", e, this);
    //         return false;
    //     }
    //
    //     return true;
    // }

    public bool Exposure(ObservationTask task)
    {   
        try
        {
            SetBin(task.Xbin, task.Ybin);
            SetImageArea(task.Xbin, task.Ybin);
            // Frame type (shutter control)
            switch (task.FrameType)
            {
                case StringHolder.Dark:
                    _cam.SetFrameType(Fli.FRAME_TYPE.DARK);
                    break;
                default:
                    _cam.SetFrameType(Fli.FRAME_TYPE.NORMAL);
                    break;
            }
            //in msec
            _cam.SetExposureTime(task.Exp * 1000);
            _cam.ExposeFrame();
        }
        catch (Exception e)
        {
            Logger.AddError("start exp", e, this);
            return false;
        }

        return true;
    }
}