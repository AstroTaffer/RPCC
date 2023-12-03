using System;
using System.Data;
using Npgsql;
using RPCC.Utils;


namespace RPCC.Tasks
{
    public static class DbCommunicate
    {
        public static string RoboPhotServer = "192.168.240.5"; // TODO to cfg
        public static string Port = "5432";
        public static string UserId = "postgres";
        public static string Password = "";
        public static string Database = "postrges";
        public static NpgsqlConnection Con;
        
        public static bool ConnectToDb()
        {
            try
            {
                var connString =
                    $"Server={RoboPhotServer};Port={Port};User Id={UserId};Password={Password}; Database={Database};";
                Con = new NpgsqlConnection(connString);
                Con.Open();
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
            Con.Close();
        }

        public static void LoadDbTable()
        {   
            const string query = "SELECT * FROM robophot_tasks";
            var com = new NpgsqlCommand(query, Con);
            var reader = com.ExecuteReader();
            if (reader.HasRows)
            {
                var dt = new DataTable();
                dt.Load(reader);
                Tasker.dataGridViewTasker.DataSource = dt;
            }
            com.Dispose();
        }

        private static string TaskQueryBuilder(ObservationTask observationTask)
        {
            return $"(({observationTask.Ra*360/24}, {observationTask.Dec})::spoint_domen, " +
                   $"'{observationTask.TimeAdd}'::timestamp, " +
                   $"'{observationTask.TimeStart}'::timestamp, " +
                   $"'{observationTask.TimeEnd}'::timestamp, " +
                   $"{observationTask.TimeLastExp}, " +
                   $"'{observationTask.Duration} hour'::interval, " +
                   $"'{observationTask.Exp} second'::interval, " +
                   $"{observationTask.DoneFrames}, {observationTask.AllFrames}, " +
                   $"{observationTask.Filters.Contains("g")}, " +
                   $"{observationTask.Filters.Contains("r")}, " +
                   $"{observationTask.Filters.Contains("i")}, " +
                   $"{observationTask.Object}, {observationTask.ObjectType}, " +
                   $"{observationTask.Status}, {observationTask.Observer}, " +
                   $"{observationTask.Object}, {observationTask.Xbin}, {observationTask.Ybin})";
        }

        public static bool AddTaskToDb(ObservationTask observationTask)
        {       
            try
            {
                var query = "INSERT INTO robophot_tasks " +
                            "(start_coord2000, " +
                            "time_add, time_start, time_end, time_last_exp, " +
                            "duration, exp_time, done_frames, all_frames, " +
                            "is_filter_g, is_filter_r, is_filter_i, " +
                            "object_name, object_type, status, observer, frame_type, x_bin, y_bin) VALUES " +
                            $"{TaskQueryBuilder(observationTask)} RETURNING task_id";
                var com = new NpgsqlCommand(query, Con);
                var reader = com.ExecuteReader();
                while (reader.Read()) observationTask.TaskNumber = Convert.ToInt32(reader[0]);
                com.Dispose();
            }
            catch (Exception e)
            {
                Logger.AddLogEntry(e.Message);
                return false;
            }
            return true;
        }

        public static bool UpdateTaskInDb(ObservationTask observationTask)
        {   
            try
            {
                var query = "UPDATE robophot_tasks " +
                            "SET (start_coord2000, " +
                            "time_add, time_start, time_end, time_last_exp, " +
                            "duration, exp_time, done_frames, all_frames, " +
                            "is_filter_g, is_filter_r, is_filter_i, " +
                            "object_name, object_type, status, observer, frame_type, x_bin, y_bin) = " +
                            $"{TaskQueryBuilder(observationTask)} WHERE task_id = {observationTask.TaskNumber}";
                var com = new NpgsqlCommand(query, Con);
                com.ExecuteReader();
                com.Dispose();
            }
            catch (Exception e)
            {
                Logger.AddLogEntry(e.Message);
                return false;
            }
            return true;
        }
        
        public static bool AddFrameToDb(ObservationTask observationTask, string path, double ra, double dec, 
            string fil, DateTime date, double ext, double temp) 
        {   
            try
            {   
                var query = "INSERT INTO robophot_frames (fk_task_id, " +
                            "frame_path, coord2000, frame_filter, " +
                            "date_utc, extinction, ccd_temp) VALUES " + 
                            $"({observationTask.TaskNumber}, {path}, ({ra*360/24}, {dec})::spoint_domen, {fil}, " +
                            $"{date}::timestamp, {ext}, {temp})";
                var com = new NpgsqlCommand(query, Con);
                com.ExecuteReader();
                com.Dispose();
            }
            catch (Exception e)
            {
                Logger.AddLogEntry(e.Message);
                return false;
            }
            return true;
        }
        
    }
}