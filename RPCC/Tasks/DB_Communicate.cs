using System;
using System.Data;
using System.Windows.Forms;
using Npgsql;
using RPCC.Utils;


namespace RPCC.Tasks
{
    public static class DbCommunicate
    {
        // public static string RoboPhotServer = "localhost"; // TODO to cfg
        public static string RoboPhotServer = "192.168.240.5";
        public static string Port = "5432";
        public static string UserId = "remote_user";
        public static string Password = "remote_user";
        public static string Database = "postgres";
        public static NpgsqlConnection Con;
        private static readonly object Loc = new object();
        
        public static bool ConnectToDb()
        {
            try
            {
                lock (Loc)
                {
                    var connString =
                        $"Server={RoboPhotServer};Port={Port};User Id={UserId};Password={Password}; Database={Database};";
                    Con = new NpgsqlConnection(connString);
                    Con.Open();
                }

            }
            catch (Exception e)
            {
                Logger.AddLogEntry(e.Message);
                return false;
            }
            return true;
        }

        public static void DisconnectFromDb()
        {
            lock (Loc)
            {
                Con.Close();
            }
        }

        public static void LoadDbTable()
        {
            lock (Loc)
            {
                ConnectToDb();
                const string query = "SELECT task_id, get_hms_dms(start_coord2000) as coord2000, time_add, time_start, time_end, " +
                                     "duration, exp_time, done_frames, all_frames, time_last_exp, " +
                                     "is_filter_g, is_filter_r, is_filter_i, object_name, object_type, " +
                                     "status, observer, frame_type, x_bin, y_bin " +
                                     "FROM robophot_tasks ORDER BY time_start DESC LIMIT 50";
                var com = new NpgsqlCommand(query, Con);
                
                using (var reader = com.ExecuteReader())
                {
                    if (reader.HasRows)
                    {
                        var dt = new DataTable();
                        dt.Load(reader);
                        Tasker.dataGridViewTasker.Invoke((MethodInvoker)delegate
                        {
                            Tasker.dataGridViewTasker.DataSource = dt;
                            Tasker.PaintTable();
                        });
                        // Tasker.dataGridViewTasker.ReadOnly = false;

                    }
                    com.Dispose(); 
                }
                DisconnectFromDb();
            }
        }

        public static DataTable GetTableForThinking()
        {
            lock (Loc)
            {
                ConnectToDb();
                const string query = "SELECT task_id, get_hms_dms(start_coord2000) as coord2000, time_add, time_start, time_end, " +
                                     "duration, exp_time, done_frames, all_frames, time_last_exp, " +
                                     "is_filter_g, is_filter_r, is_filter_i, object_name, object_type, " +
                                     "status, observer, frame_type, x_bin, y_bin " +
                                     "FROM robophot_tasks WHERE status < 2 order by time_start ASC";
                var com = new NpgsqlCommand(query, Con);
                using ( var reader = com.ExecuteReader())
                {
                    var dt = new DataTable();
                    if (reader.HasRows)
                    {
                        dt.Load(reader);
                    }
                    com.Dispose();
                    DisconnectFromDb();
                    return dt;
                }

            }
        }
                
        public static void UpdateTaskFromDb(ref ObservationTask observationTask)   
        {
            lock (Loc)
            {
                ConnectToDb();
                string query = "SELECT task_id, get_hms_dms(start_coord2000) as coord2000, time_add, time_start, time_end, " +
                                             "duration, exp_time, done_frames, all_frames, time_last_exp, " +
                                             "is_filter_g, is_filter_r, is_filter_i, object_name, object_type, " +
                                             "status, observer, frame_type, x_bin, y_bin " +
                                             $"FROM robophot_tasks WHERE task_id = {observationTask.TaskNumber}";
                var com = new NpgsqlCommand(query, Con);
                using (var reader = com.ExecuteReader())
                {
                    var dt = new DataTable();
                    if (reader.HasRows)
                    {
                        dt.Load(reader);
                    }
                    com.Dispose();
                    Tasker.GetTaskFromRow(dt.Rows[0], ref observationTask);
                }
            }
        }

        private static string TaskQueryBuilder(ObservationTask observationTask)
        {
            return $"(({observationTask.Ra*360/24}, {observationTask.Dec})::spoint_domen, " +
                   $"'{observationTask.TimeAdd}'::timestamp, " +
                   $"'{observationTask.TimeStart}'::timestamp, " +
                   $"'{observationTask.TimeEnd}'::timestamp, " +
                   $"'{observationTask.TimeLastExp}'::timestamp, " +
                   $"{observationTask.Duration}, " +
                   $"{observationTask.Exp}, " +
                   $"{observationTask.DoneFrames}, {observationTask.AllFrames}, " +
                   $"{observationTask.Filters.Contains("g")}, " +
                   $"{observationTask.Filters.Contains("r")}, " +
                   $"{observationTask.Filters.Contains("i")}, " +
                   $"'{observationTask.Object}', '{observationTask.ObjectType}', " +
                   $"{observationTask.Status}, '{observationTask.Observer}', " +
                   $"'{observationTask.FrameType}', {observationTask.Xbin}, {observationTask.Ybin})";
        }

        public static bool AddTaskToDb(ObservationTask observationTask)
        {
            try
            {
                lock (Loc)
                {
                    ConnectToDb();
                    var query = "INSERT INTO robophot_tasks " +
                                    "(start_coord2000, " +
                                    "time_add, time_start, time_end, time_last_exp, " +
                                    "duration, exp_time, done_frames, all_frames, " +
                                    "is_filter_g, is_filter_r, is_filter_i, " +
                                    "object_name, object_type, status, observer, frame_type, x_bin, y_bin) VALUES " +
                                    $"{TaskQueryBuilder(observationTask)} RETURNING task_id";
                    var com = new NpgsqlCommand(query, Con);
                    using (var reader = com.ExecuteReader())
                    {
                        while (reader.Read()) observationTask.TaskNumber = Convert.ToInt32(reader[0]);
                        com.Dispose();
                    }
                    DisconnectFromDb();
                }
            }
            catch (Exception e)
            {
                Logger.AddLogEntry("AddTaskToDb error");
                Logger.AddLogEntry(e.Message);
                return false;
            }
            LoadDbTable();
            return true;
        }

        public static bool UpdateTaskInDb(ObservationTask observationTask)
        {   
            try
            {
                lock (Loc)
                {
                    ConnectToDb();
                    var query = "UPDATE robophot_tasks " +
                                    "SET (start_coord2000, " +
                                    "time_add, time_start, time_end, time_last_exp, " +
                                    "duration, exp_time, done_frames, all_frames, " +
                                    "is_filter_g, is_filter_r, is_filter_i, " +
                                    "object_name, object_type, status, observer, frame_type, x_bin, y_bin) = " +
                                    $"{TaskQueryBuilder(observationTask)} WHERE task_id = {observationTask.TaskNumber}";
                    using (var com = new NpgsqlCommand(query, Con))
                    {
                        com.ExecuteReader();
                        com.Dispose();
                    }
                    DisconnectFromDb();
                }

            }
            catch (Exception e)
            {
                Logger.AddLogEntry("UpdateTaskInDb error");
                Logger.AddLogEntry(e.Message);
                return false;
            }
            // LoadDbTable();
            return true;
        }
        
        public static bool AddFrameToDb(ObservationTask observationTask, string path, double ra, double dec, 
            string fil, DateTime date, double ext, double temp, string sn) 
        {       
            try
            {
                lock (Loc)
                {
                    ConnectToDb();
                    var query = "INSERT INTO robophot_frames (fk_task_id, " +
                                "frame_path, coord2000, frame_filter, " +
                                "date_utc, extinction, ccd_temp, camera_sn) VALUES " + 
                                $"({observationTask.TaskNumber}, '{path}', ({ra*360/24}, {dec})::spoint_domen, '{fil}', " +
                                $"'{date}'::timestamp, {ext}, {temp}, '{sn}')";
                    using (var com = new NpgsqlCommand(query, Con))
                    {
                        com.ExecuteReader();
                        com.Dispose();
                    }
                    DisconnectFromDb();
                }

            }
            catch (Exception e)
            {
                Logger.AddLogEntry("AddFrameToDb error");
                Logger.AddLogEntry(e.Message);
                return false;
            }
            return true;
        }
        
    }
}