using System;
using System.Drawing;
using RPCC.Tasks;

namespace RPCC.Cams;

internal interface ICameraDevice
{
    int[] ImageArea { get; set; }
    // public int CamsDomain { get; set; }
    // id
    // internal int Handle { get; set; }
    internal string FileName { get; set; }
    internal string ModelName { get; set; }
    internal string SerialNumber { get; set; }
    internal double PixelSizeX { get; set; }
    internal double PixelSizeY { get; set; }
    internal string Filter { get; set; }

    // status
    internal double CcdTemp { get; set; }
    internal double BaseTemp { get; set; }
    internal double CoolerPwr { get; set; }
    internal string Status { get; set; }
    internal int RemTime { get; set; }

    // exposure
    // internal bool IsSelected { get; set; }
    internal bool IsExposing { get; set; }
    internal DateTime ExpStartDt { get; set; }
    internal double ExpStartJd { get; set; }

    // latest image
    internal string LatestImageFilename { get; set; }
    internal ushort[,] LatestImageData { get; set; }
    internal Bitmap LatestImageBitmap { get; set; }
    
    // bool GrabRow(ushort[] buff);
    RpccFits GetRpccFits();
    // bool Initialize();
    bool Close();
    bool GetCamStatusAlt();
    // bool SetFrameType(int frameType);
    // bool SetExposureTime(int exposureTime);
    bool SetBin(int vBin, int hBin);
    // bool SetImageArea(int xbin, int ybin);  
    // bool SetCameraReadoutMode(int mode);
    bool Exposure(ObservationTask task);
}