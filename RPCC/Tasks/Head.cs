using System;
using System.Data;
using System.Timers;
using System.Windows.Forms;
using Timer = System.Timers.Timer;

namespace RPCC.Tasks
{
    public static class Head
    {
        private static readonly Timer WorkTimer = new Timer();
        private static string ActualTaskNumber;
        private static ObservationTask ActualTask;
        private const short SearchNumber = 10;
        private const short TotalMinutes2StartTask = 5;
        private static bool isObserve;

        public static void StartThinking()
        {
            WorkTimer.Elapsed += Thinking;
            WorkTimer.Interval = 60000;
            WorkTimer.Start();
        }   
        
        private static void Thinking(object sender, ElapsedEventArgs e)
        {
            if (!isObserve)
            {
                var bufTask = new ObservationTask();
                foreach (DataGridViewRow row in Tasker.dataGridViewTasker.Rows)
                {
                    bufTask = Tasker.GetTaskByNumber(row.Cells[0].Value.ToString());
                    if (bufTask.Status>0) continue;
                    if (ActualTask is null)
                    {
                        ActualTask = bufTask;
                        continue;
                    }
                    
                    if (bufTask.TimeStart<ActualTask.TimeStart)
                    {
                        ActualTask = bufTask;
                    }
                }

                if (ActualTask == null) return;
                if ((ActualTask.TimeStart - DateTime.UtcNow).TotalMinutes < TotalMinutes2StartTask)
                {
                    StartTask(ActualTask);
                }
            }
            
        }
        
        private static void StartTask(ObservationTask task)
        {
            task.Status = 1;
            Tasker.UpdateTaskInTable(task);
        }
        
    }
}