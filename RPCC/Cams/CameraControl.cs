using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using APOGEELib;
using ASCOM.Tools;
using RPCC.Comms;
using RPCC.Focus;
using RPCC.Tasks;
using RPCC.Utils;
using Timer = System.Timers.Timer;

namespace RPCC.Cams;

internal static class CameraControl
{
    private static readonly object CamsLocker = new();

    private static readonly Timer CamsTimer = new(1000);
    private static readonly List<Task> ReadyImagesProcessList = [];
    internal static ResetUi resetUi;
    internal static ResetPics resetPics;

    internal static List<ICameraDevice> cams = [];

    internal static bool isConnected;
    private static bool _isCallbackRequired;
    private static int _readyCamNum;
    private static readonly string[] FilterOrder = [StringHolder.FilG, StringHolder.FilV, StringHolder.FilR, StringHolder.FilI];
    internal static ObservationTask loadedTask;
    internal delegate void ResetUi();

    internal delegate void ResetPics(ICameraDevice camera);


    #region Connect & Disconnect

    internal static bool ReconnectCameras()
    {
        var isAllGood = true;

        if (isConnected) isAllGood = DisconnectCameras();

        lock (CamsLocker)
        {
            // connect flis
            Fli.DeviceName[] fliCamerasNames = null;
            try
            {
                fliCamerasNames = Fli.List(Fli.DOMAIN.CAMERA | Fli.DOMAIN.USB);
            }
            catch (Exception e)
            {
                Logger.AddLogEntry($"ERROR WHILE search fli cams, {e.Message}");
            }

            if (fliCamerasNames is null) return false;
            foreach (var name in fliCamerasNames) cams.Add(new FliCameraDevice(name));

            if (cams.Count < 3)
            {
                // connect apogees
                ICamDiscover discover = new CamDiscover(); 
                discover.DlgCheckUsb = true;
                var cums = discover.ListUsbDevices;
                for (var i = 0; i < 3-cams.Count; i++)
                {
                    if (!cums.Contains(i.ToString())) break;
                    cams.Add(new ApogeeCameraDevice(i));
                }
            }

            // if (cams.Count < 3)
            // {
            //     // connect chin chan chons
            // }

            Logger.AddLogEntry($"{cams.Count} cameras found");
            // Sort cameras by ascending filters' wavelengths

            cams = cams.OrderBy(cam =>
            {
                var index = Array.IndexOf(FilterOrder, cam.Filter);
                // Console.WriteLine($"Sorting! Filer {cam.filter} => Index {index}");
                return index == -1 ? int.MaxValue : index;
            }).ToList();

            if (cams.Count <= 0) return false;
            CamsTimer.Elapsed += CamsTimerTickAlt;
            CamsTimer.Start();
            resetUi();
            isConnected = true;
            return isAllGood;
        }
    }


    internal static bool DisconnectCameras()
    {
        var isAllGood = true;

        lock (CamsLocker)
        {
            CamsTimer.Stop();
            CamsTimer.Elapsed -= CamsTimerTickAlt;
            isConnected = false;
            foreach (var cam in cams) isAllGood &= cam.Close();
            cams = [];
            resetUi();
        }

        return isAllGood;
    }

    #endregion

    #region Status

    // To use these functions you must implement isExposing bool flag in each CameraDevice
    // and _isCallbackRequired bool flag in CameraControl
    private static void CamsTimerTickAlt(object sender, ElapsedEventArgs e)
    {
        CamsTimer.Stop();
        lock (CamsLocker)
        {
            GetCamsStatusAlt();
            var allReady = true;
            foreach (var unused in cams.Where(cam => cam.Status == StringHolder.Exposing))
                allReady = false;

            if (allReady)
            {
                _isCallbackRequired = false;
                _readyCamNum = 0;
                foreach (var cam in cams)
                    switch (cam.Status)
                    {
                        case StringHolder.Idle:
                        {
                            _readyCamNum++;
                            if (cam.IsExposing)
                            {
                                // Image ready
                                ReadyImagesProcessList.Add(Task.Run(() => ProcessCapturedImage(cam)));
                                cam.IsExposing = false;
                                _isCallbackRequired = true;
                            }
                            break;
                        }
                        case StringHolder.Error:
                            cam.Close();
                            cams.Remove(cam);
                            continue;
                    }

                if (ReadyImagesProcessList.Count > 0)
                {
                    Task.WaitAll(ReadyImagesProcessList.ToArray());

                    if (_isCallbackRequired && _readyCamNum == cams.Count)
                    {
                        switch (Head.currentTask.FrameType)
                        {
                            case "Focus":
                                CameraFocus.CamFocusCallback();
                                break;
                            default:
                                Head.CamCallback();
                                break;
                        }

                        ReadyImagesProcessList.Clear();
                        foreach (var cam in cams)
                        {
                            if (!string.IsNullOrEmpty(cam.LatestImageFilename))
                            {
                                ReadyImagesProcessList.Add(Task.Run(() => ConstructBitmap(cam)));
                            }
                            else
                            {
                                cam.Close();
                                cams.Remove(cam);
                            }
                        }
                       
                        Task.WaitAll(ReadyImagesProcessList.ToArray());
                    }

                    ReadyImagesProcessList.Clear();
                }
            }
        }

        if (isConnected) CamsTimer.Start();
    }

    private static void GetCamsStatusAlt()
    {
        foreach (var t in cams)
        {
            if (t.GetCamStatusAlt()) continue;
            Thread.Sleep(5000);
            if (ReconnectCameras()) continue;
            Logger.AddLogEntry("ERROR cam can't reconect, stop cam timer");
            CamsTimer.Stop();
            Logger.SaveLogs();
        }
    }

    #endregion

    #region Expose Frames

    // internal static bool PrepareToObs(ObservationTask task, bool isCheck = false)
    // {
    //     
    //
    //
    //     return isAllGood;
    // }

    internal static bool StartExposure(ObservationTask task)
    {

        loadedTask = task;

        // var selCamsNum = 0;
        // var selFilters = Head.currentTask.Filters.Split(' ');
        //
        // lock (CamsLocker)
        // {
        //     foreach (var t in cams)
        //     {
                // if (!Array.Exists(selFilters, element => element == t.Filter))
                // {
                //     t.IsSelected = false;
                //     continue;
                // }
                //
                // t.IsSelected = true;
                // selCamsNum++;

                // // VBin, HBin and corresponding VisibleArea
                // if (cams[i].SetBin(task.Xbin, task.Ybin))
                // {
                //     Logger.AddLogEntry($"WARNING Unable to set camera {i + 1} bin");
                //     isAllGood = false;
                // } TODO put in settings
            // }

            // if (selCamsNum == 0)
            // {
            //     Logger.AddLogEntry($"WARNING No cameras with specified filters {Head.currentTask.Filters}");
            //     return false;
            // }
        // }
        
        lock (CamsLocker)
        {
            var dt = DateTime.UtcNow;
            var jd = AstroUtilities.JulianDateFromDateTime(dt);
            CoordinatesManager.CalculateObjectDistance2Moon(loadedTask);
            CoordinatesManager.MoonIllumination = AstroUtilities.MoonIllumination(jd);
            for (var i = 0; i < cams.Count; i++)
            {
                if (cams[i].Status is not StringHolder.Idle) continue;
                if (!cams[i].Exposure(loadedTask))
                {
                    Logger.AddLogEntry($"WARNING Unable to start camera {i + 1} exposure");
                    return false;
                }
                cams[i].IsExposing = true; // Use with CamsTimerTickAlt
                Logger.AddLogEntry($"Exposing {cams[i].Filter}");
                cams[i].ExpStartDt = dt;
                cams[i].ExpStartJd = jd;
            }
            //if (cams.Length > 0) _isExposing = true;  // Use with CamsTimerTick
        }

        return true;
    }

    #endregion

    #region Read Frames

    private static void ProcessCapturedImage(ICameraDevice cam)
    {
        var latestImage = ReadImage(cam);
        if (latestImage is null)
        {
            cam.LatestImageData = null;
            cam.LatestImageFilename = null;
            return;
        }
        cam.LatestImageData = latestImage.Data;
        cam.LatestImageFilename = latestImage.SaveFitsFile(cam);
    }

    private static RpccFits ReadImage(ICameraDevice cam)
    {
        Logger.AddDebugLogEntry($"Start read image from {cam.Filter}");

        var imageFits = cam.GetRpccFits();
        if (imageFits is null)
        {
            cam.Close();
            cams.Remove(cam);
            return null;
        }


        // Mirror image
        switch (cam.Filter)
        {
            case StringHolder.FilG:
                if (MountDataCollector.IsLookingEast) imageFits.Data = Rotate(imageFits.Data);
                break;
            case StringHolder.FilR:
                if (!MountDataCollector.IsLookingEast) imageFits.Data = Rotate(imageFits.Data);
                break;
            case StringHolder.FilI:
                imageFits.Data = MountDataCollector.IsLookingEast ? FlipV(imageFits.Data) : FlipH(imageFits.Data);
                break;
        }

        return imageFits;
    }

    private static void ConstructBitmap(ICameraDevice cam)
    {
        var stat = new GeneralImageStat();
        stat.Calculate(cam.LatestImageData);

        var h = cam.LatestImageData.GetLength(0);
        var v = cam.LatestImageData.GetLength(1);

        cam.LatestImageBitmap = new Bitmap(h, v,
            PixelFormat.Format24bppRgb);

        int pixelColor;
        for (ushort i = 0; i < h; i++)
        for (ushort j = 0; j < v; j++)
        {
            pixelColor = (int) ((cam.LatestImageData[i, j] - stat.DnrStart) * stat.DnrColorScale);
            if (pixelColor < 0) pixelColor = 0;
            if (pixelColor > 255) pixelColor = 255;
            cam.LatestImageBitmap.SetPixel(j, i, Color.FromArgb(pixelColor, pixelColor, pixelColor));
        }

        resetPics(cam);
    }

    #endregion

    #region Matrix rotate and flip

    private static ushort[,] Rotate(ushort[,] matrix)
    {
        // (I2, I1) = (I1, I2);     
        var xUp = matrix.GetUpperBound(0);
        for (var x = 0; x <= xUp; x++)
        {
            var yUp = matrix.GetUpperBound(1);
            for (var y = 0; y <= yUp / 2; y++)
            {
                var newX = xUp - x;
                var newY = yUp - y;
                if ((newX == x) & (newY == y)) continue;
                (matrix[x, y], matrix[newX, newY]) =
                    (matrix[newX, newY], matrix[x, y]);
            }
        }

        return matrix;
    }

    private static ushort[,] FlipV(ushort[,] matrix)
    {
        var yUp = matrix.GetUpperBound(1);
        for (var y = 0; y <= yUp; y++)
        {
            var xUp = matrix.GetUpperBound(0);
            for (var x = 0; x <= xUp / 2; x++)
            {
                var newX = xUp - x;
                if (newX == x) continue;
                (matrix[x, y], matrix[newX, y]) =
                    (matrix[newX, y], matrix[x, y]);
            }
        }

        return matrix;
    }

    private static ushort[,] FlipH(ushort[,] matrix)
    {
        var xUp = matrix.GetUpperBound(0);
        for (var x = 0; x <= xUp; x++)
        {
            var yUp = matrix.GetUpperBound(1);
            for (var y = 0; y <= yUp / 2; y++)
            {
                var newY = yUp - y;
                if (newY == y) continue;
                (matrix[x, y], matrix[x, newY]) =
                    (matrix[x, newY], matrix[x, y]);
            }
        }

        return matrix;
    }

    #endregion
}