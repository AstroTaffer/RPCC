using System;
using System.Data;
using System.IO;
using System.Windows.Forms;
using RPCC.Utils;

namespace RPCC.Tasks
{
    public static class Tasker
    {
        private static readonly DataTable DataTable = new DataTable();
        private static readonly DataSet DataSet = new DataSet();
        public static DataGridView dataGridViewTasker;
        public static Logger logger;
        private static readonly string FileName = Directory.GetCurrentDirectory() + "\\" + "Tasks.xml";
        public static ContextMenuStrip contextMenuStripTasker;

        private static readonly string[] Header = {
            "N", "RaDecJ2000", "t_{Added}", @"t_{Run}", @"t_{Fin}", "Dur (h)",
            "Exp", "Done", "All", "t_{Last exp}", "Filters", "Object", "Status", "Observer", "Frame type"
        };
//gri

        public static void SetHeader()
        {
            // DataGridViewButtonColumn
            
            foreach (var s in Header)
            {
                DataTable.Columns.Add(new DataColumn(s));
            }
            
            DataSet.Tables.Add(DataTable);
            dataGridViewTasker.AutoSize = true;
            dataGridViewTasker.DataSource = DataSet.Tables[0];
            dataGridViewTasker.ContextMenuStrip = contextMenuStripTasker;
            dataGridViewTasker.ReadOnly = true;
            dataGridViewTasker.AllowUserToAddRows = false;
            dataGridViewTasker.AllowUserToDeleteRows = false;
        }
        
        public static void SaveTasksToXml()
        {
            var dataTable = (DataTable) dataGridViewTasker.DataSource;
            try
            {
                dataTable.WriteXml(FileName);
        
            }
            catch (Exception exception)
            {
                logger.AddLogEntry(exception.Message);
            }
        }

        public static void LoadTasksFromXml()
        {
            if (File.Exists(FileName))
            {
                try
                {
                    DataSet.ReadXml(FileName);  
                    dataGridViewTasker.DataSource = DataSet.Tables[0];
                }
                catch (System.Xml.XmlException e)
                {
                    logger.AddLogEntry($"Tasker Error: {e.Message}");
                }
                
            }   
            else
            {
                File.Create(FileName);
                SaveTasksToXml();
            }
        }

        public static int GetTasksLen()
        {
            return dataGridViewTasker.Rows.Count;
        }
        
        public static void AddTask(ObservationTask task)
        {
            var dataRow = DataTable.NewRow();
            dataRow[Header[0]] = task.TaskNumber;
            dataRow[Header[1]] = task.RaDec;
            dataRow[Header[2]] = task.TimeAdd;
            dataRow[Header[3]] = task.TimeStart;
            dataRow[Header[4]] = task.TimeEnd;
            dataRow[Header[5]] = task.Duration;
            dataRow[Header[6]] = task.Exp;
            dataRow[Header[7]] = task.DoneFrames;
            dataRow[Header[8]] = task.AllFrames;
            dataRow[Header[9]] = task.TimeLastExp;
            dataRow[Header[10]] = task.Filters;
            dataRow[Header[11]] = task.Object;
            dataRow[Header[12]] = task.Status;
            dataRow[Header[13]] = task.Observer;
            dataRow[Header[14]] = task.FrameType;
            DataTable.Rows.Add(dataRow);
            SaveTasksToXml();
        }

        public static void EditTask()
        {
            
        }

        public static void DeleteTask(int rowIndex)
        {
            dataGridViewTasker.Rows.RemoveAt(rowIndex);
            SaveTasksToXml();
        }

        // public static ObservationTask GetTask(int taskN)
        // {
        //     // var getTask = ObservationTask();
        // }
    }
}