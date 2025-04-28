using System;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading;
using System.Timers;
using ASCOM.Tools;
using nom.tam.fits;
using RPCC.Cams;
using RPCC.Comms;
using RPCC.Focus;
using RPCC.Utils;
using Timer = System.Timers.Timer;

namespace RPCC.Tasks;

public static class Head
{
    private const short TotalMinutes2StartTask = 5; //TODO add in .cfg
    private const short FlatExp = 2; // in s  TODO add in .cfg
    private const short FlatDarkQuantity = 10; //TODO add in .cfg
    private static readonly short[] DarkExps = {2, 5, 10, 15, 20, 30, 50, 80, 120, 180}; //TODO add in .cfg
    private const double PulseGuideVelocityRa = 6; //sec in sec TODO add in .cfg
    private const double PulseGuideVelocityDec = 2; //sec in sec TODO add in .cfg
    private static double _kPA = 1.2; // //TODO add in .cfg
    private static double _kIA = 0.00005; //0.28 //TODO add in .cfg
    private static double _kDA = 25; //2.9 //TODO add in .cfg
    private static double _kPD = 1; // //TODO add in .cfg
    private static double _kID = 0.001; //0.28 //TODO add in .cfg
    private static double _kDD = 10; //2.9 //TODO add in .cfg
    
    public static readonly Timer ThinkingTimer = new();
    public static ObservationTask CurrentTask;
    public static bool IsObserve;
    public static bool IsDoFlats;
    public static bool IsDoDarks;
    public static bool IsOnPause;
    private static string _firstFrame;
    private static bool _firstFrameLookingEast;
    private static double _cd11 = 100;
    private static double _cd12 = 100;
    public static bool IsThinking;

    public static bool IsGuid = true;
    private static int _troubles;
    private static double _idec;
    private static double _ira;
    private static double _oldErrDec;
    private static double _oldErrRa;
    
    public static void StartThinking()
    {
        ThinkingTimer.Elapsed += Thinking;
        ThinkingTimer.Interval = 5000;
    }

    private static void Thinking(object sender, ElapsedEventArgs e)
    {
        ThinkingTimer.Stop();
        if (CurrentTask != null)
        {
            CurrentTask = DbCommunicate.GetTaskFromDb(CurrentTask.TaskNumber);
            // DbCommunicate.UpdateTaskFromDb(ref CurrentTask);
        }
        DbCommunicate.LoadDbTable();
        if (!CameraControl.isConnected)
        {
            if (!CameraControl.ReconnectCameras())
            {
                Logger.AddLogEntry("WARNING: Can't thinking, no connection to cameras");
                IsThinking = false;
                return;
            }
        }

        if (!SiTechExeSocket.IsConnected)
        {
            if (!SiTechExeSocket.Connect().Result)
            {
                Logger.AddLogEntry("WARNING: Can't thinking, no connection to SiTechExeSocket");
                IsThinking = false;
                return;
            }
        }

        if (MountDataCollector.IsInBlinky) 
        {
            _troubles += 1;
            SiTechExeSocket.MotorsToAuto();
        }
        else _troubles = 0;

        if (_troubles > 2)
        {
            if (CurrentTask is not null)
            {
                Logger.AddLogEntry("WARNING: Troubles with motors, stop task");
                IsThinking = false;
                EndTask(5);
            }
        }

        if (IsOnPause & CurrentTask is not null)
        {
            if (IsObserve & WeatherDataCollector.Obs |
                IsDoDarks & !WeatherDataCollector.Obs |
                IsDoFlats & WeatherDataCollector.Flat)
            {
                Logger.AddLogEntry($"Head: Unpause task #{CurrentTask.TaskNumber}");
                if (UnparkAndGoTo())
                {
                    IsOnPause = false;
                    StartExpAndCheckFuckup();
                }
            }
        }

        if (string.IsNullOrEmpty(_firstFrame) & CurrentTask is not null & IsObserve)
        {
            if (CurrentTask.RepointTimes is null || CurrentTask.RepointTimes?.Count == 0){
                // if (currentTask.RepointTimes.Count == 0) //Корректировать положение монтировки
                //                                          //можно только тогда, когда не надо делать репоинты 
                // {
                    _firstFrame = DbCommunicate.GetPath2FirstAssFrame(CurrentTask.TaskNumber);
                    _firstFrameLookingEast = MountDataCollector.IsLookingEast;
                    if(!string.IsNullOrEmpty(_firstFrame))
                        Logger.AddDebugLogEntry($"Set first frame {_firstFrame}. lookingEast = {_firstFrameLookingEast}");
                // } 
            }
        }

        if (!string.IsNullOrEmpty(_firstFrame) & _cd11 > 99)
        {
            // var cal = _firstFrame.Replace("RAW", "CALIBRATED");
            if (File.Exists(_firstFrame))
            {
                try
                {
                    //Если матрицы еще нет, ориентируемся на первый кадр, если есть, ловим решенный кадр посвежее
                    var fits = new Fits(_firstFrame);
                    var hdu = (ImageHDU)fits.GetHDU(0);
    
                    _cd11 = hdu.Header.GetDoubleValue("CD1_1");
                    _cd12 = hdu.Header.GetDoubleValue("CD1_2");
                    if (_cd11 + _cd12 == 0)
                    {
                        _cd11 = 100;
                        _cd12 = 100;
                    }
                    else
                    {
                        Logger.AddLogEntry($"Head: new CD11, CD12 = [{_cd11}, {_cd12}],\n" +
                                           $"kP_alpha = {_kPA}, kI_alpha = {_kIA}, kD_alpha = {_kDD},\n" +
                                           $"kP_alpha = {_kPA}, kI_alpha = {_kIA}, kD_alpha = {_kDD}");
                    }
                    fits.Close();
                }
                catch
                {
                        
                }
            }
        }

        var tab = DbCommunicate.GetTableForThinking().Rows;

        foreach (DataRow row in tab)
        {
            var bufTask = DbCommunicate.GetTaskFromDb(Convert.ToInt32(row["task_id"]));
            // если время конца таски уже прошло, но она все еще не начата, или тип кадра не указан
            // то ставим статус пролюблена
            if (bufTask.TimeEnd < DateTime.UtcNow)
            {
                if ((bufTask.FrameType == StringHolder.Light) | 
                    ((bufTask.FrameType == StringHolder.Dark | 
                      bufTask.FrameType == StringHolder.Flat |
                      bufTask.FrameType == StringHolder.Test | 
                      bufTask.FrameType == StringHolder.Focus) & 
                     (DateTime.UtcNow - bufTask.TimeEnd).TotalMinutes > 1))
                {
                    switch (bufTask.Status)
                    {
                        case 0:
                        { // Если не отнаблюдали ни одного кадра, значит пронаблюдать не удалось
                            bufTask.Status = 4;
                            DbCommunicate.UpdateTaskInDb(bufTask);
                            break;
                        }
                        case 1:
                        { //Если успели что-то отснять, то либо не успели закончить, либо отсняли, что хотели
                            bufTask.Status = bufTask.DoneFrames < bufTask.AllFrames ? (short)5 : (short)2;
                            DbCommunicate.UpdateTaskInDb(bufTask);
                            break;
                        }
                    }
                }
            }
            else
            {
                switch (bufTask.Status)
                {
                    case 1:
                    {
                        if (CurrentTask is null)
                        {
                            switch (bufTask.FrameType)
                            {
                                case StringHolder.Flat:
                                    if (WeatherDataCollector.Flat)
                                    {
                                        CurrentTask = bufTask;
                                    }
                                    break;
                                case StringHolder.Light:
                                    if (WeatherDataCollector.Obs)
                                    {
                                        CurrentTask = bufTask;
                                    }
                                    break;
                                case StringHolder.Test:
                                    CurrentTask = bufTask;
                                    break;
                                case StringHolder.Dark:
                                    if (!WeatherDataCollector.Flat & !WeatherDataCollector.Obs)
                                    {
                                        CurrentTask = bufTask;
                                    }
                                    break;
                            }
                        }
                        break;
                    }
                }
            }
                
            if (bufTask.Status > 0) continue; // если не ждет наблюдения, то идем дальше
            if (IsObserve || IsDoDarks || IsDoFlats) continue; // если уже идет задание, то ждем минуту
            if (CurrentTask is null)
            {
                switch (bufTask.FrameType)
                {
                    case StringHolder.Flat:
                        if (WeatherDataCollector.Flat)
                        {
                            CurrentTask = bufTask;
                        }
                        break;
                    case StringHolder.Light:
                        if (WeatherDataCollector.Obs)
                        {
                            CurrentTask = bufTask;
                        }
                        break;
                    case StringHolder.Test:
                        CurrentTask = bufTask;
                        break;
                    case StringHolder.Dark:
                        if (!WeatherDataCollector.Flat & !WeatherDataCollector.Obs)
                        {
                            CurrentTask = bufTask;
                        }
                        break;
                }
            }
        }
            
        //а если нашлась и время до начала менее 5 минут, то стартуем 
        if (CurrentTask is null)
        {
            if (CameraControl.isConnected & !IsObserve & !IsDoDarks & !IsDoFlats)
            {
                if (WeatherDataCollector.Flat)
                {
                    if (DbCommunicate.CanDoDarkFlat(false, FlatExp))
                    {
                        PrepareAndStartDoFlats();  
                    }
                }
                if (!WeatherDataCollector.Obs & !WeatherDataCollector.Flat)
                {
                    foreach (var exp in DarkExps)
                    {
                        if (!DbCommunicate.CanDoDarkFlat(true, exp)) continue;
                        PrepareAndStartDoDark(exp);
                        break;
                    }
                }
            }
        } else 
        {
            if (CurrentTask.Status > 1)
            {
                EndTask(CurrentTask.Status);
                ThinkingTimer.Start();
                return;
            }
            //Если у текущего задания
            //вышло время, но оно на паузе и
            //не вызовет коллбек,
            //то нужно его завершить иначе
            if (!IsObserve & !IsDoDarks & !IsDoFlats & (IsThinking | CurrentTask.FrameType == StringHolder.Test) &
                (CurrentTask.TimeStart - DateTime.UtcNow).TotalMinutes < TotalMinutes2StartTask &
                CameraControl.isConnected)
            {

                switch (CurrentTask.FrameType)
                {
                    case StringHolder.Light:
                    {
                        if (WeatherDataCollector.Obs)
                        {
                            StartDoLight();
                        }

                        break;
                    }
                    case StringHolder.Test:
                    {
                        StartDoTest();
                        break;
                    }
                    case StringHolder.Dark:
                    {
                        if (!WeatherDataCollector.Obs)
                        {
                            StartDoDark();
                        }

                        break;
                    }
                    case StringHolder.Flat:
                    {
                        if (WeatherDataCollector.Flat)
                        {
                            StartDoFlats();
                        }

                        break;
                    }
                }
            }
        }
        ThinkingTimer.Start();
    }

    private static void StartDoTest()
    {
        IsObserve = true;
        CurrentTask.Status = 1;
        Logger.AddLogEntry($"Start task #{CurrentTask.TaskNumber}, type: {CurrentTask.FrameType}");
        _cd11 = 100;   
        _cd12 = 100;
        _firstFrame = null;
        if (!UnparkAndGoTo()) return; //проверять доехал ли
        CurrentTask.Filters = CheckFil();
        StartExpAndCheckFuckup();
    }
    
    private static bool UnparkAndGoTo()
    {
        if (CoordinatesManager.CalculateObjectDistance2Mount(CurrentTask) < 10) return true;
        while (MountDataCollector.IsParking)
        {
            Logger.AddLogEntry("UnparkAndGoTo: mount is parking, sleep 5 sec");
            Thread.Sleep(5000);
        }
        if (MountDataCollector.IsParked)
        {
            Logger.AddLogEntry("UnparkAndGoTo: mount is parked, unparking");
            SiTechExeSocket.Unpark();
        }
        if (SiTechExeSocket.GoTo(CurrentTask.Ra, CurrentTask.Dec, true))
        {
            Thread.Sleep(3000);
            return true;
        }
        Logger.AddLogEntry($"WARNING: can't start task #{CurrentTask.TaskNumber}, error while GOTO");
        EndTask(4);
        return false;

    }

    private static void EndTask(short endStatus)
    {
        Logger.AddLogEntry($"End task №{CurrentTask.TaskNumber}, status {endStatus}");
        CurrentTask.Status = endStatus;
        if (!CameraFocus.IsFocusing & CurrentTask.TaskNumber > 0) 
        {
            DbCommunicate.UpdateTaskInDb(CurrentTask);
        }

        IsObserve = false;
        IsDoDarks = false;
        IsDoFlats = false;
        IsOnPause = false;
        CurrentTask = null;
        _firstFrame = null;
        _cd11 = 100;
        _cd12 = 100;
        if (!MountDataCollector.IsParked & !MountDataCollector.IsParking)
        {
            Logger.AddLogEntry("EndTask: mount is parking");
            SiTechExeSocket.Park();
        }
    }

    private static void StartDoLight()
    {   
        IsObserve = true;
        CurrentTask.Status = 1;
        Logger.AddLogEntry($"Start task #{CurrentTask.TaskNumber}, type: {CurrentTask.FrameType}");
        _cd11 = 100;   
        _cd12 = 100;
        _firstFrame = null;
        if (!UnparkAndGoTo()) return; //проверять доехал ли
        CurrentTask.Filters = CheckFil();
        StartExpAndCheckFuckup();
    }

    public static void CamCallback()
    {
        // DbCommunicate.UpdateTaskFromDb(ref CurrentTask);
        CurrentTask = DbCommunicate.GetTaskFromDb(CurrentTask.TaskNumber);
        GetDataFromFits fitsAnalysis = null;
        CurrentTask.DoneFrames++;
        CurrentTask.TimeLastExp = DateTime.UtcNow;
        DbCommunicate.UpdateTaskInDb(CurrentTask);
            
        if (CurrentTask.Status > 1)
        {
            EndTask(CurrentTask.Status);
            return;
        }

        switch (CurrentTask.FrameType)
        {
            case StringHolder.Light:
            {
                
                if (CurrentTask.TimeEnd > DateTime.UtcNow) //Если время задания еще не вышло
                {
                    foreach (var cam in CameraControl.cams) //перебираем камеры и выводим информацию
                                                            //о полученных кадрах
                    {
                        if (!string.IsNullOrEmpty(cam.LatestImageFilename))
                        {
                            
                            fitsAnalysis = new GetDataFromFits(cam); 
                            Logger.LogFrameInfo(fitsAnalysis, cam.Filter);
                        }
                    }

                    if (fitsAnalysis is null)
                    {
                        Logger.AddLogEntry("CamCallback: no data available, stop obs");
                        EndTask(5);
                        return;
                    }

                    if (CurrentTask.RepointTimes?.Count > 0)
                    {
                         //репоинт для объектов СС
                        Logger.AddDebugLogEntry($"HEAD: Next repoint at {CurrentTask.RepointTimes[0]} to {CurrentTask.RepointCoords[0]}");
                        if (CurrentTask.RepointTimes[0] < DateTime.UtcNow)
                        {   
                            Logger.AddDebugLogEntry($"HEAD: Repointing");
                            CurrentTask.ComputeRaDec(CurrentTask.RepointCoords[0]);
                            if (SiTechExeSocket.GoTo(CurrentTask.Ra, CurrentTask.Dec, true))
                            {
                                Logger.AddDebugLogEntry($"HEAD: Repointing done, remove point");
                                CurrentTask.RepointTimes.RemoveAt(0);
                                CurrentTask.RepointCoords.RemoveAt(0);
                                DbCommunicate.UpdateTaskInDb(CurrentTask);
                            }
                        }
                        
                    }
                    else Guiding();
                    if (!WeatherDataCollector.Obs)
                    {
                        Logger.AddLogEntry($"Weather is bad, pause task #{CurrentTask.TaskNumber}");
                        IsOnPause = true;
                        SiTechExeSocket.Park();
                    }
                    else
                    {
                        if (CameraFocus.IsAutoFocus)
                        {
                            if (!fitsAnalysis.Focused)
                            {
                                CameraFocus.StartAutoFocus();
                            }
                            else
                            {
                                CheckMountAndStartExp();
                            }
                        }
                        else
                        {
                            CheckMountAndStartExp();
                        }
                    }
                        
                }
                else
                {
                    EndTask(CurrentTask.DoneFrames < CurrentTask.AllFrames ? (short) 5 : (short) 2);
                }
                break;
            }
            case StringHolder.Test:
            {
                if (CurrentTask.DoneFrames < CurrentTask.AllFrames)
                {
                    
                    if (CurrentTask.RepointTimes?.Count > 0)
                    {
                        //репоинт для объектов СС
                        Logger.AddDebugLogEntry($"HEAD: Next repoint at {CurrentTask.RepointTimes[0]} to {CurrentTask.RepointCoords[0]}");
                        if (CurrentTask.RepointTimes[0] < DateTime.UtcNow)
                        {   
                            Logger.AddDebugLogEntry($"HEAD: Repointing");
                            CurrentTask.ComputeRaDec(CurrentTask.RepointCoords[0]);
                            if (SiTechExeSocket.GoTo(CurrentTask.Ra, CurrentTask.Dec, true))
                            {
                                Logger.AddDebugLogEntry($"HEAD: Repointing done, remove point");
                                CurrentTask.RepointTimes.RemoveAt(0);
                                CurrentTask.RepointCoords.RemoveAt(0);
                                DbCommunicate.UpdateTaskInDb(CurrentTask);
                            }
                        }
                        
                    }
                    else Guiding();
                    foreach (var cam in CameraControl.cams)
                    {
                        
                        if (!string.IsNullOrEmpty(cam.LatestImageFilename))
                        {
                            fitsAnalysis = new GetDataFromFits(cam); 
                            Logger.LogFrameInfo(fitsAnalysis, cam.Filter);
                        }
                    }

                    if (fitsAnalysis is null)
                    {
                        Logger.AddLogEntry("CamCallback: no data available, stop obs");
                        EndTask(5);
                        return;
                    }
                    if (CameraFocus.IsAutoFocus)
                    {
                        if (!fitsAnalysis.Focused)
                        {
                            CameraFocus.StartAutoFocus();
                        }
                        else
                        {
                            CheckMountAndStartExp();
                        }
                    }
                    else
                    {
                        CheckMountAndStartExp();
                    }
                }
                else
                {
                    EndTask(2);

                }
                break;
            }
            case StringHolder.Dark:
            {
                if (CurrentTask.DoneFrames < CurrentTask.AllFrames)
                {
                        
                    if (!(WeatherDataCollector.Obs & WeatherDataCollector.Flat))
                    {
                        StartExpAndCheckFuckup();
                    }
                    else
                    {
                        IsOnPause = true;
                    }
                }
                else
                {
                    DbCommunicate.AddMFrameToBd(CurrentTask);
                    EndTask(2);
                }
                break;
            }
            case StringHolder.Flat:
            {
                if (CurrentTask.DoneFrames < CurrentTask.AllFrames)
                {
                    if (WeatherDataCollector.Flat)
                    {
                        StartExpAndCheckFuckup();
                    }
                    else
                    {
                        EndTask(5);
                    }
                }
                else
                {
                    DbCommunicate.AddMFrameToBd(CurrentTask);
                    EndTask(2);
                }
                break;
            }
        }
    }

    public static void CheckMountAndStartExp()
    {
        while (MountDataCollector.IsSlewing)
        {
            Logger.AddDebugLogEntry("Scope is slewing, wait 1s");
            Thread.Sleep(1000);
        }
        StartExpAndCheckFuckup();
    }

    private static void Guiding()
    {
        if (_firstFrame is null)
        {
            _idec = 0;
            _ira = 0;
            _oldErrDec = 0;
            _oldErrRa = 0;
        }
        else
        {
            if (_cd11 > 99 || _cd12 > 99 || _cd11 == 0 || _cd12 == 0)
            {
                return;
            }

            if (_firstFrameLookingEast != MountDataCollector.IsLookingEast)
            {
                Logger.AddLogEntry("GUIDING: GEM flipped, reset frame to correct");
                _firstFrame = null;
                _cd11 = 100;
                _cd12 = 100;
                _idec = 0;
                _ira = 0;
                _oldErrDec = 0;
                _oldErrRa = 0;
                return;
            }
            var correction = DonutsRunner.GetImageShift(_firstFrame, 
                CameraControl.cams.Last().LatestImageFilename); //Получаем коррекцию
            
            var one = _firstFrameLookingEast ? 1 : -1;
            var dx = one * correction.Dx;
            var dy = one * correction.Dy;
            var dRa = one * correction.Dalpha;
            var dDec = one * correction.Ddelta;
            if (Math.Abs(dx) > 50 | Math.Abs(dy) > 50)
            {
                Logger.AddLogEntry($"WARNING: guiding correction: dx = {dx} px, dy = {dy} px, " +
                                   $"dRa = {dRa} arcsec, dDec = {dDec} arcsec");
            }
                
            _idec += dDec*CurrentTask.Exp;
            _ira += dRa*CurrentTask.Exp;
                
            var ddec = (dDec - _oldErrDec) / CurrentTask.Exp;
            var dra = (dRa - _oldErrRa) / CurrentTask.Exp;
                
                
            var outDec = Math.Round(_kPD*dDec + _kID*_idec + _kDD*ddec, 2);
            var outRa = Math.Round(_kPA*dRa + _kIA*_ira + _kDA*dra, 2);
                
            _oldErrDec = dDec;
            _oldErrRa = dRa;
                
            int pulseN = (int)(Math.Abs(outDec)*1e3/PulseGuideVelocityDec);
            int pulseE =  (int)(Math.Abs(outRa)*1e3/PulseGuideVelocityRa);
                
            const int bound = 20000;
            // const int bound = 10000;
            if (pulseN > bound)
            {
                Logger.AddDebugLogEntry($"WARNING Guiding: pulseN > {bound}");
                pulseN = bound;
            }
            if (pulseE > bound)
            {
                Logger.AddDebugLogEntry($"WARNING Guiding: pulseE > {bound}");
                pulseE = bound;
            }
                
            Logger.AddLogEntry($"Guiding correction: dx = {dx} px, dDec = {dDec} arcsec, " +
                               $"outDec = {outDec} arcsec, Pdec = {Math.Round(_kPD*dDec, 2 )}, " +
                               $"Idec = {Math.Round(_kID*_idec, 2)}, Ddec = {Math.Round(_kDD*ddec, 2)}");
            Logger.AddLogEntry($"Guiding correction: dy = {dy} px; dRa = {dRa} arcsec, " +
                               $"outRa = {outRa} arcsec, Pra = {Math.Round(_kPA*dRa, 2)}, " +
                               $"Ira = {Math.Round(_kIA*_ira, 2)}, Dra = {Math.Round(_kDA*dra, 2)}");
            Logger.AddDebugLogEntry($"IsLookingEast = {MountDataCollector.IsLookingEast}");
            if (!IsGuid) return;
            SiTechExeSocket.PulseGuide(outDec > 0 ? SiTechExeSocket.PulseGuideDirection.N : SiTechExeSocket.PulseGuideDirection.S, pulseN);
            SiTechExeSocket.PulseGuide(outRa > 0 ? SiTechExeSocket.PulseGuideDirection.E : SiTechExeSocket.PulseGuideDirection.W, pulseE);
            Thread.Sleep(pulseN > pulseE ? pulseN : pulseE);
        }
    }

    #region Flats
    private static void PrepareAndStartDoFlats()
    {
        var flatTask = new ObservationTask
        {
            Exp = FlatExp,
            TimeAdd = DateTime.UtcNow,
            AllFrames = FlatDarkQuantity,
            Status = 0,
            FrameType = StringHolder.Flat,
            Observer = StringHolder.AutoFlat
        };
        var zenRaDec = CoordinatesManager.GetRaDecFromAltAz(180, 90);
        flatTask.ComputeRaDec($"{Utilities.HoursToHMS(zenRaDec[0])} " +
                              $"{Utilities.DegreesToDMS(zenRaDec[1])}");
        CurrentTask = flatTask;
        StartDoFlats();
    }

    private static void StartDoFlats()
    {
        if(!WeatherDataCollector.Flat)
        {
            Logger.AddLogEntry($"can't start task# {CurrentTask.TaskNumber}, type: {CurrentTask.FrameType}, no dusk");
            EndTask(4);
            return;
        }
        IsDoFlats = true;
        Logger.AddLogEntry($"Start task# {CurrentTask.TaskNumber}, type: {CurrentTask.FrameType}");
        if (!UnparkAndGoTo()) return;
        CurrentTask.Status = 1;
        CurrentTask.Filters = CheckFil();
        CurrentTask.TimeStart = DateTime.UtcNow;
        CurrentTask.TimeEnd = DateTime.UtcNow.AddSeconds((short) (FlatDarkQuantity*CurrentTask.Exp + 180));
        CurrentTask.Duration = (float) Math.Round((CurrentTask.TimeEnd - CurrentTask.TimeStart).TotalHours, 2);
        StartExpAndCheckFuckup();
    }

    #endregion

    #region Dark

    private static void PrepareAndStartDoDark(short exp)
    {
        var darkTask = new ObservationTask
        {
            FrameType = StringHolder.Dark,
            Exp = exp,
            Status = 0,
            AllFrames = FlatDarkQuantity,
            TimeAdd = DateTime.UtcNow,
            Observer = StringHolder.AutoDark
        };
        CurrentTask = darkTask;
        StartDoDark();
    }

    private static void StartDoDark()
    {
        if (!WeatherDataCollector.Obs & !WeatherDataCollector.Flat)
        {
            IsDoDarks = true;
            Logger.AddLogEntry($"Start task #{CurrentTask.TaskNumber}, type: {CurrentTask.FrameType}, exp: {CurrentTask.Exp}");

            CurrentTask.Status = 1;
            CurrentTask.Filters = CheckFil();
            CurrentTask.TimeStart = DateTime.UtcNow;
            CurrentTask.TimeEnd = DateTime.UtcNow.AddSeconds((short) (FlatDarkQuantity*CurrentTask.Exp + 180));
            CurrentTask.Duration = (float) Math.Round((CurrentTask.TimeEnd - CurrentTask.TimeStart).TotalHours, 2);
            StartExpAndCheckFuckup();
            return;
        }

        Logger.AddLogEntry($"WARNING: can't start task# {CurrentTask.TaskNumber}, type: {CurrentTask.FrameType}");
        EndTask(4);
    }
    #endregion

    private static string CheckFil()
    {   
        var buf = CameraControl.cams
            .Aggregate("", (current, cam) => current + (cam.Filter + " "));
        return buf;
    }

    internal static void StartExpAndCheckFuckup(ObservationTask task=null)
    {
        task ??= CurrentTask;
        if (task.TaskNumber == 0)
        {
            if (!DbCommunicate.AddTaskToDb(task))
            {
                EndTask(4);
                return;
            }
        }
        else if (!CameraFocus.IsFocusing)
        {
            DbCommunicate.UpdateTaskInDb(task);
        }
        
        if (!CameraControl.StartExposure(task))
        {
            EndTask(4);
        }
        
    }
}