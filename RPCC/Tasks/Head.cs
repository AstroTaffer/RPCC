using System;
using System.Collections.Generic;
using System.Linq;
using System.Timers;
using System.Windows.Forms;
using ASCOM.Tools;
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
        private static readonly Timer ThinkingTimer = new Timer();
        private static ObservationTask _currentTask;
        private static bool _isObserve;
        private static bool _isDoFlats;
        private static bool _isDoDarks;
        private static bool _isOnPause;

        private static readonly short[] DarkExps = {2, 5, 10, 15, 20, 30, 50, 80, 120, 180};
        private static readonly List<ObservationTask> Darks = new List<ObservationTask>();
        private const string Dark = "Dark";
        private const string Flat = "Flat";
        private const string Light = "Object";
        private const string Test = "Test";
        
        public static void StartThinking()
        {
            ThinkingTimer.Elapsed += Thinking;
            ThinkingTimer.Interval = 10000; //думать раз в минуту TODO
            ThinkingTimer.Start();
            // Thinking(null, null);
        }

        private static void Thinking(object sender, ElapsedEventArgs e)
        {
            ThinkingTimer.Stop();
            
            _currentTask?.Update();
            
            if (!CameraControl.isConnected)
            {
                CameraControl.ReconnectCameras();
            }
            
            // if (!CameraControl.isConnected & WeatherDataCollector.Sun >= 90)
            // {
            //     CameraControl.ReconnectCameras();
            // }
            //
            // if (CameraControl.isConnected & WeatherDataCollector.Sun < 89)
            // {
            //     CameraControl.DisconnectCameras();
            // }
            
            // Logger.AddLogEntry($"Last darks time {Settings._lastDarksTime}");

            if (_isOnPause)
            {
                if (_isObserve & WeatherDataCollector.Obs)
                {
                    _isOnPause = false;
                    CameraControl.StartExposure();
                }

                if (_isDoDarks & !WeatherDataCollector.Obs)
                {
                    _isOnPause = false;
                    CameraControl.StartExposure();
                }

                if (_isDoFlats & WeatherDataCollector.Flat)
                {
                    _isOnPause = false;
                    CameraControl.StartExposure();
                }
            }

            if (CameraControl.isConnected & !_isObserve & !_isDoDarks & !_isDoFlats)
            {
                if (WeatherDataCollector.Flat &
                                ((DateTime.UtcNow - Settings._lastFlatsTime).TotalDays > TotalDays2ReMakeCalibrationFrames)) PrepareAndStartDoFlats();
                
                if (!WeatherDataCollector.Obs &
                                ((DateTime.UtcNow - Settings._lastDarksTime).TotalDays > TotalDays2ReMakeCalibrationFrames)) PrepareAndStartDoDarks();
            }

            foreach (DataGridViewRow row in Tasker.dataGridViewTasker.Rows)
            {
                var bufTask = Tasker.GetTaskByNumber(row.Cells[0].Value.ToString());

                // если время конца таски уже прошло, но она все еще не начата, или тип кадра не указан
                // то ставим статус пролюблена
                if (bufTask.TimeEnd < DateTime.UtcNow || string.IsNullOrEmpty(bufTask.FrameType))
                {
                    if ((bufTask.FrameType == Light) | ((bufTask.FrameType == Dark | bufTask.FrameType == Flat) &
                                                        (DateTime.UtcNow - bufTask.TimeEnd).TotalMinutes > 10))
                    {
                        switch (bufTask.Status)
                        {
                            case 1:
                            {
                                bufTask.Status = bufTask.DoneFrames < bufTask.AllFrames ? (short) 5 : (short) 2;
                                break;
                            }
                            case 0:
                            {
                                bufTask.Status = 4;
                                break;
                            }
                        }
;
                    }
                    continue;
                }
                else
                {
                    switch (bufTask.Status)
                    {
                        case 1:
                        {
                            if (_currentTask is null)
                            {
                                bufTask.Status = 0;
                            }
                            break;
                        }
                        
                    }
                }
                Tasker.UpdateTaskInTable(bufTask);
                
                if (bufTask.Status > 0) continue; // если не ждет наблюдения, то идем дальше
                
                if (_isObserve || _isDoDarks || _isDoFlats) continue; // если уже идет задание, то ждем минуту
                // Далее выбираем таску для наблюдения 
                // if (!WeatherDataCollector.Obs) {
                //     continue; // если нельзя наблюдать, то ждем минуту
                // }

                //выбираем таску 
                if (_currentTask is null)
                {
                    _currentTask = bufTask;
                    // ThinkingTimer.Start();
                    continue;
                }

                if (bufTask.TimeStart < _currentTask.TimeStart) //выбираем таску с наименьшей датой начала наблюдения
                    //(вне зависимости от нынешнего времени)
                    _currentTask = bufTask;
            }
            
            //если после проверки таблицы не нашлась таска на выполнение, то уходим на следующую минуту
            if (_currentTask == null || _isObserve || _isDoDarks || _isDoFlats)
            {
                ThinkingTimer.Start();
                return;
            }
            //а если нашлась и время до начала менее 5 минут, то стартуем 
            if ((_currentTask.TimeStart - DateTime.UtcNow).TotalMinutes < TotalMinutes2StartTask &
                !_isDoDarks & !_isDoFlats & !_isObserve & CameraControl.isConnected)
            {
                // if (MountDataCollector.IsParking) TODO
                // {
                //     ThinkingTimer.Start();
                //     return;
                // }
                // if (MountDataCollector.IsParked)
                // {
                //     SiTechExeSocket.Unpark();
                // }
                switch (_currentTask.FrameType)
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
            ThinkingTimer.Start();
        }

        private static void StartDoTest()
        {
            _isObserve = true;
            SiTechExeSocket.GoTo(_currentTask.Ra, _currentTask.Dec, true); //проверять доехал ли
            //если доехал, то начинаем снимать 
            _currentTask.Status = 1;
            Logger.AddLogEntry($"Start task# {_currentTask.TaskNumber}, type: {_currentTask.FrameType}");
            if (!CameraControl.PrepareToObs(_currentTask)) return;  //не удалось подготовиться к наблюдению  
            _currentTask.Filters = CheckFil();
            Tasker.UpdateTaskInTable(_currentTask);
            CameraControl.StartExposure();
        }

        // private static void StartTask()
        // {
        //     
        //     if (_currentTask.FrameType == Light & WeatherDataCollector.Obs) StartDoLight();
        //     if (_currentTask.FrameType == Dark & !WeatherDataCollector.Obs) StartDoDark();
        //     if (_currentTask.FrameType == Flat & WeatherDataCollector.Flat) StartDoFlats();
        // }

        public static void EndTask(short endStatus)
        {
            _currentTask.Status = endStatus;
            Tasker.UpdateTaskInTable(_currentTask);
            _isObserve = false;
            _isDoDarks = false;
            _isDoFlats = false;
            _isOnPause = false;
            // _currentTask = null;
            // if (!MountDataCollector.IsParked)
            // {
            //     SiTechExeSocket.Park();
            // }
        }

        private static void StartDoLight()
        {   
            _isObserve = true;
            SiTechExeSocket.GoTo(_currentTask.Ra, _currentTask.Dec, true); //проверять доехал ли
            //если доехал, то начинаем снимать 
            _currentTask.Status = 1;
            Logger.AddLogEntry($"Start task# {_currentTask.TaskNumber}, type: {_currentTask.FrameType}");
            if (!CameraControl.PrepareToObs(_currentTask)) return;  //не удалось подготовиться к наблюдению  
            _currentTask.Filters = CheckFil();
            Tasker.UpdateTaskInTable(_currentTask);
            CameraControl.StartExposure();
        }

        public static void CamCallback()
        {
            _currentTask.Update();
            GetDataFromFits fitsAnalysis = null;
            _currentTask.DoneFrames++;
            _currentTask.TimeLastExp = DateTime.UtcNow;
            Tasker.UpdateTaskInTable(_currentTask);

            if (_currentTask.Status > 1)
            {
                _isObserve = false;
                _isDoDarks = false;
                _isDoFlats = false;
                _isOnPause = false;
                _currentTask = null;
                return;
            }

            switch (_currentTask.FrameType)
            {
                case Light:
                {
                    if (_currentTask.TimeEnd > DateTime.UtcNow)
                    {
                        foreach (var cam in CameraControl.cams)
                        {
                            if (!string.IsNullOrEmpty(cam.latestImageFilename))
                            {
                                fitsAnalysis =  new GetDataFromFits(cam.latestImageFilename); //TODO распараллелить
                            }
                        }

                        if (fitsAnalysis is null)
                        {
                            Logger.AddLogEntry("CamCallback: no data available, stop obs");
                            EndTask(5);
                            return;
                        }
                        if (fitsAnalysis.CheckFocused() || !CameraFocus.IsAutoFocus)
                        {
                            if (WeatherDataCollector.Obs) CameraControl.StartExposure();
                            else _isOnPause = true;
                        }
                        else CameraFocus.StartAutoFocus(_currentTask);
                    
                        //asinc doAstrometry() 
                        //asinc doDonuts()
                    
                    }
                    else
                    {
                        EndTask(_currentTask.DoneFrames < _currentTask.AllFrames ? (short) 5 : (short) 2);
                        _currentTask = null;
                    }
                    break;
                }
                case Test:
                {
                    if (_currentTask.DoneFrames < _currentTask.AllFrames)
                    {
                        CameraControl.StartExposure();
                    }
                    else
                    {
                        EndTask(2);
                        _currentTask = null;
                    }
                    break;
                }
                case Dark:
                {
                    if (_currentTask.DoneFrames < _currentTask.AllFrames)
                    {
                        
                        if (!(WeatherDataCollector.Obs & WeatherDataCollector.Flat))
                        {
                            CameraControl.StartExposure();
                        }
                        else
                        {
                            _isOnPause = true;
                        }
                        
                    }
                    else
                    {
                        EndTask(2);
                        if (Darks.Count > 0)
                        {
                            _currentTask = Darks[0];
                            Darks.RemoveAt(0);
                            StartDoDark();
                        }
                        else
                        {
                            _currentTask = null;
                            Settings._lastDarksTime = DateTime.UtcNow;
                            Settings.SaveXmlConfig("SettingsDefault.xml");
                        }
                    }
                    break;
                }
                case Flat:
                {
                    if (_currentTask.DoneFrames < _currentTask.AllFrames)
                    {
                        if (WeatherDataCollector.Obs)
                        {
                            CameraControl.StartExposure();
                        }
                        else
                        {
                            _isOnPause = true;
                        }
                        
                    }
                    else
                    {
                        EndTask(2);
                        _currentTask = null;
                        Settings._lastFlatsTime = DateTime.UtcNow;
                        Settings.SaveXmlConfig("SettingsDefault.xml");
                    }
                    break;
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
            // flatTask.Duration = (float) Math.Round((flatTask.TimeEnd - flatTask.TimeStart).TotalHours, 2);
            var zenRaDec = SiTechExeSocket.GoToAltAz(180, 90);
            flatTask.ComputeRaDec($"{Utilities.HoursToHMS(zenRaDec[0])} " +
                                  $"{Utilities.DegreesToDMS(zenRaDec[1])}");
            Tasker.AddTask(flatTask);
            _currentTask = flatTask;
            StartDoFlats();
            // _lastFlatsTime = DateTime.UtcNow;
        }

        private static void StartDoFlats()
        {
            if(WeatherDataCollector.Flat)
            {
                _isDoFlats = true;
                Logger.AddLogEntry($"Start task# {_currentTask.TaskNumber}, type: {_currentTask.FrameType}");
                SiTechExeSocket.GoTo(_currentTask.Ra, _currentTask.Dec, true); //проверять доехал ли
                if (!CameraControl.PrepareToObs(_currentTask))
                {
                    _isDoFlats = false;
                    _currentTask.Status = 4;
                    _currentTask = null;
                    return; //не удалось подготовиться к наблюдению
                }
                _currentTask.Status = 1;
                _currentTask.Filters = CheckFil();
                _currentTask.TimeStart = DateTime.UtcNow;
                _currentTask.TimeEnd = DateTime.UtcNow.AddSeconds((short) (_currentTask.Exp + 50));
                _currentTask.Duration = (float) Math.Round((_currentTask.TimeEnd - _currentTask.TimeStart).TotalHours, 2);
                Tasker.UpdateTaskInTable(_currentTask);
                CameraControl.StartExposure();
            }
            else
            {
                Logger.AddLogEntry($"Can't start task# {_currentTask.TaskNumber}, type: {_currentTask.FrameType}, no dusk");
            }
        }

        #endregion

        #region Dark

        private static void PrepareAndStartDoDarks()
        {
            // var l = 0;
            foreach (var exp in DarkExps)
            {
                // l += exp;
                var darkTask = new ObservationTask
                {
                    TaskNumber = Tasker.GetTasksLen(),
                    FrameType = Dark,
                    Exp = exp,
                    Status = 0,
                    AllFrames = FlatDarkQuantity,
                    TimeAdd = DateTime.UtcNow,
                    // TimeStart = DateTime.UtcNow.AddSeconds(l + 180), 
                    Observer = "AUTO_DARK"
                };
                Tasker.AddTask(darkTask);
                Darks.Add(darkTask);
            }
            // Settings._lastDarksTime = DateTime.UtcNow;
            _currentTask = Darks[0];
            Darks.RemoveAt(0);
            StartDoDark();
        }

        private static void StartDoDark()
        {
            _isDoDarks = true;
            Logger.AddLogEntry($"Start task# {_currentTask.TaskNumber}, type: {_currentTask.FrameType}");
            if (!CameraControl.PrepareToObs(_currentTask))
            {
                _isDoDarks = false;
                return;  //не удалось подготовиться к наблюдению
            }
            _currentTask.Status = 1;
            _currentTask.Filters = CheckFil();
            _currentTask.TimeStart = DateTime.UtcNow;
            _currentTask.TimeEnd = DateTime.UtcNow.AddSeconds((short) (_currentTask.Exp + 50));
            _currentTask.Duration = (float) Math.Round((_currentTask.TimeEnd - _currentTask.TimeStart).TotalHours, 2);
            Tasker.UpdateTaskInTable(_currentTask);
            CameraControl.StartExposure();
        }
        #endregion

        private static string CheckFil()
        {
            var buf = CameraControl.cams.Where(cam => cam.isSelected).Aggregate("", (current, cam) => current + (cam.filter + " "));
            return buf;
        }
    }
}