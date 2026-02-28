using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using System.Xml.Linq;
using APOGEELib;
using ASCOM.Tools;
using RPCC.Focus;
using RPCC.Tasks;
using RPCC.Utils;
using Timer = System.Timers.Timer;

namespace RPCC.Cams;

internal static class CameraControl
{
    private static readonly object CamsLocker = new();

    private static readonly Timer CamsTimer = new(1000);
    // private static readonly List<Task> ReadyImagesProcessList = [];
    internal static List<ICameraDevice> cams = [];

    internal static bool isConnected;
    private static bool _isCallbackRequired;
    private static int _readyCamNum;
    private static readonly string[] FilterOrder = [StringHolder.FilG, StringHolder.FilV, StringHolder.FilR, StringHolder.FilI];
    internal static ObservationTask loadedTask;
    // Guards against re-entrancy of the timer tick (including slow ticks / overlapping invocations).
    private static int _camsTickRunning = 0;

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
                Logger.AddError("search fli cams", e);
            }

            if (fliCamerasNames is null) return false;
            foreach (var name in fliCamerasNames) cams.Add(new FliCameraDevice(name));

            if (cams.Count < 3)
            {
                // connect apogees
                ICamDiscover discover = new CamDiscover(); 
                discover.DlgCheckUsb = true;
                var cums = "";
                try
                {
                    cums = discover.ListUsbDevices;  // какая-то внешняя ошибка Apogee
                }catch (Exception e)
                {
                    Logger.AddLogEntry($"CAN'T CONNECT TO APOGEE CAMERA: {e}");
                }
                
                if (!string.IsNullOrEmpty(cums))
                {
                    var cum = XDocument.Parse(cums);
                    foreach (var c in cum.Elements("d"))
                    {
                        if (string.IsNullOrEmpty(c.Value)) continue;
                        var fuckingbullshit = c.Value.Split(',')[0].Split('=')[1];
                        var i = int.Parse(fuckingbullshit);
                        cams.Add(new ApogeeCameraDevice(i));
                    }
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
            foreach (var cam in cams)
            {
                switch (cam.Filter)
                {
                    case StringHolder.FilG:
                    case StringHolder.FilV:
                        cam.UiBlock = new CameraUiBlock
                        {
                            GroupBoxCam = MainForm.Instance.groupBoxCam1,
                            LabelCcdTemp = MainForm.Instance.labelCam1CcdTemp,
                            LabelBaseTemp = MainForm.Instance.labelCam1BaseTemp,
                            LabelCoolerPwr = MainForm.Instance.labelCam1CoolerPwr,
                            LabelStatus = MainForm.Instance.labelCam1Status,
                            LabelRemTime = MainForm.Instance.labelCam1RemTime,
                            
                            PictureBoxPreview = MainForm.Instance.pictureBoxImage1,
                            ExposureProgressBar = MainForm.Instance.progressBarG,
                            LabelFilter = MainForm.Instance.labelCam1Filter,
                            LabelModel = MainForm.Instance.labelCam1Model,
                            LabelSerial = MainForm.Instance.labelCam1Sn
                        };
                        break;

                    case StringHolder.FilR:
                        cam.UiBlock = new CameraUiBlock
                        {
                            GroupBoxCam = MainForm.Instance.groupBoxCam2,
                            LabelCcdTemp = MainForm.Instance.labelCam2CcdTemp,
                            LabelBaseTemp = MainForm.Instance.labelCam2BaseTemp,
                            LabelCoolerPwr = MainForm.Instance.labelCam2CoolerPwr,
                            LabelStatus = MainForm.Instance.labelCam2Status,
                            LabelRemTime = MainForm.Instance.labelCam2RemTime,
                            
                            PictureBoxPreview = MainForm.Instance.pictureBoxImage2,
                            ExposureProgressBar = MainForm.Instance.progressBarR,
                            LabelFilter = MainForm.Instance.labelCam2Filter,
                            LabelModel = MainForm.Instance.labelCam2Model,
                            LabelSerial = MainForm.Instance.labelCam2Sn
                        };
                        break;

                    case StringHolder.FilI:
                        cam.UiBlock = new CameraUiBlock
                        {
                            GroupBoxCam = MainForm.Instance.groupBoxCam3,
                            LabelCcdTemp = MainForm.Instance.labelCam3CcdTemp,
                            LabelBaseTemp = MainForm.Instance.labelCam3BaseTemp,
                            LabelCoolerPwr = MainForm.Instance.labelCam3CoolerPwr,
                            LabelStatus = MainForm.Instance.labelCam3Status,
                            LabelRemTime = MainForm.Instance.labelCam3RemTime,
                            
                            PictureBoxPreview = MainForm.Instance.pictureBoxImage3,
                            ExposureProgressBar = MainForm.Instance.progressBarI,
                            LabelFilter = MainForm.Instance.labelCam3Filter,
                            LabelModel = MainForm.Instance.labelCam3Model,
                            LabelSerial = MainForm.Instance.labelCam3Sn
                        };
                        break;
                }
            }

            GetCamsStatusAlt();
            CamsTimer.Elapsed += CamsTimerTickAlt;
            CamsTimer.Start();
            // resetUi();
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
            foreach (var cam in cams)
                isAllGood &= cam.Close();

            cams = [];
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
        
        // Prevent overlapping ticks (System.Timers.Timer can re-enter if handler runs long).
        if (Interlocked.Exchange(ref _camsTickRunning, 1) == 1)
        {
            if (isConnected) CamsTimer.Start();
            return;
        }

        try
        {
            var processTasks = new List<Task>(capacity: 8);
            var bitmapTasks = new List<Task>(capacity: 8);

            bool allReady;
            bool callbackRequired;
            int readyCamNum;
            int camsCountSnapshot;

            // 1) Under lock: update statuses, decide what to do, and mutate shared state quickly.
            lock (CamsLocker)
            {
                GetCamsStatusAlt();

                if (cams.Count <= 0)
                {
                    DisconnectCameras();
                    return;
                }

                // Fast readiness check (no LINQ allocations; also clearer).
                allReady = true;
                for (var i = 0; i < cams.Count; i++)
                {
                    if (cams[i].Status == StringHolder.Exposing) { allReady = false; break; }
                }

                if (!allReady) return;

                callbackRequired = false;
                readyCamNum = 0;

                // IMPORTANT: do not modify 'cams' inside foreach; iterate backwards so removal is safe.
                for (var i = cams.Count - 1; i >= 0; i--)
                {
                    var cam = cams[i];
                    switch (cam.Status)
                    {
                        case StringHolder.Idle:
                        {
                            readyCamNum++;
                            if (cam.IsExposing)
                            {
                                // Image ready: mark state under lock, heavy work outside lock.
                                cam.IsExposing = false;
                                callbackRequired = true;
                                processTasks.Add(Task.Run(() => ProcessCapturedImage(cam)));
                            }
                            break;
                        }
                        case StringHolder.Error:
                        {
                            cam.Close();
                            cams.RemoveAt(i);
                            break;
                        }
                    }
                }

                if (cams.Count <= 0)
                {
                    DisconnectCameras();
                    return;
                }

                camsCountSnapshot = cams.Count;
            }

            // 2) Heavy IO/CPU outside the camera lock.
            if (processTasks.Count > 0) Task.WaitAll(processTasks.ToArray());

            // 3) Callback + preview generation can touch other subsystems; never do it under CamsLocker.
            if (callbackRequired && readyCamNum == camsCountSnapshot)
            {
                // StatusUpdater.RunPreviewGenerator();
                StatusUpdater.RunPreviewGeneratorAsync();
                if (CameraFocus.IsFocusing) CameraFocus.CamFocusCallback();
                else Head.CamCallback();

                // 4) Decide which bitmaps to build; remove broken cams safely under lock.
                lock (CamsLocker)
                {
                    for (var i = cams.Count - 1; i >= 0; i--)
                    {
                        var cam = cams[i];
                        if (!string.IsNullOrEmpty(cam.LatestImageFilename))
                        {
                            bitmapTasks.Add(Task.Run(() => ConstructBitmap(cam)));
                        }
                        else
                        {
                            cam.Close();
                            cams.RemoveAt(i);
                        }
                    }
                }

                if (bitmapTasks.Count > 0) Task.WaitAll(bitmapTasks.ToArray());
            }
        }
        finally
        {
            Interlocked.Exchange(ref _camsTickRunning, 0);
            if (isConnected) CamsTimer.Start();
        }
    }

    private static void GetCamsStatusAlt()
    {
        foreach (var t in cams)
        {
            if (t.GetCamStatusAlt()) continue;
            // Thread.Sleep(5000);
            // if (ReconnectCameras()) continue;
            Logger.AddLogEntry("Stop cam timer");
            CamsTimer.Stop();
            Logger.SaveLogs();
        }
    }

    #endregion

    #region Expose Frames

    internal static bool StartExposure(ObservationTask task)
    {

        loadedTask = task;
        
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
        latestImage.SaveFitsFile(cam);
    }

    private static RpccFits ReadImage(ICameraDevice cam)
    {
        Logger.AddDebugLogEntry($"Start read image from {cam.Filter}");

        var imageFits = cam.GetRpccFits();
        if (imageFits is null)
        {
            Logger.AddLogEntry($"ERROR while read image, return null and close cam {cam.Filter}");
            cam.Close();
            cams.Remove(cam);
            return null;
        }
        Logger.AddDebugLogEntry($"End reading image from {cam.Filter}");
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
        cam.UpdatePreview();
    }

    #endregion
    //
    // #region Matrix rotate and flip
    //
    // private static ushort[,] Rotate(ushort[,] matrix)
    // {
    //     // (I2, I1) = (I1, I2);     
    //     var xUp = matrix.GetUpperBound(0);
    //     for (var x = 0; x <= xUp; x++)
    //     {
    //         var yUp = matrix.GetUpperBound(1);
    //         for (var y = 0; y <= yUp / 2; y++)
    //         {
    //             var newX = xUp - x;
    //             var newY = yUp - y;
    //             if ((newX == x) & (newY == y)) continue;
    //             (matrix[x, y], matrix[newX, newY]) =
    //                 (matrix[newX, newY], matrix[x, y]);
    //         }
    //     }
    //
    //     return matrix;
    // }
    //
    // private static ushort[,] FlipV(ushort[,] matrix)
    // {
    //     var yUp = matrix.GetUpperBound(1);
    //     for (var y = 0; y <= yUp; y++)
    //     {
    //         var xUp = matrix.GetUpperBound(0);
    //         for (var x = 0; x <= xUp / 2; x++)
    //         {
    //             var newX = xUp - x;
    //             if (newX == x) continue;
    //             (matrix[x, y], matrix[newX, y]) =
    //                 (matrix[newX, y], matrix[x, y]);
    //         }
    //     }
    //
    //     return matrix;
    // }
    //
    // private static ushort[,] FlipH(ushort[,] matrix)
    // {
    //     var xUp = matrix.GetUpperBound(0);
    //     for (var x = 0; x <= xUp; x++)
    //     {
    //         var yUp = matrix.GetUpperBound(1);
    //         for (var y = 0; y <= yUp / 2; y++)
    //         {
    //             var newY = yUp - y;
    //             if (newY == y) continue;
    //             (matrix[x, y], matrix[x, newY]) =
    //                 (matrix[x, newY], matrix[x, y]);
    //         }
    //     }
    //
    //     return matrix;
    // }
    //
    // #endregion
}