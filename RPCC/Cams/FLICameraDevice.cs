using System;
using System.Drawing;
using System.Threading;
using RPCC.Tasks;
using RPCC.Utils;

namespace RPCC.Cams;

internal class FliCameraDevice : ICameraDevice
{
    // _camsDomain = bitwise OR of 0x02 (USB interface) and 0x100 (Camera device)
    // public int CamsDomain { get; set; } = 0x02 | 0x100;
    public CameraUiBlock UiBlock { get; set; }
    private readonly object _camSync = new();
    private readonly Fli _cam;
    public int[] ImageArea { get; set; } = new int[4];
    // public int Handle { get; set; }
    
    private T ExecuteWithDeviceLock<T>(string opName, Func<T> action)
    {
        lock (_camSync)
        {
            var locked = false;
            try
            {
                _cam.LockDevice();
                locked = true;
                return action();
            }
            finally
            {
                if (locked)
                {
                    try { _cam.UnlockDevice(); }
                    catch (Exception e) { Logger.AddError($"{opName}: unlock device", e, this); }
                }
            }
        }
    }

    private void ExecuteWithDeviceLock(string opName, Action action)
    {
        ExecuteWithDeviceLock<object>(opName, () =>
        {
            action();
            return null;
        });
    }
    
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
    public int LastImageId { get; set; }
    public ushort[,] LatestImageData { get; set; }
    public Bitmap LatestImageBitmap { get; set; }
    public CameraSettingsCollector SettingsCollector { get; set; }
    
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
            ExecuteWithDeviceLock("grab frame", () =>
            {
                _cam.GrabFrame(buff);
            });
        }
        catch (Exception e)
        {
            Logger.AddError("grab frame", e, this);
            // return null;
            try
            {
                Thread.Sleep(100);
                ExecuteWithDeviceLock("grab frame retry", () =>
                {
                    _cam.GrabFrame(buff);
                });
                Logger.AddDebugLogEntry($"grab frame retry success: {Filter}");
            }
            catch (Exception e2)
            {
                Logger.AddError("grab frame retry", e2, this);
                return null;
            }
        }
        return new RpccFits{Data = buff};
    }
    
    public FliCameraDevice(Fli.DeviceName name)
    {
        /* TODO:
             * - Настройка подключаемой камеры через функцию GetCameraSettingsSet(string id)
             * - Настройка CamTemp теперь является частью CameraSettingsCollector
             * - Удалить if-else на SnCamG/R/I/V
         */

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
            // SerialNumber = _cam.GetSerialString();
            
            SerialNumber = ExecuteWithDeviceLock("get serial", () => _cam.GetSerialString());
            SettingsCollector = Settings.GetCameraSettingsSet(SerialNumber);
            Filter = SettingsCollector.Filter;

            SetBin(SettingsCollector.Bin, SettingsCollector.Bin);
            
            // _cam.GetPixelSize(out var xbuf, out var ybuf);
            double xbuf = 0, ybuf = 0;
            ExecuteWithDeviceLock("get pixel size", () => _cam.GetPixelSize(out xbuf, out ybuf));
            
            PixelSizeX = Math.Round(xbuf * 1e6, 2);
            PixelSizeY = Math.Round(ybuf * 1e6, 2);
            
            ExecuteWithDeviceLock("initialize camera", () =>
            {
                _cam.SetFanSpeed(Fli.FAN_SPEED.ON);
                _cam.SetBitDepth(Fli.BIT_DEPTH.MODE_16BIT);
                _cam.ControlBackgroundFlush(Fli.BGFLUSH.START);
                // _cam.SetNFlushes(Settings.NumFlushes);
                // _cam.SetTemperature(Settings.CamTemp);
                _cam.SetTemperature(SettingsCollector.TempSetpoint);
            });
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
                           $"Filter {Filter} | PixelSizeX {PixelSizeX} um | PixelSizeY {PixelSizeY} um");
    }

    public bool Close()
    {
        try
        {
            Logger.AddDebugLogEntry($"Close cam {SerialNumber}");
            // _cam.CancelExposure();
            // _cam.ControlBackgroundFlush(Fli.BGFLUSH.STOP);
            // _cam.Close();
            
            lock (_camSync)
            {
                var locked = false;
                try
                {
                    _cam.LockDevice();
                    locked = true;
                    _cam.CancelExposure();
                    _cam.ControlBackgroundFlush(Fli.BGFLUSH.STOP);
                }
                finally
                {
                    if (locked) try { _cam.UnlockDevice(); } catch { /* ignore */ }
                }
                _cam.Close();
            }
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
                // CcdTemp = _cam.ReadTemperature(Fli.CHANNEL.CCD);
                // BaseTemp = _cam.ReadTemperature(Fli.CHANNEL.BASE);
                // CoolerPwr = _cam.GetCoolerPower();
                //
                // // There is no proper documentation on how to use FLIGetDeviceStatus command
                // // But this solution have been working so far, so I'm leaving it here as a backup
                // // deviceStatus &= 0x03;
                // var deviceStatus = _cam.GetDeviceStatus();
                
                Fli.STATUS deviceStatus = 0;
                ExecuteWithDeviceLock("get cam status", () =>
                {
                    CcdTemp = _cam.ReadTemperature(Fli.CHANNEL.CCD);
                    BaseTemp = _cam.ReadTemperature(Fli.CHANNEL.BASE);
                    CoolerPwr = _cam.GetCoolerPower();
                    deviceStatus = _cam.GetDeviceStatus();
                    if ((deviceStatus & Fli.STATUS.CAMERA_STATUS_EXPOSING) != 0)
                        RemTime = _cam.GetExposureStatus() / 1000;
                });
                
                // FLI status is a bit mask, not a single enum value.
                if ((deviceStatus & Fli.STATUS.CAMERA_STATUS_EXPOSING) != 0)
                {
                    Status = StringHolder.Exposing;
                    IsExposing = false;
                }
                else if ((deviceStatus & Fli.STATUS.CAMERA_STATUS_READING_CCD) != 0)
                {
                    Status = StringHolder.Exposing;
                    IsExposing = false;
                }
                else if ((deviceStatus & Fli.STATUS.CAMERA_DATA_READY) != 0)
                {
                    Status = StringHolder.Idle;
                    IsExposing = true;
                    RemTime = 0;
                }
                else if ((deviceStatus & Fli.STATUS.CAMERA_STATUS_IDLE) != 0)
                {
                    Status = StringHolder.Idle;
                    IsExposing = false;
                    RemTime = 0;
                }
                else
                {
                    Status = StringHolder.Error;
                    IsExposing = false;
                    Logger.AddLogEntry($"Unknown FLI camera status: {deviceStatus} ({Filter})");
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
            // _cam.SetHBin(hBin);
            // _cam.SetVBin(vBin);
            ExecuteWithDeviceLock("set bin", () =>
            {
                _cam.SetHBin(hBin);
                _cam.SetVBin(vBin);
            });
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
            // _cam.GetVisibleArea(out ImageArea[0], out ImageArea[1], out ImageArea[2], out ImageArea[3]);
            // ImageArea[2] = ImageArea[0] + (ImageArea[2] - ImageArea[0]) / xbin;
            // ImageArea[3] = ImageArea[1] + (ImageArea[3] - ImageArea[1]) / ybin;
            
            ExecuteWithDeviceLock("set image area", () =>
            {
                _cam.GetVisibleArea(out ImageArea[0], out ImageArea[1], out ImageArea[2], out ImageArea[3]);
                ImageArea[2] = ImageArea[0] + (ImageArea[2] - ImageArea[0]) / xbin;
                ImageArea[3] = ImageArea[1] + (ImageArea[3] - ImageArea[1]) / ybin;
                _cam.SetImageArea(ImageArea[0], ImageArea[1],
                    ImageArea[2], ImageArea[3]);
            });
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
            ExecuteWithDeviceLock("start exp", () =>
            {
            //     case StringHolder.Dark:
            //         _cam.SetFrameType(Fli.FRAME_TYPE.DARK);
            //         break;
            //     default:
            //         _cam.SetFrameType(Fli.FRAME_TYPE.NORMAL);
            //         break;
            // }
            // //in msec
            // _cam.SetExposureTime(task.Exp * 1000);
            // _cam.ExposeFrame();
            
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
            // in msec
            _cam.SetExposureTime(task.Exp * 1000);
            _cam.ExposeFrame();
            });
        }
        catch (Exception e)
        {
            Logger.AddError("start exp", e, this);
            return false;
        }

        return true;
    }
    public void UpdateUi()
    {
        UiBlock?.Update(CcdTemp, BaseTemp, CoolerPwr, Status, RemTime, ModelName, SerialNumber, Filter);
    }
    
    public void UpdatePreview()
    {
        UiBlock?.UpdatePreview(LatestImageBitmap);
    }

    public void UpdateProgressBar()
    {
        UiBlock?.UpdateProgressBar(ExpStartDt);
    }
}