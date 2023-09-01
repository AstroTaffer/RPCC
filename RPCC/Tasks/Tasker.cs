using System;
using System.Data;
using System.Drawing;
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
            "N", "RaDecJ2000", "t_{Added}", @"t_{Run}", @"t_{Fin}", "Duration",
            "Exp", "Done", "All", "t_{Last exp}", "Filters", "Object", "Status", "Observer", "Frame type", 
            "Xbin", "XSubframeStart", "XSubframeEnd", "Ybin", "YSubframeStart", "YSubframeEnd"
            
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
            
            dataGridViewTasker.Columns[0].Width = 60;
            dataGridViewTasker.Columns[1].Width = 150;
            dataGridViewTasker.Columns[2].Width = 120;
            dataGridViewTasker.Columns[3].Width = 120;
            dataGridViewTasker.Columns[4].Width = 120;
            dataGridViewTasker.Columns[5].Width = 60;
            dataGridViewTasker.Columns[6].Width = 60;
            dataGridViewTasker.Columns[7].Width = 60;
            dataGridViewTasker.Columns[8].Width = 60;
            dataGridViewTasker.Columns[9].Width = 120;
            dataGridViewTasker.Columns[10].Width = 60;
            dataGridViewTasker.Columns[11].Width = 120;
            dataGridViewTasker.Columns[12].Width = 60;
            dataGridViewTasker.Columns[13].Width = 120;
            dataGridViewTasker.Columns[14].Width = 80;

            dataGridViewTasker.Columns[15].Visible = false;
            dataGridViewTasker.Columns[16].Visible = false;
            dataGridViewTasker.Columns[17].Visible = false;
            dataGridViewTasker.Columns[18].Visible = false;
            dataGridViewTasker.Columns[19].Visible = false;
            dataGridViewTasker.Columns[20].Visible = false;
            
            foreach (DataGridViewColumn column in dataGridViewTasker.Columns)
            {
                column.SortMode = DataGridViewColumnSortMode.NotSortable;
            }

            // dataGridViewTasker.Sort(dataGridViewTasker.Columns[0], ListSortDirection.Descending);
            // dataGridViewTasker.Columns[0].DefaultCellStyle.Alignment = DataGridViewContentAlignment.TopCenter;
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
                    PaintTable();
                    
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
        

        public static void PaintTable()
        {
            foreach (DataGridViewRow row in dataGridViewTasker.Rows)
            {
                switch (short.Parse(row.Cells[12].Value.ToString()))
                {
                    case 0: // Wait
                        row.DefaultCellStyle.BackColor = Color.White;
                        break;
                    case 1: // In progress	
                        row.DefaultCellStyle.BackColor = Color.CornflowerBlue;
                        break;
                    case 2: // Ended complete
                        row.DefaultCellStyle.BackColor = Color.LightGreen;
                        break;
                    case 3: // Rejected by observer
                        row.DefaultCellStyle.BackColor = Color.LightPink;
                        break;
                    case 4: // Not observed
                        row.DefaultCellStyle.BackColor = Color.Gray;
                        break;
                    case 5: // Ended not complete
                        row.DefaultCellStyle.BackColor = Color.Green;
                        break;
                }
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
            dataRow[Header[15]] = task.Xbin;
            dataRow[Header[16]] = task.XSubframeStart;
            dataRow[Header[17]] = task.XSubframeEnd;
            dataRow[Header[18]] = task.Ybin;
            dataRow[Header[19]] = task.YSubframeStart;
            dataRow[Header[20]] = task.YSubframeEnd;
            
            DataTable.Rows.InsertAt(dataRow, 0);
            SaveTasksToXml();
        }

        public static void DeleteTask(int rowIndex)
        {
            dataGridViewTasker.Rows.RemoveAt(rowIndex);
            SaveTasksToXml();
        }

        public static ObservationTask GetTask(int rowIndex)
        {
            var task = new ObservationTask();
            task.TaskNumber = Convert.ToInt32(dataGridViewTasker.Rows[rowIndex].Cells[0].Value);
            task.RaDec = dataGridViewTasker.Rows[rowIndex].Cells[1].Value.ToString();
            task.TimeAdd = DateTime.Parse(dataGridViewTasker.Rows[rowIndex].Cells[2].Value.ToString());
            task.TimeStart = DateTime.Parse(dataGridViewTasker.Rows[rowIndex].Cells[3].Value.ToString());
            task.TimeEnd = DateTime.Parse(dataGridViewTasker.Rows[rowIndex].Cells[4].Value.ToString());
            task.Duration = float.Parse(dataGridViewTasker.Rows[rowIndex].Cells[5].Value.ToString());
            task.Exp = Convert.ToInt16(dataGridViewTasker.Rows[rowIndex].Cells[6].Value);
            task.AllFrames = Convert.ToInt16(dataGridViewTasker.Rows[rowIndex].Cells[8].Value);
            task.Filters = dataGridViewTasker.Rows[rowIndex].Cells[10].Value.ToString();
            task.Object = dataGridViewTasker.Rows[rowIndex].Cells[11].Value.ToString();
            task.Status = Convert.ToInt16(dataGridViewTasker.Rows[rowIndex].Cells[12].Value);
            task.Observer = dataGridViewTasker.Rows[rowIndex].Cells[13].Value.ToString();
            task.FrameType = dataGridViewTasker.Rows[rowIndex].Cells[14].Value.ToString();
            task.Xbin = Convert.ToInt16(dataGridViewTasker.Rows[rowIndex].Cells[15].Value);
            task.XSubframeStart = Convert.ToInt16(dataGridViewTasker.Rows[rowIndex].Cells[16].Value);
            task.XSubframeEnd = Convert.ToInt16(dataGridViewTasker.Rows[rowIndex].Cells[17].Value);
            task.Ybin = Convert.ToInt16(dataGridViewTasker.Rows[rowIndex].Cells[18].Value);
            task.YSubframeStart = Convert.ToInt16(dataGridViewTasker.Rows[rowIndex].Cells[19].Value);
            task.YSubframeEnd = Convert.ToInt16(dataGridViewTasker.Rows[rowIndex].Cells[20].Value);

            return task;
        }
    }
}