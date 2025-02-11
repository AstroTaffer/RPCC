﻿using System;
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
            "Xbin", "Ybin"
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

            DataGridViewTasker.Columns[0].Width = 60;
            DataGridViewTasker.Columns[1].Width = 180;
            DataGridViewTasker.Columns[2].Width = 120;
            DataGridViewTasker.Columns[3].Width = 120;
            DataGridViewTasker.Columns[4].Width = 120;
            DataGridViewTasker.Columns[5].Width = 60;
            DataGridViewTasker.Columns[6].Width = 60;
            DataGridViewTasker.Columns[7].Width = 60;
            DataGridViewTasker.Columns[8].Width = 60;
            DataGridViewTasker.Columns[9].Width = 120;
            DataGridViewTasker.Columns[10].Width = 60;
            DataGridViewTasker.Columns[11].Width = 60;
            DataGridViewTasker.Columns[12].Width = 60;
            DataGridViewTasker.Columns[13].Width = 120;
            DataGridViewTasker.Columns[14].Width = 120;
            DataGridViewTasker.Columns[15].Width = 60;
            DataGridViewTasker.Columns[16].Width = 120;
            DataGridViewTasker.Columns[17].Width = 80;
            DataGridViewTasker.Columns[18].Width = 40;
            DataGridViewTasker.Columns[19].Width = 40;

            foreach (DataGridViewColumn column in DataGridViewTasker.Columns)
                column.SortMode = DataGridViewColumnSortMode.NotSortable;

            // dataGridViewTasker.Sort(dataGridViewTasker.Columns[0], ListSortDirection.Descending);
            // dataGridViewTasker.Columns[0].DefaultCellStyle.Alignment = DataGridViewContentAlignment.TopCenter;
        }

        // public static void SaveTasksToXml()
        // {
        //     var dataTable = (DataTable) dataGridViewTasker.DataSource;
        //     try
        //     {
        //         dataTable.WriteXml(FileName);
        //     }
        //     catch (Exception exception)
        //     {
        //         Logger.AddLogEntry(exception.Message);
        //     }
        // }

        // public static void LoadTasksFromXml()
        // {
        //     if (File.Exists(FileName))
        //     {
        //         try
        //         {
        //             DataSet.ReadXml(FileName);
        //             dataGridViewTasker.DataSource = DataSet.Tables[0];
        //             PaintTable();
        //         }
        //         catch (XmlException e)
        //         {
        //             Logger.AddLogEntry($"Tasker Error: {e.Message}");
        //         }
        //     }
        //     else
        //     {
        //         File.Create(FileName);
        //         SaveTasksToXml();
        //     }
        // }


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

        // public static int GetTasksLen()
        // {
        //     return DataGridViewTasker.Rows.Count;
        // }

        // public static void AddTask(ObservationTask task)
        // {
        //     var dataRow = DataTable.NewRow();
        //     dataRow[Header[0]] = task.TaskNumber;
        //     dataRow[Header[1]] = task.RaDec;
        //     dataRow[Header[2]] = task.TimeAdd;
        //     dataRow[Header[3]] = task.TimeStart;
        //     dataRow[Header[4]] = task.TimeEnd;
        //     dataRow[Header[5]] = task.Duration;
        //     dataRow[Header[6]] = task.Exp;
        //     dataRow[Header[7]] = task.DoneFrames;
        //     dataRow[Header[8]] = task.AllFrames;
        //     dataRow[Header[9]] = task.TimeLastExp;
        //     dataRow[Header[10]] = task.Filters.Contains("g");
        //     dataRow[Header[11]] = task.Filters.Contains("r");
        //     dataRow[Header[12]] = task.Filters.Contains("i");
        //     dataRow[Header[13]] = task.Object;
        //     dataRow[Header[14]] = task.ObjectType;
        //     dataRow[Header[15]] = task.Status;
        //     dataRow[Header[16]] = task.Observer;
        //     dataRow[Header[17]] = task.FrameType;
        //     dataRow[Header[18]] = task.Xbin;
        //     dataRow[Header[19]] = task.Ybin;
        //
        //     DataTable.Rows.InsertAt(dataRow, 0);
        //     // SaveTasksToXml();
        // }

        // public static void UpdateTaskInTable(ObservationTask task)
        // {
        //     foreach (DataGridViewRow row in DataGridViewTasker.Rows)
        //     {
        //         if (!string.Equals(row.Cells[0].Value.ToString(), task.TaskNumber.ToString())) continue;
        //         row.Cells[0].Value = task.TaskNumber;
        //         row.Cells[1].Value = task.RaDec;
        //         row.Cells[2].Value = task.TimeAdd;
        //         row.Cells[3].Value = task.TimeStart;
        //         row.Cells[4].Value = task.TimeEnd;
        //         row.Cells[5].Value = Math.Round(task.Duration, 2);
        //         row.Cells[6].Value = task.Exp;
        //         row.Cells[7].Value = task.DoneFrames;
        //         row.Cells[8].Value = task.AllFrames;
        //         row.Cells[9].Value = task.TimeLastExp;
        //         row.Cells[10].Value = task.Filters.Contains("g");
        //         row.Cells[11].Value = task.Filters.Contains("r");
        //         row.Cells[12].Value = task.Filters.Contains("i");
        //         row.Cells[13].Value = task.Object;
        //         row.Cells[14].Value = task.ObjectType;
        //         row.Cells[15].Value = task.Status;
        //         row.Cells[16].Value = task.Observer;
        //         row.Cells[17].Value = task.FrameType;
        //         row.Cells[18].Value = task.Xbin;
        //         row.Cells[19].Value = task.Ybin;
        //     }
        //     PaintTable();
        //     // SaveTasksToXml();
        // }

        public static void DeleteTaskByRowIndex(int rowIndex)
        {
            DataGridViewTasker.Rows.RemoveAt(rowIndex);
            // SaveTasksToXml();
        }

        // public static ObservationTask GetTaskByNumber(string taskNumber)
        // {
        //     var task = new ObservationTask();
        //     foreach (DataGridViewRow row in DataGridViewTasker.Rows)
        //     {
        //         if (!string.Equals(row.Cells[0].Value.ToString(), taskNumber)) continue;
        //         task.TaskNumber = Convert.ToInt32(row.Cells[0].Value);
        //         if (!string.IsNullOrEmpty(row.Cells[1].Value.ToString()))
        //         {
        //             task.ComputeRaDec(row.Cells[1].Value.ToString());
        //         }
        //         task.TimeAdd = DateTime.Parse(row.Cells[2].Value.ToString());
        //         task.TimeStart = DateTime.Parse(row.Cells[3].Value.ToString());
        //         task.TimeEnd = DateTime.Parse(row.Cells[4].Value.ToString());
        //         task.Duration = float.Parse(row.Cells[5].Value.ToString());
        //         task.Exp = Convert.ToInt16(row.Cells[6].Value);
        //         task.DoneFrames = Convert.ToInt16(row.Cells[7].Value);
        //         task.AllFrames = Convert.ToInt16(row.Cells[8].Value);
        //         task.TimeLastExp = (string.IsNullOrEmpty(row.Cells[9].Value.ToString()) ? 
        //             new DateTime() : DateTime.Parse(row.Cells[9].Value.ToString()));
        //         var f = "";
        //         if ((bool)row.Cells[10].Value)
        //         {
        //             f += "g ";
        //         }
        //         if ((bool)row.Cells[11].Value)
        //         {
        //             f += "r ";
        //         }
        //         if ((bool)row.Cells[12].Value)
        //         {
        //             f += "i";
        //         }
        //         task.Filters = f;
        //         task.Object = row.Cells[13].Value.ToString();
        //         task.ObjectType = row.Cells[14].Value.ToString();
        //         task.Status = Convert.ToInt16(row.Cells[15].Value);
        //         task.Observer = row.Cells[16].Value.ToString();
        //         task.FrameType = row.Cells[17].Value.ToString();
        //         task.Xbin = Convert.ToInt16(row.Cells[18].Value);
        //         task.Ybin = Convert.ToInt16(row.Cells[19].Value);
        //         break;
        //     }
        //
        //     return task;
        // }
        //
        // public static void UpdateTaskFromTable(ObservationTask task)
        // {
        //     foreach (DataGridViewRow row in DataGridViewTasker.Rows)
        //     {
        //         if (!Equals(row.Cells[0].Value, task.TaskNumber)) continue;
        //         task.TaskNumber = Convert.ToInt32(row.Cells[0].Value);
        //         if (!string.IsNullOrEmpty(row.Cells[1].Value.ToString()))
        //         {
        //             task.ComputeRaDec(row.Cells[1].Value.ToString());
        //         }
        //         task.TimeAdd = DateTime.Parse(row.Cells[2].Value.ToString());
        //         task.TimeStart = DateTime.Parse(row.Cells[3].Value.ToString());
        //         task.TimeEnd = DateTime.Parse(row.Cells[4].Value.ToString());
        //         task.Duration = float.Parse(row.Cells[5].Value.ToString());
        //         task.Exp = Convert.ToInt16(row.Cells[6].Value);
        //         task.DoneFrames = Convert.ToInt16(row.Cells[7].Value);
        //         task.AllFrames = Convert.ToInt16(row.Cells[8].Value);
        //         task.TimeLastExp = (string.IsNullOrEmpty(row.Cells[9].Value.ToString()) ? 
        //             new DateTime() : DateTime.Parse(row.Cells[9].Value.ToString()));
        //         
        //         var f = "";
        //         if ((bool)row.Cells[10].Value)
        //         {
        //             f += "g ";
        //         }
        //         if ((bool)row.Cells[11].Value)
        //         {
        //             f += "r ";
        //         }
        //         if ((bool)row.Cells[12].Value)
        //         {
        //             f += "i";
        //         }
        //         task.Filters = f;
        //         task.Object = row.Cells[13].Value.ToString();
        //         task.ObjectType = row.Cells[14].Value.ToString();
        //         task.Status = Convert.ToInt16(row.Cells[15].Value);
        //         task.Observer = row.Cells[16].Value.ToString();
        //         task.FrameType = row.Cells[17].Value.ToString();
        //         task.Xbin = Convert.ToInt16(row.Cells[18].Value);
        //         task.Ybin = Convert.ToInt16(row.Cells[19].Value);
        //         break;
        //     }
        // }

        public static ObservationTask GetTaskByRowIndex(int rowIndex)
        {
            var task = new ObservationTask();
            var row = DataGridViewTasker.Rows[rowIndex];
            task.TaskNumber = Convert.ToInt32(row.Cells[0].Value);
            if (!string.IsNullOrEmpty(row.Cells[1].Value.ToString()))
            {
                task.ComputeRaDec(row.Cells[1].Value.ToString());
            }
            task.TimeAdd = DateTime.Parse(row.Cells[2].Value.ToString());
            task.TimeStart = DateTime.Parse(row.Cells[3].Value.ToString());
            task.TimeEnd = DateTime.Parse(row.Cells[4].Value.ToString());
            task.Duration = float.Parse(row.Cells[5].Value.ToString());
            task.Exp = Convert.ToInt16(row.Cells[6].Value);
            task.DoneFrames = Convert.ToInt16(row.Cells[7].Value);
            task.AllFrames = Convert.ToInt16(row.Cells[8].Value);
            var f = "";
            if ((bool)row.Cells[10].Value)
            {
                f += "g ";
            }
            if ((bool)row.Cells[11].Value)
            {
                f += "r ";
            }
            if ((bool)row.Cells[12].Value)
            {
                f += "i";
            }
            task.Filters = f;
            task.Object = DataGridViewTasker.Rows[rowIndex].Cells[13].Value.ToString();
            task.ObjectType = DataGridViewTasker.Rows[rowIndex].Cells[14].Value.ToString();
            task.Status = Convert.ToInt16(row.Cells[15].Value);
            task.Observer = row.Cells[16].Value.ToString();
            task.FrameType = row.Cells[17].Value.ToString();
            task.Xbin = Convert.ToInt16(row.Cells[18].Value);
            task.Ybin = Convert.ToInt16(row.Cells[19].Value);

            return task;
        }

        public static void GetTaskFromRow(DataRow row, ref ObservationTask task)
        {
            task.TaskNumber = Convert.ToInt32(row.ItemArray[0]);
            if (!string.IsNullOrEmpty(row.ItemArray[1].ToString()))
            {
                task.ComputeRaDec(row.ItemArray[1].ToString());
            }
            task.TimeAdd = DateTime.Parse(row.ItemArray[2].ToString());
            task.TimeStart = DateTime.Parse(row.ItemArray[3].ToString());
            task.TimeEnd = DateTime.Parse(row.ItemArray[4].ToString());
            task.Duration = float.Parse(row.ItemArray[5].ToString());
            task.Exp = Convert.ToInt16(row.ItemArray[6]);
            task.DoneFrames = Convert.ToInt16(row.ItemArray[7]);
            task.AllFrames = Convert.ToInt16(row.ItemArray[8]);
            var f = "";
            if ((bool)row.ItemArray[10])
            {
                f += "g ";
            }
            if ((bool)row.ItemArray[11])
            {
                f += "r ";
            }
            if ((bool)row.ItemArray[12])
            {
                f += "i";
            }
            task.Filters = f;
            task.Object = row.ItemArray[13].ToString();
            task.ObjectType = row.ItemArray[14].ToString();
            task.Status = Convert.ToInt16(row.ItemArray[15]);
            task.Observer = row.ItemArray[16].ToString();
            task.FrameType = row.ItemArray[17].ToString();
            task.Xbin = Convert.ToInt16(row.ItemArray[18]);
            task.Ybin = Convert.ToInt16(row.ItemArray[19]);

            task.RepointCoords = row.ItemArray[20] as List<string>;
            task.RepointTimes = row.ItemArray[21] as List<DateTime>;
        }
    }
}