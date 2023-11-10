using System;
using System.Collections.Generic;
using System.Linq;
using System.Timers;
using System.Windows.Forms;
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
        private static ObservationTask _currentTask;
        private static bool _isObserve;
        private static bool _isDoFlats;
        private static bool _isDoDarks;
        private static bool _isOnPause;
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

        public static void StartThinking()
        {
            ThinkingTimer.Elapsed += Thinking;
            ThinkingTimer.Interval = 10000; //думать раз в минуту?
        }

        private static void Thinking(object sender, ElapsedEventArgs e)
        {
            ThinkingTimer.Stop();
            _currentTask?.Update();
            
            if (!CameraControl.isConnected)
            {
                CameraControl.ReconnectCameras();
            }

            if (_isOnPause & !(_currentTask is null))
            {
                if (_isObserve & WeatherDataCollector.Obs |
                    _isDoDarks & !WeatherDataCollector.Obs |
                    _isDoFlats & WeatherDataCollector.Flat)
                {
                    _isOnPause = false;
                    Logger.AddLogEntry($"Unpause task #{_currentTask.TaskNumber}");
                    CameraControl.StartExposure();
                }
            }

            if (!string.IsNullOrEmpty(_firstFrame))
            {
                //Если матрицы еще нет, ориентируемся на первый кадр, если есть, ловим решенный кадр по свежее
                var fits = new Fits(CD1_1 > 99 | CD1_2 > 99 ? _firstFrame : CameraControl.cams.Last().latestImageFilename);
                var hdu = (ImageHDU) fits.GetHDU(0);
                try
                {
                    CD1_1 = hdu.Header.GetDoubleValue("CD1_1");
                    CD1_2 = hdu.Header.GetDoubleValue("CD1_2");
                    _isLookingEastLastCd = MountDataCollector.IsLookingEast;
                }
                catch
                {
                    
                }
                fits.Close();
            }

            if (CameraControl.isConnected & !_isObserve & !_isDoDarks & !_isDoFlats)
            {
                if (WeatherDataCollector.Flat &
                                ((DateTime.UtcNow - Settings.LastFlatsTime).TotalDays > TotalDays2ReMakeCalibrationFrames)) PrepareAndStartDoFlats();
                
                if (!WeatherDataCollector.Obs &
                                ((DateTime.UtcNow - Settings.LastDarksTime).TotalDays > TotalDays2ReMakeCalibrationFrames)) PrepareAndStartDoDarks();
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
                    }
                    // continue;
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

                //выбираем таску 
                if (_currentTask is null)
                {
                    _currentTask = bufTask;
                    continue;
                }

                if (bufTask.TimeStart < _currentTask.TimeStart) //выбираем таску с наименьшей датой начала наблюдения
                    //(вне зависимости от нынешнего времени)
                    _currentTask = bufTask;
            }
            
            //если после проверки таблицы не нашлась таска на выполнение, то уходим на следующую минуту
            if (_currentTask == null || _isObserve || _isDoDarks || _isDoFlats || !IsThinking)
            {
                ThinkingTimer.Start();
                return;
            }
            
            ThinkingTimer.Start();
            //а если нашлась и время до начала менее 5 минут, то стартуем 
            if ((_currentTask.TimeStart - DateTime.UtcNow).TotalMinutes < TotalMinutes2StartTask &
                !_isDoDarks & !_isDoFlats & !_isObserve & CameraControl.isConnected)
            {
                _currentTask.Status = 1;
                Tasker.UpdateTaskInTable(_currentTask);
                
                if (MountDataCollector.IsParked)
                {
                    
                    SiTechExeSocket.Unpark();
                }
                
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
        }

        private static void StartDoTest()
        {
            _isObserve = true;
            if (SiTechExeSocket.GoTo(_currentTask.Ra, _currentTask.Dec, true).Result)
            {
                if (CameraControl.PrepareToObs(_currentTask))
                {
                    Logger.AddLogEntry($"Start task# {_currentTask.TaskNumber}, type: {_currentTask.FrameType}");
                    _currentTask.Filters = CheckFil();
                    Tasker.UpdateTaskInTable(_currentTask);
                    CameraControl.StartExposure();
                    return;
                }
                else
                {
                    Logger.AddLogEntry($"WARNING: can't start task #{_currentTask.TaskNumber}, error while prepare to observe");
                }
            }
            else
            {
                Logger.AddLogEntry($"WARNING: can't start task #{_currentTask.TaskNumber}, error while GOTO");
            }
            EndTask(4);
        }

        public static void EndTask(short endStatus)
        {
            _currentTask.Status = endStatus;
            Tasker.UpdateTaskInTable(_currentTask);
            _isObserve = false;
            _isDoDarks = false;
            _isDoFlats = false;
            _isOnPause = false;
            _currentTask = null;
            _firstFrame = null;
            if (!MountDataCollector.IsParked)
            {
                SiTechExeSocket.Park();
            }
        }

        private static void StartDoLight()
        {   
            _isObserve = true;
            if (SiTechExeSocket.GoTo(_currentTask.Ra, _currentTask.Dec, true).Result) //проверять доехал ли
            {
                // _currentTask.Status = 1;
                Logger.AddLogEntry($"Start task# {_currentTask.TaskNumber}, type: {_currentTask.FrameType}");
                if (CameraControl.PrepareToObs(_currentTask))
                {
                    _currentTask.Filters = CheckFil();
                    Tasker.UpdateTaskInTable(_currentTask);
                    CameraControl.StartExposure();
                    //если доехал, то начинаем снимать 
                    return;
                }
                else
                {
                    Logger.AddLogEntry($"WARNING: can't start task #{_currentTask.TaskNumber}, error while prepare to observe");
                }
                //не удалось подготовиться к наблюдению  
                
            }
            else
            {
                Logger.AddLogEntry($"WARNING: can't start task #{_currentTask.TaskNumber}, error while GOTO");
            }
            EndTask(4);
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
                        // Guiding();
                        
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

                        if (!WeatherDataCollector.Obs)
                        {
                            Logger.AddLogEntry($"Weather is bad, pause task #{_currentTask.TaskNumber}");
                            _isOnPause = true;
                            // return;
                        }
                        else
                        {
                            if (CameraFocus.IsAutoFocus)
                            {
                                if (!fitsAnalysis.CheckFocused())
                                {
                                    CameraFocus.StartAutoFocus(_currentTask);
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
                        EndTask(_currentTask.DoneFrames < _currentTask.AllFrames ? (short) 5 : (short) 2);
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
                            Settings.LastDarksTime = DateTime.UtcNow;
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
            Tasker.AddTask(flatTask);
            _currentTask = flatTask;
            StartDoFlats();
        }

        private static void StartDoFlats()
        {
            if(WeatherDataCollector.Flat)
            {
                _isDoFlats = true;
                Logger.AddLogEntry($"Start task# {_currentTask.TaskNumber}, type: {_currentTask.FrameType}");
                if (SiTechExeSocket.GoTo(_currentTask.Ra, _currentTask.Dec, true).Result)
                {
                    if (CameraControl.PrepareToObs(_currentTask))
                    {
                        _currentTask.Status = 1;
                        _currentTask.Filters = CheckFil();
                        _currentTask.TimeStart = DateTime.UtcNow;
                        _currentTask.TimeEnd = DateTime.UtcNow.AddSeconds((short) (_currentTask.Exp + 50));
                        _currentTask.Duration = (float) Math.Round((_currentTask.TimeEnd - _currentTask.TimeStart).TotalHours, 2);
                        Tasker.UpdateTaskInTable(_currentTask);
                        CameraControl.StartExposure();
                        return; //не удалось подготовиться к наблюдению
                    }
                    else
                    {
                        Logger.AddLogEntry($"WARNING: can't start task #{_currentTask.TaskNumber}, error while prepare to observe");
                    }
                }
                else
                {
                    Logger.AddLogEntry($"WARNING: can't start task #{_currentTask.TaskNumber}, error while GOTO");
                }
            }
            else
            {
                Logger.AddLogEntry($"Can't start task# {_currentTask.TaskNumber}, type: {_currentTask.FrameType}, no dusk");
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
                Tasker.AddTask(darkTask);
                Darks.Add(darkTask);
            }
            _currentTask = Darks[0];
            Darks.RemoveAt(0);
            StartDoDark();
        }

        private static void StartDoDark()
        {
            _isDoDarks = true;
            Logger.AddLogEntry($"Start task# {_currentTask.TaskNumber}, type: {_currentTask.FrameType}");
            if (CameraControl.PrepareToObs(_currentTask))
            {
                 _currentTask.Status = 1;
                 _currentTask.Filters = CheckFil();
                 _currentTask.TimeStart = DateTime.UtcNow;
                 _currentTask.TimeEnd = DateTime.UtcNow.AddSeconds((short) (_currentTask.Exp + 50));
                 _currentTask.Duration = (float) Math.Round((_currentTask.TimeEnd - _currentTask.TimeStart).TotalHours, 2);
                 Tasker.UpdateTaskInTable(_currentTask);
                 CameraControl.StartExposure();
                 return;
            }
            else
            {
                Logger.AddLogEntry($"WARNING: can't start task #{_currentTask.TaskNumber}, error while prepare to observe");
            }


            if (Darks.Count>0)
            {
                _currentTask.Status = 4;
                Tasker.UpdateTaskInTable(_currentTask);
                _currentTask = Darks[0];
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