using System;
using System.Data;
using System.IO;
using System.Windows.Forms;
using RPCC.Utils;

namespace RPCC
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
            "N", "α, δ J2000", "t_{Added}", @"t_{Run}", @"t_{Fin}", 
            "Exp", "Done/All", "t_{Last exp}", "Filters", "Object", "Status", "Observer", "Frame type"
        };
//gri

        public static void SetHeader()
        {
            foreach (var s in Header)
            {
                DataTable.Columns.Add(new DataColumn(s));
            }

            DataSet.Tables.Add(DataTable);
            dataGridViewTasker.AutoSize = true;
            dataGridViewTasker.DataSource = DataSet.Tables[0];
            dataGridViewTasker.ContextMenuStrip = contextMenuStripTasker;
            dataGridViewTasker.Rows[0].ContextMenuStrip = contextMenuStripTasker;
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
                    foreach (DataGridViewRow row in dataGridViewTasker.Rows)
                    {
                        row.ContextMenuStrip = contextMenuStripTasker;
                    }
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

        public static void AddTask()
        {
            var dataRow = DataTable.NewRow();
            dataRow[Header[0]] = "1";
            dataRow[Header[1]] = "10:10:10.20 -20.20.20.20";
            DataTable.Rows.Add(dataRow);
            // DataTable;
            // dataGridViewTasker.Rows.Add(new object[] {"1", "20:20:20.20 -20:20:20.20"});
            // dataGridViewTasker.DataSource = dataGridViewTasker.
        }

        public static void EditTask()
        {
            
        }
    }
}