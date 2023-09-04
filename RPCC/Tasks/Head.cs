using System;
using System.Timers;
using System.Windows.Forms;
using RPCC.Comms;
using RPCC.Utils;
using Timer = System.Timers.Timer;

namespace RPCC.Tasks
{
    public static class Head
    {
        private static readonly Timer ThinkingTimer = new Timer();
        private static string ActualTaskNumber;
        private static ObservationTask ActualTask;
        private const short TotalMinutes2StartTask = 5; //TODO add in .cfg
        private const short TotalDays2ReMakeCalibrationFrames = 30; //TODO add in .cfg
        private static bool isObserve;
        private static bool isDoFlats;
        private static bool isDoDarks;
        private static DateTime lastDarksTime; //TODO add in .cfg
        private static DateTime lastFlatsTime; //TODO add in .cfg
        
        public static void StartThinking()
        {
            ThinkingTimer.Elapsed += Thinking;
            ThinkingTimer.Interval = 60000; //думать раз в минуту
            ThinkingTimer.Start();
        }   
        
        private static void Thinking(object sender, ElapsedEventArgs e)
        {
            if (WeatherDataCollector.Sun >= 90)
            {
                //start cum + условие на работу камеры
            }
            else
            {
                //end cum
            }
            
            if (!isObserve & !isDoDarks & !isDoFlats & WeatherDataCollector.Flat &
                (DateTime.UtcNow - lastFlatsTime).TotalDays > TotalDays2ReMakeCalibrationFrames) StartDoFlats();
            
            if (!isObserve & !isDoDarks & !isDoFlats & !WeatherDataCollector.Obs &
                (DateTime.UtcNow - lastDarksTime).TotalDays > TotalDays2ReMakeCalibrationFrames) StartDoDarks();

            foreach (DataGridViewRow row in Tasker.dataGridViewTasker.Rows)
            {
                var bufTask = Tasker.GetTaskByNumber(row.Cells[0].Value.ToString());
                if (bufTask.Status>0) continue; // если не ждет наблюдения, то идем дальше
                
                // если время конца таски уже прошло, но она все еще не начата, или тип кадра не указан
                // то ставим статус пролюблена
                if (bufTask.TimeEnd < DateTime.UtcNow || string.IsNullOrEmpty(bufTask.FrameType)) 
                {
                    bufTask.Status = 4;
                    Tasker.UpdateTaskInTable(bufTask);
                    continue;
                }
                
                // Далее выбираем таску для наблюдения 
                if (!WeatherDataCollector.Obs) continue; // если нельзя наблюдать, то ждем минуту
                if (isObserve || isDoDarks || isDoFlats) continue; // если уже идет задание, то ждем минуту
                
                //выбираем таску 
                if (ActualTask is null)
                {
                    ActualTask = bufTask;
                    continue;
                }
                
                if (bufTask.TimeStart<ActualTask.TimeStart) //выбираем таску с наименьшей датой начала наблюдения
                                                            //(вне зависимости от нынешнего времени)
                {
                    ActualTask = bufTask;
                }
            }

            //если после проверки таблицы не нашлась таска на выполнение, то уходим на следующую минуту
            if (ActualTask == null) return;
            //а если нашлась и время до начала менее 5 минут, то стартуем 
            if ((ActualTask.TimeStart - DateTime.UtcNow).TotalMinutes < TotalMinutes2StartTask)
            {
                StartTask();
            }
        }
        
        private static void StartTask()
        {
            ActualTask.Status = 1;
            Tasker.UpdateTaskInTable(ActualTask);
            if (ActualTask.FrameType == "light") StartDoLight();
            if (ActualTask.FrameType == "dark") StartDoDarks(ActualTask);
            if (ActualTask.FrameType == "flat") StartDoFlats(ActualTask);
        }

        private static void EndTask(short endStatus, ObservationTask task)
        {
            task.Status = endStatus;
            Tasker.UpdateTaskInTable(task);
            isObserve = false;
            isDoDarks = false;
            isDoFlats = false;
            ActualTask = null;
        }
    
        private static void StartDoLight()
        {
            Logger.AddLogEntry($"Start task# {ActualTask.TaskNumber}, type: {ActualTask.FrameType}");
            isObserve = true;
            
            // SiTechExeSocket.GoTo();
            // навести монтировку в нужную точку (проверка на доступность)
            // сфокусироваться
            // начать делать экспозиции (автофокус опционален)
        }

        #region Flats

            private static void StartDoFlats()
            {
                //TODO create flat task
                var flatTask = new ObservationTask();
                StartDoFlats(flatTask);
                lastFlatsTime = DateTime.UtcNow;
            }
            private static void StartDoFlats(ObservationTask task)
            {
                Logger.AddLogEntry($"Start task# {task.TaskNumber}, type: {task.FrameType}");
                isDoFlats = true;
                //TODO FLAT
                // навести монтировку в зенит
                // начать делать экспозиции (автофокус выкл)
            }

        #endregion
        
        #region Dark

            private static void StartDoDarks()
            {
                //TODO create dark task
                var darkTask = new ObservationTask();
                StartDoDarks(darkTask);
                lastDarksTime = DateTime.UtcNow;
            }
            
            private static void StartDoDarks(ObservationTask task)
            {
                Logger.AddLogEntry($"Start task# {task.TaskNumber}, type: {task.FrameType}");
                isDoDarks = true;
                //TODO DARK
                // начать делать экспозиции (автофокус выкл)
            }

        #endregion

    }
}