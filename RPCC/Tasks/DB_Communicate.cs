using System;
using System.Data;
using System.Linq;
using System.Windows.Forms;
using Npgsql;
using RPCC.Cams;
using RPCC.Comms;
using RPCC.Utils;


namespace RPCC.Tasks
{
    public static class DbCommunicate
    {
        private const string RoboPhotServer = "192.168.240.5";
        private const string Port = "5432";
        private const string UserId = "remote_user";
        private const string Password = "remote_user";
        private const string Database = "postgres";

        private const string QueryForLoadDbTable = "SELECT task_id, get_hms_dms(start_coord2000) as coord2000, " +
                                                   "time_add, time_start, time_end, " +
                                                   "duration, exp_time, done_frames, all_frames, time_last_exp, " +
                                                   "is_filter_g, is_filter_r, is_filter_i, object_name, object_type, " +
                                                   "status, observer, frame_type, x_bin, y_bin " +
                                                   "FROM robophot_tasks ORDER BY time_start DESC LIMIT 50";

        private const string QueryGetTableForThinking = "SELECT task_id, get_hms_dms(start_coord2000) as coord2000, " +
                                                        "time_add, time_start, time_end, " +
                                                        "duration, exp_time, done_frames, all_frames, time_last_exp, " +
                                                        "is_filter_g, is_filter_r, is_filter_i, object_name, object_type, " +
                                                        "status, observer, frame_type, x_bin, y_bin " +
                                                        "FROM robophot_tasks WHERE status < 2 order by time_start ASC";
        // public static NpgsqlConnection Con;
        private static readonly object Loc = new object();
        
        public static NpgsqlConnection ConnectToDb()
        {
            try
            {
                lock (Loc)
                {
                    var connString =
                        $"Server={RoboPhotServer};Port={Port};User Id={UserId};Password={Password}; Database={Database};";
                     var con = new NpgsqlConnection(connString);
                    con.Open();
                    return con;
                }   

            }
            catch (Exception e)
            {
                Logger.AddLogEntry(@"Can't connect to data base");
                Logger.AddLogEntry(e.Message);
                // MessageBox.Show(@"Can't connect to data base", @"OK", MessageBoxButtons.OK);
                return null;
            }
        }

        public static void LoadDbTable()
        {
            lock (Loc)
            {
                using (var con = ConnectToDb())
                {
                    var com = new NpgsqlCommand(QueryForLoadDbTable, con);
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
                        }
                    }
                }
            }
        }

        public static DataTable GetTableForThinking()
        {
            lock (Loc)
            {
                using (var con = ConnectToDb())
                {
                    var com = new NpgsqlCommand(QueryGetTableForThinking, con);
                    using (var reader = com.ExecuteReader())
                    {
                        var dt = new DataTable();
                        if (reader.HasRows)
                        {
                            dt.Load(reader);
                        }
                        return dt;
                    }
                }
            }
        }
                
        public static void UpdateTaskFromDb(ref ObservationTask observationTask)   
        {
            lock (Loc)
            {
                var query = "SELECT task_id, get_hms_dms(start_coord2000) as coord2000, " +
                            "time_add, time_start, time_end, " +
                            "duration, exp_time, done_frames, all_frames, time_last_exp, " +
                            "is_filter_g, is_filter_r, is_filter_i, object_name, object_type, " +
                            "status, observer, frame_type, x_bin, y_bin " +
                            $"FROM robophot_tasks WHERE task_id = {observationTask.TaskNumber}";
                using (var con = ConnectToDb())
                {
                    var com = new NpgsqlCommand(query, con);
                    using (var reader = com.ExecuteReader())
                    {
                        var dt = new DataTable();
                        if (reader.HasRows)
                        {
                            dt.Load(reader);
                        }
                        Tasker.GetTaskFromRow(dt.Rows[0], ref observationTask);
                    }
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
                    var query = "INSERT INTO robophot_tasks " +
                                    "(start_coord2000, " +
                                    "time_add, time_start, time_end, time_last_exp, " +
                                    "duration, exp_time, done_frames, all_frames, " +
                                    "is_filter_g, is_filter_r, is_filter_i, " +
                                    "object_name, object_type, status, observer, frame_type, x_bin, y_bin) VALUES " +
                                    $"{TaskQueryBuilder(observationTask)} RETURNING task_id";
                    using (var con = ConnectToDb())
                    {
                       var com = new NpgsqlCommand(query, con);
                       using (var reader = com.ExecuteReader())
                       {
                           while (reader.Read()) observationTask.TaskNumber = Convert.ToInt32(reader[0]);
                       } 
                    }
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
                    var query = "UPDATE robophot_tasks " +
                                     "SET (start_coord2000, " +
                                     "time_add, time_start, time_end, time_last_exp, " +
                                     "duration, exp_time, done_frames, all_frames, " +
                                     "is_filter_g, is_filter_r, is_filter_i, " +
                                     "object_name, object_type, status, observer, frame_type, x_bin, y_bin) = " +
                                     $"{TaskQueryBuilder(observationTask)} WHERE task_id = {observationTask.TaskNumber}";
                    using (var con = ConnectToDb())
                    {
                        using (var com = new NpgsqlCommand(query, con))
                        {
                            com.ExecuteReader();
                        }
                    }

                }

            }
            catch (Exception e)
            {
                Logger.AddLogEntry("UpdateTaskInDb error");
                Logger.AddLogEntry(e.Message);
                return false;
            }
            LoadDbTable();
            return true;
        }
        
        public static bool AddFrameToDb(ObservationTask observationTask, string path, double ra, double dec, 
            string fil, DateTime date, double ext, double temp, string sn) 
        {       
            try
            {
                lock (Loc)
                {
                    var query = "INSERT INTO robophot_frames (fk_task_id, " +
                                "frame_path, coord2000, frame_filter, " +
                                "date_utc, extinction, ccd_temp, camera_sn, is_focus, is_looking_east) VALUES " + 
                                $"({observationTask.TaskNumber}, '{path}', ({ra*360/24}, {dec})::spoint_domen, '{fil}', " +
                                $"'{date}'::timestamp, {ext}, {temp}, '{sn}', {observationTask.FrameType == Head.Focus}, " +
                                $"{MountDataCollector.IsLookingEast})";
                    using (var con = ConnectToDb())
                    {
                        using (var com = new NpgsqlCommand(query, con))
                        {
                            com.ExecuteReader();
                        }
                    }
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

        public static bool AddMFrameToBd(ObservationTask observationTask)
        {
            lock (Loc)
            {
                try
                {
                    var fils = observationTask.Filters.Split(' ');
                    foreach (var fil in fils)
                    {
                        var cam = CameraControl.cams.Last(c => c.filter == fil);
                        var query = "INSERT INTO robophot_master_frames (m_frame_type, m_frame_filter, fk_task_id, " +
                                    $"m_camera_sn, m_x_bin, m_y_bin, m_exp_time) VALUES " +
                                    $"('m_{observationTask.FrameType}', '{fil}', {observationTask.TaskNumber}, '{cam.serialNumber}'," +
                                    $"{observationTask.Xbin}, {observationTask.Ybin}, {observationTask.Exp})";
                        using (var con = ConnectToDb())
                        {
                            using (var com = new NpgsqlCommand(query, con))
                            {
                                com.ExecuteReader();
                            }
                        }
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

        public static bool CanDoDarkFlat(bool isDark, int exp)
        {
            lock (Loc)
            {
                double time = 0;
                var query = "SELECT EXTRACT(EPOCH FROM (NOW() - time_start)::INTERVAL)/3600 " +
                            $"FROM robophot_tasks WHERE exp_time = {exp} AND " +
                            $"frame_type = '{(isDark ? Head.Dark : Head.Flat)}' ORDER BY task_id DESC LIMIT 1";
                using (var con = ConnectToDb())
                {
                    var com = new NpgsqlCommand(query, con);
                    using (var reader = com.ExecuteReader())
                    {
                        while (reader.Read()) time = Convert.ToDouble(reader[0]);
                    }
                }
                return isDark ? time > 24 : time > 8;
            }
        }

        public static string GetPath2FirstAssFrame(int task)
        {
            string path = null;
            lock (Loc)
            {
                var query = "SELECT robophot_frames.calibration_frame_path " +
                            $"FROM robophot_frames WHERE fk_task_id = {task} AND is_do_astrometry " +
                            $"AND robophot_frames.is_looking_east = {MountDataCollector.IsLookingEast} " +
                            $"ORDER BY frame_id ASC LIMIT 1";
                using (var con = ConnectToDb())
                {
                    var com = new NpgsqlCommand(query, con);
                    using (var reader = com.ExecuteReader())
                    {
                        while (reader.Read()) path = Convert.ToString(reader[0]);
                    }
                }
            }
            
            return !string.IsNullOrEmpty(path) ? path : null;
        }
        
    }
}   