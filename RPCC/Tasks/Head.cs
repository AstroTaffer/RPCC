using System;
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
        private static DateTime _lastDarksTime; //TODO add in .cfg
        private static DateTime _lastFlatsTime; //TODO add in .cfg
        private static readonly short[] DarkExps = {2, 5, 10, 15, 20, 30, 50, 80, 120, 180};

        //TODO ADD PAUSE 
        
        public static void StartThinking()
        {
            ThinkingTimer.Elapsed += Thinking;
            ThinkingTimer.Interval = 60000; //думать раз в минуту
            ThinkingTimer.Start();
        }

        private static void Thinking(object sender, ElapsedEventArgs e)
        {
            if (!CameraControl.isConnected & WeatherDataCollector.Sun >= 90)
            {
                CameraControl.ReconnectCameras();
            }

            if (CameraControl.isConnected & WeatherDataCollector.Sun < 89)
            {
                CameraControl.DisconnectCameras();
            }

            if (_isOnPause & WeatherDataCollector.Obs)
            {
                _isOnPause = false;
                
                //doExp();
            }
            
            if (!_isObserve & !_isDoDarks & !_isDoFlats & WeatherDataCollector.Flat &
                ((DateTime.UtcNow - _lastFlatsTime).TotalDays > TotalDays2ReMakeCalibrationFrames)) StartDoFlats();

            if (!_isObserve & !_isDoDarks & !_isDoFlats & !WeatherDataCollector.Obs &
                ((DateTime.UtcNow - _lastDarksTime).TotalDays > TotalDays2ReMakeCalibrationFrames)) StartDoDarks();

            foreach (DataGridViewRow row in Tasker.dataGridViewTasker.Rows)
            {
                var bufTask = Tasker.GetTaskByNumber(row.Cells[0].Value.ToString());
                if (bufTask.Status > 0) continue; // если не ждет наблюдения, то идем дальше

                // если время конца таски уже прошло, но она все еще не начата, или тип кадра не указан
                // то ставим статус пролюблена
                if (bufTask.TimeEnd < DateTime.UtcNow || string.IsNullOrEmpty(bufTask.FrameType))
                {
                    if (bufTask.Status == 1)
                    {
                        EndTask(_currentTask.DoneFrames < _currentTask.AllFrames ? (short) 5 : (short) 2, _currentTask);
                    }
                    else
                    {
                        bufTask.Status = 4;
                        Tasker.UpdateTaskInTable(bufTask);
                    }
                    continue;
                }

                // Далее выбираем таску для наблюдения 
                if (!WeatherDataCollector.Obs) continue; // если нельзя наблюдать, то ждем минуту
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
            if (_currentTask == null) return;
            //а если нашлась и время до начала менее 5 минут, то стартуем 
            if ((_currentTask.TimeStart - DateTime.UtcNow).TotalMinutes < TotalMinutes2StartTask) StartTask();
        }

        private static void StartTask()
        {
            Tasker.UpdateTaskInTable(_currentTask);
            if (_currentTask.FrameType == "light") StartDoLight();
            if (_currentTask.FrameType == "dark") StartDoDark(_currentTask);
            if (_currentTask.FrameType == "flat") StartDoFlats(_currentTask);
        }

        private static void EndTask(short endStatus, ObservationTask task)
        {
            task.Status = endStatus;
            Tasker.UpdateTaskInTable(task);
            _isObserve = false;
            _isDoDarks = false;
            _isDoFlats = false;
            _isOnPause = false;
            _currentTask = null;
        }

        private static void StartDoLight()
        {   
            _isObserve = true;
            SiTechExeSocket.GoTo(_currentTask.Ra, _currentTask.Dec, true); //проверять доехал ли
            //если доехал, то начинаем снимать 
            if (!CameraControl.PrepareToObs(_currentTask)) return;
            _currentTask.Status = 1;
            Logger.AddLogEntry($"Start task# {_currentTask.TaskNumber}, type: {_currentTask.FrameType}");
            CameraControl.PrepareToObs(_currentTask);
            CameraControl.Expose();
        }

        public static void CamCallback(string fitsPath)
        {
            if (_currentTask.TimeEnd > DateTime.UtcNow)
            {
                Tasker.UpdateTaskInTable(_currentTask);
                var fitsAnalysis = new GetDataFromFits(fitsPath);
                if (fitsAnalysis.CheckFocused() || !CameraFocus.IsAutoFocus)
                {
                    if (WeatherDataCollector.Obs)
                    {
                        //TODO doExp()
                    }
                    else
                    {
                        _isOnPause = true;
                    }
                }
                else
                {
                    CameraFocus.StartAutoFocus(_currentTask);
                }
                
                //asinc doAstrometry() 
                //asinc doDonuts()
                return;
            }

            EndTask(_currentTask.DoneFrames < _currentTask.AllFrames ? (short) 5 : (short) 2, _currentTask);
        }

        #region Flats

        private static void StartDoFlats()
        {
            var flatTask = new ObservationTask();
            flatTask.Exp = FlatExp;
            flatTask.TaskNumber = Tasker.GetTasksLen();
            flatTask.TimeAdd = DateTime.UtcNow;
            flatTask.TimeStart = DateTime.UtcNow;
            flatTask.TimeEnd = DateTime.UtcNow.AddMinutes(FlatExp * FlatDarkQuantity / 6.0);
            flatTask.AllFrames = FlatDarkQuantity;
            flatTask.Duration = (float) (flatTask.TimeEnd - flatTask.TimeStart).TotalHours;
            flatTask.Status = 1;
            flatTask.FrameType = "flat";
            var zenRaDec = SiTechExeSocket.GoToAltAz(180, 90);
            flatTask.ComputeRaDec($"{Utilities.HoursToHMS(zenRaDec[0])} " +
                                  $"{Utilities.DegreesToDMS(zenRaDec[1])}");
            Tasker.AddTask(flatTask);
            StartDoFlats(flatTask);
            _lastFlatsTime = DateTime.UtcNow;
        }

        private static void StartDoFlats(ObservationTask task)
        {
            Logger.AddLogEntry($"Start task# {task.TaskNumber}, type: {task.FrameType}");
            _isDoFlats = true;
            //TODO FLAT
            SiTechExeSocket.GoTo(task.Ra, task.Dec, true); //проверять доехал ли
            CameraControl.PrepareToObs(task);
            // начать делать экспозиции (автофокус выкл)
        }

        #endregion

        #region Dark

        private static void StartDoDarks()
        {
            foreach (var exp in DarkExps)
            {
                var darkTask = new ObservationTask();
                darkTask.TaskNumber = Tasker.GetTasksLen();
                darkTask.FrameType = "dark";
                darkTask.Exp = exp;
                darkTask.Status = 1;
                darkTask.AllFrames = FlatDarkQuantity;
                darkTask.TimeAdd = DateTime.UtcNow;
                darkTask.TimeStart = DateTime.UtcNow;
                darkTask.TimeEnd = darkTask.TimeStart.AddMinutes((short) (FlatDarkQuantity * exp / 60) + 3);
                darkTask.Duration = (float) (darkTask.TimeEnd - darkTask.TimeStart).TotalHours;
                Tasker.AddTask(darkTask);

                StartDoDark(darkTask);
            }

            _lastDarksTime = DateTime.UtcNow;
        }

        private static void StartDoDark(ObservationTask task)
        {
            Logger.AddLogEntry($"Start task# {task.TaskNumber}, type: {task.FrameType}");
            _isDoDarks = true;
            CameraControl.PrepareToObs(task);
            //TODO DARK
            // начать делать экспозиции (автофокус выкл)
        }

        #endregion
    }
}