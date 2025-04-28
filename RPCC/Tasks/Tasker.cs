using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Windows.Forms;

namespace RPCC.Tasks
{
    public static class Tasker
    {
        private static readonly DataTable DataTable = new();
        private static readonly DataSet DataSet = new();
        public static DataGridView DataGridViewTasker;
        // private static readonly string FileName = Directory.GetCurrentDirectory() + "\\" + "Tasks.xml";
        public static ContextMenuStrip ContextMenuStripTasker;

        public static readonly string[] Header =
        {
            "N", "RaDecJ2000", "t_{Added}", @"t_{Run}", @"t_{Fin}", "Duration (h)",
            "Exp", "Done", "All", "t_{Last exp}", "Filter g", "Filter r", "Filter i", 
            "Object", "Object type", "Status", "Observer", "Frame type",
            "Xbin", "Ybin", "repoint_coords", "repoint_times", "Filter V"
        };
//gri

        public static void SetHeader()
        {
            // DataGridViewButtonColumn

            foreach (var s in Header) DataTable.Columns.Add(new DataColumn(s));

            DataSet.Tables.Add(DataTable);
            DataGridViewTasker.AutoSize = true;
            DataGridViewTasker.DataSource = DataSet.Tables[0];
            DataGridViewTasker.ContextMenuStrip = ContextMenuStripTasker;
            DataGridViewTasker.ReadOnly = true;
            DataGridViewTasker.AllowUserToAddRows = false;
            DataGridViewTasker.AllowUserToDeleteRows = false;

            DataGridViewTasker.Columns[0].DataPropertyName = "task_id";
            DataGridViewTasker.Columns[1].DataPropertyName = "coord2000";
            DataGridViewTasker.Columns[2].DataPropertyName = "time_add";
            DataGridViewTasker.Columns[3].DataPropertyName = "time_start";
            DataGridViewTasker.Columns[4].DataPropertyName = "time_end";
            DataGridViewTasker.Columns[5].DataPropertyName = "duration";
            DataGridViewTasker.Columns[6].DataPropertyName = "exp_time";
            DataGridViewTasker.Columns[7].DataPropertyName = "done_frames";
            DataGridViewTasker.Columns[8].DataPropertyName = "all_frames";
            DataGridViewTasker.Columns[9].DataPropertyName = "time_last_exp";
            DataGridViewTasker.Columns[10].DataPropertyName = "is_filter_g";
            DataGridViewTasker.Columns[11].DataPropertyName = "is_filter_r";
            DataGridViewTasker.Columns[12].DataPropertyName = "is_filter_i";
            DataGridViewTasker.Columns[13].DataPropertyName = "object_name";
            DataGridViewTasker.Columns[14].DataPropertyName = "object_type";
            DataGridViewTasker.Columns[15].DataPropertyName = "status";
            DataGridViewTasker.Columns[16].DataPropertyName = "observer";
            DataGridViewTasker.Columns[17].DataPropertyName = "frame_type";
            DataGridViewTasker.Columns[18].DataPropertyName = "x_bin";
            DataGridViewTasker.Columns[19].DataPropertyName = "y_bin";
            DataGridViewTasker.Columns[20].DataPropertyName = "repoint_coords";
            DataGridViewTasker.Columns[21].DataPropertyName = "repoint_times";
            DataGridViewTasker.Columns[22].DataPropertyName = "is_filter_v";
            DataGridViewTasker.Columns[0].Width = 60;
            DataGridViewTasker.Columns[1].Width = 120;
            DataGridViewTasker.Columns[2].Width = 80;
            DataGridViewTasker.Columns[3].Width = 80;
            DataGridViewTasker.Columns[4].Width = 80;
            DataGridViewTasker.Columns[5].Width = 40;
            DataGridViewTasker.Columns[6].Width = 30;
            DataGridViewTasker.Columns[7].Width = 30;
            DataGridViewTasker.Columns[8].Width = 30;
            DataGridViewTasker.Columns[9].Width = 80;
            DataGridViewTasker.Columns[10].Width = 40;
            DataGridViewTasker.Columns[11].Width = 40;
            DataGridViewTasker.Columns[12].Width = 40;
            DataGridViewTasker.Columns[13].Width = 60;
            DataGridViewTasker.Columns[14].Width = 60;
            DataGridViewTasker.Columns[15].Width = 30;
            DataGridViewTasker.Columns[16].Width = 80;
            DataGridViewTasker.Columns[17].Width = 40;
            DataGridViewTasker.Columns[18].Width = 20;
            DataGridViewTasker.Columns[19].Width = 20;
            DataGridViewTasker.Columns[20].Width = 60;
            DataGridViewTasker.Columns[21].Width = 60;
            DataGridViewTasker.Columns[22].Width = 40;
            
            foreach (DataGridViewColumn column in DataGridViewTasker.Columns)
                column.SortMode = DataGridViewColumnSortMode.NotSortable;

            // dataGridViewTasker.Sort(dataGridViewTasker.Columns[0], ListSortDirection.Descending);
            // dataGridViewTasker.Columns[0].DefaultCellStyle.Alignment = DataGridViewContentAlignment.TopCenter;
        }
        


        public static void PaintTable()
        {
            if (DataGridViewTasker.Rows.Count == 0)
            {
                return;
            }
            foreach (DataGridViewRow row in DataGridViewTasker.Rows)
                row.DefaultCellStyle.BackColor = short.Parse(row.Cells[15].Value.ToString()) switch
                {
                    0 => // Wait
                        Color.White,
                    1 => // In progress	
                        Color.CornflowerBlue,
                    2 => // Ended complete
                        Color.LightGreen,
                    3 => // Rejected by observer
                        Color.LightPink,
                    4 => // Not observed
                        Color.Gray,
                    5 => // Ended not complete
                        Color.Green,
                    _ => row.DefaultCellStyle.BackColor
                };
        }

        public static void DeleteTaskByRowIndex(int rowIndex)
        {
            DataGridViewTasker.Rows.RemoveAt(rowIndex);
        }

        public static ObservationTask GetTaskByRowIndex(int rowIndex)
        {
            var task = new ObservationTask();
            // var r = DataGridViewTasker.DataSource;
            var row = DataGridViewTasker.Rows[rowIndex];

            
            task.TaskNumber = Convert.ToInt32(row.Cells["N"].Value);
            if (!string.IsNullOrEmpty(row.Cells["RaDecJ2000"].Value.ToString()))
            {
                task.ComputeRaDec(row.Cells[1].Value.ToString());
            }
            task.TimeAdd = DateTime.Parse(row.Cells["t_{Added}"].Value.ToString());
            task.TimeStart = DateTime.Parse(row.Cells["t_{Run}"].Value.ToString());
            task.TimeEnd = DateTime.Parse(row.Cells["t_{Fin}"].Value.ToString());
            task.Duration = float.Parse(row.Cells["Duration (h)"].Value.ToString());
            task.Exp = Convert.ToInt16(row.Cells["Exp"].Value);
            task.DoneFrames = Convert.ToInt16(row.Cells["Done"].Value);
            task.AllFrames = Convert.ToInt16(row.Cells["All"].Value);
            var f = "";
            if ((bool)row.Cells["Filter g"].Value)
            {
                f += $"{StringHolder.FilG} ";
            }
            if ((bool)row.Cells["Filter V"].Value)
            {
                f += $"{StringHolder.FilV} ";
            }
            if ((bool)row.Cells["Filter r"].Value)
            {
                f += $"{StringHolder.FilR} ";
            }
            if ((bool)row.Cells["Filter i"].Value)
            {
                f += StringHolder.FilI;
            }
            
            task.Filters = f;
            task.Object = row.Cells["Object"].Value.ToString();
            task.ObjectType = row.Cells["Object type"].Value.ToString();
            task.Status = Convert.ToInt16(row.Cells["Status"].Value);
            task.Observer = row.Cells["Observer"].Value.ToString();
            task.FrameType = row.Cells["Frame type"].Value.ToString();
            task.Xbin = Convert.ToInt16(row.Cells["Xbin"].Value);
            task.Ybin = Convert.ToInt16(row.Cells["Ybin"].Value);

            task.RepointCoords = string.IsNullOrEmpty(row.Cells["repoint_coords"].Value.ToString()) ? 
                [] : row.Cells[20].Value as List<string>;
            task.RepointTimes = string.IsNullOrEmpty(row.Cells["repoint_times"].Value.ToString()) ? 
                [] : row.Cells[21].Value as List<DateTime>;
            return task;
        }
    }
}