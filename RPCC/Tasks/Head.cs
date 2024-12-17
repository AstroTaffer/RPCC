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
    private const short FlatExp = 2; // in s 
    private const short FlatDarkQuantity = 10; //TODO add in .cfg
    public static readonly Timer ThinkingTimer = new();
    public static ObservationTask currentTask;
    private static bool _isObserve;
    private static bool _isDoFlats;
    private static bool _isDoDarks;
    public static bool isOnPause;
    private static string _firstFrame;
    private static bool _firstFrameLookingEast;
    private static double _cd11 = 100;  
    private static double _cd12 = 100;  
    private const double PulseGuideVelocityDec = 2; //sec in sec TODO add in .cfg
    private const double PulseGuideVelocityRa = 6; //sec in sec TODO add in .cfg
    public static bool isThinking;

    private static readonly short[] DarkExps = {2, 5, 10, 15, 20, 30, 50, 80, 120, 180};

    public static bool isGuid = true;
    private static int _troubles;
    private static double _idec;
    private static double _ira;
    private static double _oldErrDec;
    private static double _oldErrRa;
    private static double _kP = 1.2; //
    private static double _kI = 0.00005; //0.28
    private static double _kD = 25; //2.9
        
        
    public static void StartThinking()
    {
        ThinkingTimer.Elapsed += Thinking;
        ThinkingTimer.Interval = 5000;
    }

    private static void Thinking(object sender, ElapsedEventArgs e)
    {
        ThinkingTimer.Stop();
        if (currentTask != null)
        {
            DbCommunicate.UpdateTaskFromDb(ref currentTask);
        }
        DbCommunicate.LoadDbTable();
        if (!CameraControl.isConnected)
        {
            if (!CameraControl.ReconnectCameras())
            {
                Logger.AddLogEntry("WARNING: Can't thinking, no connection to cameras");
                return;
            }
        }
        if (!DonutsSocket.IsConnected)
        {
            if (!DonutsSocket.Connect())
            {
                Logger.AddLogEntry("WARNING: Can't thinking, no connection to Donuts");
                return;
            }
        }
        if (!SiTechExeSocket.IsConnected)
        {
            if (!SiTechExeSocket.Connect().Result)
            {
                Logger.AddLogEntry("WARNING: Can't thinking, no connection to SiTechExeSocket");
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
            if (currentTask is not null)
            {
                Logger.AddLogEntry("WARNING: Troubles with motors");
                EndTask(5);
            }
        }

        if (isOnPause & currentTask is not null)
        {
            if (_isObserve & WeatherDataCollector.Obs |
                _isDoDarks & !WeatherDataCollector.Obs |
                _isDoFlats & WeatherDataCollector.Flat)
            {
                Logger.AddLogEntry($"Head: Unpause task #{currentTask.TaskNumber}");
                if (UnparkAndGoTo())
                {
                    isOnPause = false;
                    StartExpAndCheckFuckup();
                }
            }
        }
            

        // if (!(WeatherDataCollector.Obs | WeatherDataCollector.Flat) & 
        //      (CurrentTask != null) & !IsOnPause)
        // {
        //     IsOnPause = true;
        //     Logger.AddLogEntry($"Head: Pause task #{CurrentTask.TaskNumber}");
        // }

        if (string.IsNullOrEmpty(_firstFrame) & currentTask is not null & _isObserve)
        {
            if (currentTask.RepointTimes.Count == 0) //Корректировать положение монтировки
                                                     //можно только тогда, когда не надо делать репоинты 
            {
                _firstFrame = DbCommunicate.GetPath2FirstAssFrame(currentTask.TaskNumber);
                _firstFrameLookingEast = MountDataCollector.IsLookingEast;
                Logger.AddDebugLogEntry($"Set first frame. lookingEast = {_firstFrameLookingEast}");
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
                        Logger.AddLogEntry($"Head: new CD11, CD12 = [{_cd11}, {_cd12}], " +
                                           $"kP = {_kP}, kI = {_kI}, kD = {_kD}");
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
            var bufTask = new ObservationTask();
            Tasker.GetTaskFromRow(row, ref bufTask);
            // если время конца таски уже прошло, но она все еще не начата, или тип кадра не указан
            // то ставим статус пролюблена
            if (bufTask.TimeEnd < DateTime.UtcNow)
            {
                if ((bufTask.FrameType == StringHolder.Light) | 
                    ((bufTask.FrameType == StringHolder.Dark | 
                      bufTask.FrameType == StringHolder.Flat |
                      bufTask.FrameType == StringHolder.Test) & 
                     (DateTime.UtcNow - bufTask.TimeEnd).TotalMinutes > 5))
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
                        if (currentTask is null)
                        {
                            switch (bufTask.FrameType)
                            {
                                case StringHolder.Flat:
                                    if (WeatherDataCollector.Flat)
                                    {
                                        currentTask = bufTask;
                                    }
                                    break;
                                case StringHolder.Light:
                                    if (WeatherDataCollector.Obs)
                                    {
                                        currentTask = bufTask;
                                    }
                                    break;
                                case StringHolder.Dark:
                                    if (!WeatherDataCollector.Flat & !WeatherDataCollector.Obs)
                                    {
                                        currentTask = bufTask;
                                    }
                                    break;
                            }
                        }
                        break;
                    }
                }
            }
                
            if (bufTask.Status > 0) continue; // если не ждет наблюдения, то идем дальше
            if (_isObserve || _isDoDarks || _isDoFlats) continue; // если уже идет задание, то ждем минуту
            if (currentTask is null)
            {
                switch (bufTask.FrameType)
                {
                    case StringHolder.Flat:
                        if (WeatherDataCollector.Flat)
                        {
                            currentTask = bufTask;
                        }
                        break;
                    case StringHolder.Light:
                        if (WeatherDataCollector.Obs)
                        {
                            currentTask = bufTask;
                        }
                        break;
                    case StringHolder.Dark:
                        if (!WeatherDataCollector.Flat & !WeatherDataCollector.Obs)
                        {
                            currentTask = bufTask;
                        }
                        break;
                }
            }
        }
            
        //а если нашлась и время до начала менее 5 минут, то стартуем 
        if (currentTask is null)
        {
            if (CameraControl.isConnected & !_isObserve & !_isDoDarks & !_isDoFlats)
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
            if (currentTask.Status > 0 & isOnPause) EndTask(currentTask.Status); 
            //Если у текущего задания
            //вышло время, но оно на паузе и
            //не вызовет коллбек,
            //то нужно его завершить иначе
            if (!_isObserve & !_isDoDarks & !_isDoFlats & (isThinking | currentTask.FrameType == StringHolder.Test) &
                (currentTask.TimeStart - DateTime.UtcNow).TotalMinutes < TotalMinutes2StartTask &
                CameraControl.isConnected)
            {

                switch (currentTask.FrameType)
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
        _isObserve = true;
        currentTask.Status = 1;
        Logger.AddLogEntry($"Start task# {currentTask.TaskNumber}, type: {currentTask.FrameType}");
        if (!UnparkAndGoTo()) return;
        currentTask.Filters = CheckFil();
        StartExpAndCheckFuckup();
    }
    
    private static bool UnparkAndGoTo()
    {
        if (CoordinatesManager.CalculateObjectDistance2Mount(currentTask) < 10) return true;
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
        if (SiTechExeSocket.GoTo(currentTask.Ra, currentTask.Dec, true))
        {
            Thread.Sleep(3000);
            return true;
        }
        Logger.AddLogEntry($"WARNING: can't start task #{currentTask.TaskNumber}, error while GOTO");
        EndTask(4);
        return false;

    }

    private static void EndTask(short endStatus)
    {
        currentTask.Status = endStatus;
        DbCommunicate.UpdateTaskInDb(currentTask);
        _isObserve = false;
        _isDoDarks = false;
        _isDoFlats = false;
        isOnPause = false;
        currentTask = null;
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
        _isObserve = true;
        currentTask.Status = 1;
        Logger.AddLogEntry($"Start task# {currentTask.TaskNumber}, type: {currentTask.FrameType}");
        _cd11 = 100;   
        _cd12 = 100;
        _firstFrame = null;
        if (!UnparkAndGoTo()) return; //проверять доехал ли
        currentTask.Filters = CheckFil();
        StartExpAndCheckFuckup();
    }

    public static void CamCallback()
    {
        DbCommunicate.UpdateTaskFromDb(ref currentTask);
        GetDataFromFits fitsAnalysis = null;
        currentTask.DoneFrames++;
        currentTask.TimeLastExp = DateTime.UtcNow;
        DbCommunicate.UpdateTaskInDb(currentTask);
            
        if (currentTask.Status > 1)
        {
            EndTask(currentTask.Status);
            return;
        }

        switch (currentTask.FrameType)
        {
            case StringHolder.Light:
            {
                if (currentTask.TimeEnd > DateTime.UtcNow) //Если время задания еще не вышло
                {
                    foreach (var cam in CameraControl.cams) //перебираем камеры и выводим информацию
                                                            //о полученных кадрах
                    {
                        if (!string.IsNullOrEmpty(cam.LatestImageFilename))
                        {
                            fitsAnalysis = new GetDataFromFits(cam.LatestImageFilename); //TODO распараллелить?
                            Logger.LogFrameInfo(fitsAnalysis, cam.Filter);
                        }
                    }

                    if (fitsAnalysis is null)
                    {
                        Logger.AddLogEntry("CamCallback: no data available, stop obs");
                        EndTask(5);
                        return;
                    }

                    if (currentTask.RepointTimes.Count > 0)  //репоинт для объектов СС
                    {
                        if (currentTask.RepointTimes[0] < DateTime.UtcNow)
                        {   
                            currentTask.ComputeRaDec(currentTask.RepointCoords[0]);
                            if (SiTechExeSocket.GoTo(currentTask.Ra, currentTask.Dec, true))
                            {
                                currentTask.RepointTimes.RemoveAt(0);
                                currentTask.RepointCoords.RemoveAt(0);
                                DbCommunicate.UpdateTaskInDb(currentTask);
                            }
                        }
                    }
                    else Guiding();
                    if (!WeatherDataCollector.Obs)
                    {
                        Logger.AddLogEntry($"Weather is bad, pause task #{currentTask.TaskNumber}");
                        isOnPause = true;
                        SiTechExeSocket.Park();
                    }
                    else
                    {
                        if (CameraFocus.IsAutoFocus)
                        {
                            if (!fitsAnalysis.Focused)
                            {
                                CameraFocus.StartAutoFocus(currentTask);
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
                    EndTask(currentTask.DoneFrames < currentTask.AllFrames ? (short) 5 : (short) 2);
                }
                break;
            }
            case StringHolder.Test:
            {
                if (currentTask.DoneFrames < currentTask.AllFrames)
                {
                    Guiding();
                    foreach (var cam in CameraControl.cams)
                    {
                        if (!string.IsNullOrEmpty(cam.LatestImageFilename))
                        {
                            fitsAnalysis = new GetDataFromFits(cam.LatestImageFilename); //TODO распараллелить
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
                            CameraFocus.StartAutoFocus(currentTask);
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
                if (currentTask.DoneFrames < currentTask.AllFrames)
                {
                        
                    if (!(WeatherDataCollector.Obs & WeatherDataCollector.Flat))
                    {
                        StartExpAndCheckFuckup();
                    }
                    else
                    {
                        isOnPause = true;
                    }
                }
                else
                {
                    DbCommunicate.AddMFrameToBd(currentTask);
                    EndTask(2);
                }
                break;
            }
            case StringHolder.Flat:
            {
                if (currentTask.DoneFrames < currentTask.AllFrames)
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
                    DbCommunicate.AddMFrameToBd(currentTask);
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
            // _firstFrame = CameraControl.cams.Last().LatestImageFilename;
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
            if (!DonutsSocket.IsConnected)
            {
                Logger.AddLogEntry("WARNING: can't guide, no connection to donuts");
                return;
            } //Если нет конекта или нет матрицы, то выходим
            var correction = DonutsSocket.GetGuideCorrection(_firstFrame, 
                CameraControl.cams.Last().LatestImageFilename); //Получаем коррекцию
            var one = _firstFrameLookingEast ? 1 : -1;
            var dx = one * correction[0];
            var dy = one * correction[1];
            var dRa = one * correction[2];
            var dDec = one * correction[3];
            if (Math.Abs(dx) > 50 | Math.Abs(dy) > 50)
            {
                Logger.AddLogEntry($"WARNING: guiding correction: dx = {dx} px, dy = {dy} px, " +
                                   $"dRa = {dRa} arcsec, dDec = {dDec} arcsec");
            }
                
            _idec += dDec*currentTask.Exp;
            _ira += dRa*currentTask.Exp;
                
            var ddec = (dDec - _oldErrDec) / currentTask.Exp;
            var dra = (dRa - _oldErrRa) / currentTask.Exp;
                
                
            var outDec = Math.Round(_kP*dDec + _kI*_idec + _kD*ddec, 2);
            var outRa = Math.Round(_kP*dRa + _kI*_ira + _kD*dra, 2);
                
            _oldErrDec = dDec;
            _oldErrRa = dRa;
                
            int pulseN = (int)(Math.Abs(outDec)*1e3/PulseGuideVelocityDec);
            int pulseE =  (int)(Math.Abs(outRa)*1e3/PulseGuideVelocityRa);
                
            const int bound = 20000;
            if (pulseN > bound) pulseN = bound;
            if (pulseE > bound) pulseE = bound;
                
            Logger.AddLogEntry($"Guiding correction: dx = {dx} px, dDec = {dDec} arcsec, " +
                               $"outDec = {outDec} arcsec, Pdec = {Math.Round(_kP*dDec, 2 )}, " +
                               $"Idec = {Math.Round(_kI*_idec, 2)}, Ddec = {Math.Round(_kD*ddec, 2)}");
            Logger.AddLogEntry($"Guiding correction: dy = {dy} px; dRa = {dRa} arcsec, " +
                               $"outRa = {outRa} arcsec, Pra = {Math.Round(_kP*dRa, 2)}, " +
                               $"Ira = {Math.Round(_kI*_ira, 2)}, Dra = {Math.Round(_kD*dra, 2)}");
            Logger.AddDebugLogEntry($"IsLookingEast = {MountDataCollector.IsLookingEast}");
            if (!isGuid) return;
            SiTechExeSocket.PulseGuide(outDec > 0 ? "N" : "S", pulseN);
            SiTechExeSocket.PulseGuide(outRa > 0 ? "E" : "W", pulseE);
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
        currentTask = flatTask;
        StartDoFlats();
    }

    private static void StartDoFlats()
    {
        if(!WeatherDataCollector.Flat)
        {
            Logger.AddLogEntry($"can't start task# {currentTask.TaskNumber}, type: {currentTask.FrameType}, no dusk");
            EndTask(4);
        }
        _isDoFlats = true;
        Logger.AddLogEntry($"Start task# {currentTask.TaskNumber}, type: {currentTask.FrameType}");
        if (!UnparkAndGoTo()) return;
        currentTask.Status = 1;
        currentTask.Filters = CheckFil();
        currentTask.TimeStart = DateTime.UtcNow;
        currentTask.TimeEnd = DateTime.UtcNow.AddSeconds((short) (FlatDarkQuantity*currentTask.Exp + 180));
        currentTask.Duration = (float) Math.Round((currentTask.TimeEnd - currentTask.TimeStart).TotalHours, 2);
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
        currentTask = darkTask;
        StartDoDark();
    }

    private static void StartDoDark()
    {
        if (!WeatherDataCollector.Obs & !WeatherDataCollector.Flat)
        {
            _isDoDarks = true;
            Logger.AddLogEntry($"Start task# {currentTask.TaskNumber}, type: {currentTask.FrameType}");

            currentTask.Status = 1;
            currentTask.Filters = CheckFil();
            currentTask.TimeStart = DateTime.UtcNow;
            currentTask.TimeEnd = DateTime.UtcNow.AddSeconds((short) (FlatDarkQuantity*currentTask.Exp + 180));
            currentTask.Duration = (float) Math.Round((currentTask.TimeEnd - currentTask.TimeStart).TotalHours, 2);
            StartExpAndCheckFuckup();
            return;
        }

        Logger.AddLogEntry($"WARNING: can't start task# {currentTask.TaskNumber}, type: {currentTask.FrameType}");
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
        task ??= currentTask;
        if (CameraControl.StartExposure(task))
        {
            DbCommunicate.UpdateTaskInDb(task);
            return;
        }
        EndTask(4);
    }
}