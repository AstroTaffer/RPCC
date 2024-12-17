using System;
using System.Drawing;
using System.Runtime.InteropServices;
using APOGEELib;
using RPCC.Tasks;
using RPCC.Utils;

namespace RPCC.Cams;


//https://github.com/pawelDylag/SarcusImaging/tree/master
internal class ApogeeCameraDevice : ICameraDevice
{

    private readonly ICamera2 _cam;
    public int[] ImageArea { get; set; }

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
    public bool IsExposing { get; set; }
    public DateTime ExpStartDt { get; set; }
    public double ExpStartJd { get; set; }
    public string LatestImageFilename { get; set; }
    public ushort[,] LatestImageData { get; set; }  
    public Bitmap LatestImageBitmap { get; set; }

    public ApogeeCameraDevice(int camIdOne)
    {
        _cam = new Camera2();
        
        try
        {
            _cam.Init(Apn_Interface.Apn_Interface_USB, camIdOne, 0, 0);
        }
        catch (Exception e)
        {
            Logger.AddError("connect to apogee camera", e, this);
            return;
        }

        ModelName = _cam.CameraModel;
        
        LatestImageData = null;
        LatestImageFilename = null;
        LatestImageBitmap = null;
        
        try
        {
            SerialNumber = _cam.CameraSerialNumber;
        
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
                Logger.AddLogEntry("WARNING Unable to identify apogee camera filter");
                Filter = StringHolder.Unknown;
            }
            
            PixelSizeX = _cam.PixelSizeX;
            PixelSizeY = _cam.PixelSizeY;

            _cam.DigitizationSpeed = 0; // 0 - 16 bit, 1 - 12 bit
            _cam.CoolerEnable = true;
            _cam.PreFlashEnable = true;
            _cam.CoolerSetPoint = Settings.CamTemp;
        }
        catch (Exception e)
        {
            Logger.AddError("initialize apogee cam", e, this);
            SerialNumber = StringHolder.Error;
            Filter = StringHolder.Error;
            return;
        }
        
        Logger.AddLogEntry($"Connect camera: " +
                           $"Model {ModelName} | Serial Number {SerialNumber} | " +
                           $"Filter {Filter} | PixelSizeX {PixelSizeX} | PixelSizeY {PixelSizeY}");

    }

    public RpccFits GetRpccFits()
    {
        var imageWidth = _cam.ImagingColumns;
        var imageHeight = _cam.ImagingRows;
        try
        {   
            var buff = GetImageToMemory(imageWidth, imageHeight);
            return new RpccFits{Data = buff};
        }
        catch (Exception e)
        {
            Logger.AddError("grab frame", e, this);
            return null;
        }
    }
    
    /// <summary>
    /// Gets image from camera by passing ptr to allocated memory.
    /// </summary>
    /// <param name="width"></param>
    /// <param name="height"></param>
    /// <returns></returns>
    private ushort[,] GetImageToMemory(long width, long height)
    {
        // Allocating array of image size (width * height)
        // where pixel is size of unsigned short (2 BYTES)
        // possible values: 0 to 65535
        ushort[,] buff = new ushort[width, height];
        // Gets pointer to allocated array and fixes it, 
        // so that it won't be moved by Garbage Collector

        // 32-bit platform -> int
        // 64-bit platform -> long
        GCHandle buffGch = GCHandle.Alloc(buff, GCHandleType.Pinned);
        IntPtr buffPtr = buffGch.AddrOfPinnedObject();
    
        try
        {
            _cam.GetImage(buffPtr.ToInt64());
        }
        finally
        {
            buffGch.Free();
        }
        if (buff.Length * sizeof(ushort) != buff.Length)
            throw new InvalidOperationException("bytesgrabbed != sizeof(buff)");

        return buff;
    }

    public bool Close()
    {
        try
        {
            _cam.StopExposure(false);
            _cam.Close();
        }
        catch (Exception e)
        {
            Logger.AddError("apogee close", e, this);
            return false;
        }
        return true;
    }

    public bool GetCamStatusAlt()
    {
        try
        {
            CcdTemp = _cam.TempCCD;
            BaseTemp = _cam.TempHeatsink;
            CoolerPwr = _cam.CoolerDrive;
            
            var cum = _cam.ImagingStatus;
            switch (cum)
            {
                case Apn_Status.Apn_Status_Exposing:
                    Status = StringHolder.Exposing;
                    break;
                case Apn_Status.Apn_Status_Flushing:
                case Apn_Status.Apn_Status_Idle:
                case Apn_Status.Apn_Status_ImageReady:
                    Status = StringHolder.Idle;
                    break;
                case Apn_Status.Apn_Status_DataError:
                case Apn_Status.Apn_Status_ConnectionError:
                case Apn_Status.Apn_Status_PatternError:
                    Status = StringHolder.Error;
                    break;
                case Apn_Status.Apn_Status_WaitingOnTrigger:
                    Status = StringHolder.Wft;
                    break;
                case Apn_Status.Apn_Status_ImagingActive:
                    Status = StringHolder.Reading;
                    break;
                default:
                    Status = StringHolder.Unknown;
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
    //     throw new NotImplementedException();
    // }
    //
    // public bool SetExposureTime(int exposureTime)
    // {
    //     throw new NotImplementedException();
    // }

    public bool SetBin(int vBin, int hBin)
    {
        try
        {
            _cam.RoiBinningH = hBin;
            _cam.RoiBinningV = vBin;
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
        ImageArea[0] = 0;
        ImageArea[1] = 0;
        try
        {
            ImageArea[2] = _cam.ImagingColumns;
            ImageArea[3] = _cam.ImagingRows;
        }
        catch (Exception e)
        {
            ImageArea[2] = 1024;
            ImageArea[3] = 1024;
            Logger.AddError("get visible area", e, this);
        }
        
        ImageArea[2] = ImageArea[0] + (ImageArea[2] - ImageArea[0]) / xbin;
        ImageArea[3] = ImageArea[1] + (ImageArea[3] - ImageArea[1]) / ybin;
        
        try
        {
            _cam.RoiPixelsH = ImageArea[3];
            _cam.RoiPixelsV = ImageArea[2];
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
    //     //У апоги нет такой приколюхи
    //
    //     return true;
    // }
    
    public bool Exposure(ObservationTask task)
    {   
        try
        {
            SetImageArea(task.Xbin, task.Ybin);
            // in sec
            // Frame type (shutter control)
            switch (task.FrameType)
            {
                case StringHolder.Dark:
                    _cam.Expose(task.Exp, false);
                    break;
                default:
                    _cam.Expose(task.Exp, true);
                    break;
            }
        }
        catch (Exception e)
        {
            Logger.AddError("start exp", e, this);
            return false;
        }

        return true;
    }
}