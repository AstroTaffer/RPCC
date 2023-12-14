using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Timers;
using ASCOM.Tools;
using nom.tam.fits;
using RPCC.Cams;
using RPCC.Comms;
using RPCC.Focus;
using RPCC.Utils;
using Timer = System.Timers.Timer;

namespace RPCC.Tasks
{
    public static class Head
    {
        private const short TotalMinutes2StartTask = 5; //TODO add in .cfg
        private const short TotalDays2ReMakeCalibrationFrames = 30; //TODO add in .cfg
        private const short FlatExp = 2; // in s 
        private const short FlatDarkQuantity = 10; //TODO add in .cfg
        public static readonly Timer ThinkingTimer = new Timer();
        public static ObservationTask currentTask;
        private static bool _isObserve;
        public static bool isFocusing;
        private static bool _isDoFlats;
        private static bool _isDoDarks;
        public static bool isOnPause;
        private static string _firstFrame;
        private static double CD1_1 = 100;
        private static double CD1_2 = 100;
        private static bool _isLookingEastLastCd;
        private const double PulseGuideVelocity = 2; //sec in sec TODO add in .cfg
        public static bool IsThinking;

        private static readonly short[] DarkExps = {2, 5, 10, 15, 20, 30, 50, 80, 120, 180};
        private static readonly List<ObservationTask> Darks = new List<ObservationTask>();
        public const string Dark = "Dark";
        public const string Flat = "Flat";
        public const string Light = "Object";
        public const string Test = "Test";
        public static bool isGuid = true;

        public static void StartThinking()
        {
            ThinkingTimer.Elapsed += Thinking;
            ThinkingTimer.Interval = 10000; //думать раз в минуту?
        }

        private static void Thinking(object sender, ElapsedEventArgs e)
        {
            ThinkingTimer.Stop();
            if (currentTask != null)
            {
                DbCommunicate.UpdateTaskFromDb(ref currentTask);
            }

            // if (_currentTask != null & !_isObserve)
            // {
            //     _currentTask = null;
            // }
            DbCommunicate.LoadDbTable();
            // Tasker.PaintTable();
            if (!CameraControl.isConnected)
            {
                CameraControl.ReconnectCameras();
            }

            if (isOnPause & !(currentTask is null))
            {
                if (_isObserve & WeatherDataCollector.Obs |
                    _isDoDarks & !WeatherDataCollector.Obs |
                    _isDoFlats & WeatherDataCollector.Flat)
                {
                    isOnPause = false;
                    Logger.AddLogEntry($"Unpause task #{currentTask.TaskNumber}");
                    CameraControl.StartExposure();
                }
            }

            if (!string.IsNullOrEmpty(_firstFrame))
            {
                try
                {
                    //Если матрицы еще нет, ориентируемся на первый кадр, если есть, ловим решенный кадр по свежее
                    var fits = new Fits(CD1_1 > 99 | CD1_2 > 99
                        ? _firstFrame
                        : CameraControl.cams.Last().latestImageFilename);
                    var hdu = (ImageHDU)fits.GetHDU(0);

                    CD1_1 = hdu.Header.GetDoubleValue("CD1_1");
                    CD1_2 = hdu.Header.GetDoubleValue("CD1_2");
                    _isLookingEastLastCd = MountDataCollector.IsLookingEast;
                    fits.Close();
                }
                catch
                {

                }

            }

            if (CameraControl.isConnected & !_isObserve & !_isDoDarks & !_isDoFlats)
            {
                if (WeatherDataCollector.Flat &
                    ((DateTime.UtcNow - Settings.LastFlatsTime).TotalDays > TotalDays2ReMakeCalibrationFrames))
                    PrepareAndStartDoFlats();

                if (!WeatherDataCollector.Obs &
                    ((DateTime.UtcNow - Settings.LastDarksTime).TotalDays > TotalDays2ReMakeCalibrationFrames))
                    PrepareAndStartDoDarks();
            }

            foreach (DataRow row in DbCommunicate.GetTableForThinking().Rows)
            {
                var bufTask = new ObservationTask();
                Tasker.GetTaskFromRow(row, ref bufTask);
                // если время конца таски уже прошло, но она все еще не начата, или тип кадра не указан
                // то ставим статус пролюблена
                if (bufTask.TimeEnd < DateTime.UtcNow || string.IsNullOrEmpty(bufTask.FrameType))
                {
                    if ((bufTask.FrameType == Light) | ((bufTask.FrameType == Dark | bufTask.FrameType == Flat |
                                                         bufTask.FrameType == Test) &
                                                        (DateTime.UtcNow - bufTask.TimeEnd).TotalMinutes > 10))
                    {
                        switch (bufTask.Status)
                        {
                            case 1:
                            {
                                bufTask.Status = bufTask.DoneFrames < bufTask.AllFrames ? (short)5 : (short)2;
                                DbCommunicate.UpdateTaskInDb(bufTask);
                                break;
                            }
                            case 0:
                            {
                                bufTask.Status = 4;
                                DbCommunicate.UpdateTaskInDb(bufTask);
                                break;
                            }
                        }
                    }
                    // continue;
                }
                else
                {
                    switch (bufTask.Status)
                    {
                        case 1:
                        {
                            if (currentTask is null)
                            {
                                // bufTask.Status = 0;
                                currentTask = bufTask;
                                // DbCommunicate.UpdateTaskInDb(_currentTask);
                                
                            }
                            break;
                        }
                    }
                }

                if (bufTask.Status > 0) continue; // если не ждет наблюдения, то идем дальше
                if (_isObserve || _isDoDarks || _isDoFlats) continue; // если уже идет задание, то ждем минуту
                if (currentTask is null)
                {
                    currentTask = bufTask;
                }
            }

        
            //а если нашлась и время до начала менее 5 минут, то стартуем 
        if (!(currentTask is null))
        {
            if (!_isObserve & !_isDoDarks & !_isDoFlats & (IsThinking | currentTask.FrameType == Test) &
                  (currentTask.TimeStart - DateTime.UtcNow).TotalMinutes < TotalMinutes2StartTask &
                  CameraControl.isConnected)
            {
                if (MountDataCollector.IsParked)
                {
                    SiTechExeSocket.Unpark();
                    //Thread.Sleep(5000);
                }

                switch (currentTask.FrameType)
                {
                    case Light:
                    {
                        if (WeatherDataCollector.Obs)
                        {
                            StartDoLight();
                        }

                        break;
                    }
                    case Test:
                    {
                        StartDoTest();
                        break;
                    }
                    case Dark:
                    {
                        if (!WeatherDataCollector.Obs)
                        {
                            StartDoDark();
                        }

                        break;
                    }
                    case Flat:
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
            // DbCommunicate.UpdateTaskInDb(_currentTask);
            if (!(Math.Abs(currentTask.Ra - MountDataCollector.RightAsc) < 1e-3 &
                  Math.Abs(currentTask.Dec - MountDataCollector.Declination) < 1e-3))
            {
                if (MountDataCollector.IsParked)
                {
                    SiTechExeSocket.Unpark();
                }
                if (!SiTechExeSocket.GoTo(currentTask.Ra, currentTask.Dec, true))
                {
                    Logger.AddLogEntry($"WARNING: can't start task #{currentTask.TaskNumber}, error while GOTO");
                    EndTask(4);
                    return;
                }
            }
            
            if (CameraControl.PrepareToObs(currentTask))
            {
                Logger.AddLogEntry($"Start task# {currentTask.TaskNumber}, type: {currentTask.FrameType}");
                currentTask.Filters = CheckFil();
                DbCommunicate.UpdateTaskInDb(currentTask);
                CameraControl.StartExposure();
                return;
            }
            else
            {
                Logger.AddLogEntry($"WARNING: can't start task #{currentTask.TaskNumber}, error while prepare to observe");
            }
            EndTask(4);
        }

        public static void EndTask(short endStatus)
        {
            currentTask.Status = endStatus;
            DbCommunicate.UpdateTaskInDb(currentTask);
            _isObserve = false;
            _isDoDarks = false;
            _isDoFlats = false;
            isOnPause = false;
            currentTask = null;
            _firstFrame = null;
            if (!MountDataCollector.IsParked)
            {
                SiTechExeSocket.Park();
            }
        }

        private static void StartDoLight()
        {   
            _isObserve = true;
            if (SiTechExeSocket.GoTo(currentTask.Ra, currentTask.Dec, true)) //проверять доехал ли
            {
                currentTask.Status = 1;
                Logger.AddLogEntry($"Start task# {currentTask.TaskNumber}, type: {currentTask.FrameType}");
                if (CameraControl.PrepareToObs(currentTask))
                {
                    currentTask.Filters = CheckFil();
                    // Tasker.UpdateTaskInTable(_currentTask);
                    DbCommunicate.UpdateTaskInDb(currentTask);
                    CameraControl.StartExposure();
                    //если доехал, то начинаем снимать 
                    return;
                }
                else
                {
                    Logger.AddLogEntry($"WARNING: can't start task #{currentTask.TaskNumber}, error while prepare to observe");
                }
                //не удалось подготовиться к наблюдению  
                
            }
            else
            {
                Logger.AddLogEntry($"WARNING: can't start task #{currentTask.TaskNumber}, error while GOTO");
            }
            EndTask(4);
        }

        public static void CamCallback()
        {
            DbCommunicate.UpdateTaskFromDb(ref currentTask);
            GetDataFromFits fitsAnalysis = null;
            currentTask.DoneFrames++;
            currentTask.TimeLastExp = DateTime.UtcNow;
            // Tasker.UpdateTaskInTable(_currentTask);
            DbCommunicate.UpdateTaskInDb(currentTask);
            
            if (currentTask.Status > 1)
            {
                _isObserve = false;
                _isDoDarks = false;
                _isDoFlats = false;
                isOnPause = false;
                currentTask = null;
                if (!MountDataCollector.IsParked)
                {
                    SiTechExeSocket.Park();
                }
                return;
            }

            switch (currentTask.FrameType)
            {
                case Light:
                {
                    if (currentTask.TimeEnd > DateTime.UtcNow)
                    {
                        // 
                        if (isGuid)
                        {
                            Guiding(); 
                        }

                        foreach (var cam in CameraControl.cams)
                        {
                            if (!string.IsNullOrEmpty(cam.latestImageFilename))
                            {
                                fitsAnalysis = new GetDataFromFits(cam.latestImageFilename); //TODO распараллелить
                            }
                        }

                        if (fitsAnalysis is null)
                        {
                            Logger.AddLogEntry("CamCallback: no data available, stop obs");
                            EndTask(5);
                            return;
                        }

                        if (!WeatherDataCollector.Obs)
                        // if (false)
                        {
                            Logger.AddLogEntry($"Weather is bad, pause task #{currentTask.TaskNumber}");
                            isOnPause = true;
                            // return;
                        }
                        else
                        {
                            if (CameraFocus.IsAutoFocus)
                            {
                                if (!fitsAnalysis.CheckFocused())
                                {
                                    CameraFocus.StartAutoFocus(currentTask);
                                }
                                else
                                {
                                    CameraControl.StartExposure();
                                }
                            }
                            else
                            {
                                CameraControl.StartExposure();
                            }
                        }
                        
                    }
                    else
                    {
                        EndTask(currentTask.DoneFrames < currentTask.AllFrames ? (short) 5 : (short) 2);
                    }
                    break;
                }
                case Test:
                {
                    if (currentTask.DoneFrames < currentTask.AllFrames)
                    {
                        CameraControl.StartExposure();
                    }
                    else
                    {
                        EndTask(2);

                    }
                    break;
                }
                case Dark:
                {
                    if (currentTask.DoneFrames < currentTask.AllFrames)
                    {
                        
                        if (!(WeatherDataCollector.Obs & WeatherDataCollector.Flat))
                        {
                            CameraControl.StartExposure();
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
                        if (Darks.Count > 0)
                        {
                            currentTask = Darks[0];
                            Darks.RemoveAt(0);
                            StartDoDark();
                        }
                        else
                        {
                            Settings.LastDarksTime = DateTime.UtcNow;
                            Settings.SaveXmlConfig("SettingsDefault.xml");
                        }
                    }
                    break;
                }
                case Flat:
                {
                    if (currentTask.DoneFrames < currentTask.AllFrames)
                    {
                        if (WeatherDataCollector.Obs)
                        {
                            CameraControl.StartExposure();
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
                        Settings.LastFlatsTime = DateTime.UtcNow;
                        Settings.SaveXmlConfig("SettingsDefault.xml");
                    }
                    break;
                }
            }
        }

        public static void Guiding()
        {
            if (_firstFrame is null)
            {
                _firstFrame = CameraControl.cams.Last().latestImageFilename;
            }
            else
            {
                if (DonutsSocket.IsConnected & CD1_1 < 100 & CD1_2 < 100)
                {
                    var req = $"{_firstFrame}_{CameraControl.cams.Last().latestImageFilename}";
                    var correction = DonutsSocket.GetGuideCorrection(req);

                    correction[0] = (float) (correction[0] * CD1_1 * 60 * 60);
                    correction[1] = (float) (correction[1] * CD1_2 * 60 * 60);
                    //arcsec
                    //x = north
                    //y = east
                    if (!(_isLookingEastLastCd & MountDataCollector.IsLookingEast))
                    {
                        correction[0] *= -1;
                        correction[1] *= -1;
                    }
                    SiTechExeSocket.PulseGuide(correction[0] > 0 ? "N" : "S", (int) (correction[0]*1e3/PulseGuideVelocity));
                    SiTechExeSocket.PulseGuide(correction[1] > 0 ? "E" : "W", (int) (correction[1]*1e3/PulseGuideVelocity));
                }
            }
        }

        #region Flats
        private static void PrepareAndStartDoFlats()
        {
            var flatTask = new ObservationTask
            {
                Exp = FlatExp,
                TaskNumber = Tasker.GetTasksLen(),
                TimeAdd = DateTime.UtcNow,
                AllFrames = FlatDarkQuantity,
                Status = 0,
                FrameType = Flat,
                Observer = "AUTO_FLAT"
            };
            var zenRaDec = SiTechExeSocket.GoToAltAz(180, 90);
            flatTask.ComputeRaDec($"{Utilities.HoursToHMS(zenRaDec[0])} " +
                                  $"{Utilities.DegreesToDMS(zenRaDec[1])}");
            DbCommunicate.AddTaskToDb(flatTask);
            // Tasker.AddTask(flatTask);
            currentTask = flatTask;
            StartDoFlats();
        }

        private static void StartDoFlats()
        {
            if(WeatherDataCollector.Flat)
            {
                _isDoFlats = true;
                Logger.AddLogEntry($"Start task# {currentTask.TaskNumber}, type: {currentTask.FrameType}");
                if (SiTechExeSocket.GoTo(currentTask.Ra, currentTask.Dec, true))
                {
                    if (CameraControl.PrepareToObs(currentTask))
                    {
                        currentTask.Status = 1;
                        currentTask.Filters = CheckFil();
                        currentTask.TimeStart = DateTime.UtcNow;
                        currentTask.TimeEnd = DateTime.UtcNow.AddSeconds((short) (FlatDarkQuantity*currentTask.Exp + 180));
                        currentTask.Duration = (float) Math.Round((currentTask.TimeEnd - currentTask.TimeStart).TotalHours, 2);
                        // Tasker.UpdateTaskInTable(_currentTask);
                        DbCommunicate.UpdateTaskInDb(currentTask);
                        CameraControl.StartExposure();
                        return; //не удалось подготовиться к наблюдению
                    }
                    else
                    {
                        Logger.AddLogEntry($"WARNING: can't start task #{currentTask.TaskNumber}, error while prepare to observe");
                    }
                }
                else
                {
                    Logger.AddLogEntry($"WARNING: can't start task #{currentTask.TaskNumber}, error while GOTO");
                }
            }
            else
            {
                Logger.AddLogEntry($"Can't start task# {currentTask.TaskNumber}, type: {currentTask.FrameType}, no dusk");
            }
            EndTask(4);
        }

        #endregion

        #region Dark

        private static void PrepareAndStartDoDarks()
        {
            foreach (var exp in DarkExps)
            {
                var darkTask = new ObservationTask
                {
                    TaskNumber = Tasker.GetTasksLen(),
                    FrameType = Dark,
                    Exp = exp,
                    Status = 0,
                    AllFrames = FlatDarkQuantity,
                    TimeAdd = DateTime.UtcNow,
                    Observer = "AUTO_DARK"
                };
                // Tasker.AddTask(darkTask);
                DbCommunicate.AddTaskToDb(darkTask);
                Darks.Add(darkTask);
            }
            currentTask = Darks[0];
            Darks.RemoveAt(0);
            StartDoDark();
        }

        private static void StartDoDark()
        {
            _isDoDarks = true;
            Logger.AddLogEntry($"Start task# {currentTask.TaskNumber}, type: {currentTask.FrameType}");
            if (CameraControl.PrepareToObs(currentTask))
            {
                 currentTask.Status = 1;
                 currentTask.Filters = CheckFil();
                 currentTask.TimeStart = DateTime.UtcNow;
                 currentTask.TimeEnd = DateTime.UtcNow.AddSeconds((short) (FlatDarkQuantity*currentTask.Exp + 180));
                 currentTask.Duration = (float) Math.Round((currentTask.TimeEnd - currentTask.TimeStart).TotalHours, 2);
                 // Tasker.UpdateTaskInTable(_currentTask);
                 DbCommunicate.UpdateTaskInDb(currentTask);
                 CameraControl.StartExposure();
                 return;
            }
            else
            {
                Logger.AddLogEntry($"WARNING: can't start task #{currentTask.TaskNumber}, error while prepare to observe");
            }


            if (Darks.Count>0)
            {
                currentTask.Status = 4;
                // Tasker.UpdateTaskInTable(_currentTask);
                DbCommunicate.UpdateTaskInDb(currentTask);
                currentTask = Darks[0];
                Darks.RemoveAt(0);
                StartDoDark();
            }
            else
            {
                EndTask(4);
            }

        }
        #endregion

        private static string CheckFil()
        {
            var buf = CameraControl.cams.Where(cam => cam.isSelected).Aggregate("", (current, cam) => current + (cam.filter + " "));
            return buf;
        }
    }
}