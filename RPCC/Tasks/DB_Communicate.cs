using System;
using System.Data;
using System.Linq;
using System.Windows.Forms;
using Npgsql;
using NpgsqlTypes;
using RPCC.Cams;
using RPCC.Comms;
using RPCC.Utils;


namespace RPCC.Tasks;

public static class DbCommunicate
{
    private static readonly object DbLoc = new();
        
    private static NpgsqlConnection ConnectToDb()
    {
        try
        {
            lock (DbLoc)  
            {
                var connString =
                    $"Server=127.0.0.1;Port={Settings.DbPort};User Id={Settings.DbUserId};" +
                    $"Password={Settings.DbPassword}; Database={Settings.Database};";
                var con = new NpgsqlConnection(connString);
                con.Open();
                
                return con;
            }   

        }
        catch (Exception e)
        {
            Logger.AddError("Can't connect to data base", e);
            return null;
        }
    }

    public static void LoadDbTable()
    {
        // lock (Loc)
        // {   
        //      const string queryForLoadDbTable = "SELECT task_id, get_hms_dms(start_coord2000) as coord2000, " +
        //                                         "time_add, time_start, time_end, " +
        //                                         "duration, exp_time, done_frames, all_frames, time_last_exp, " +
        //                                         "is_filter_g, is_filter_r, is_filter_i, object_name, object_type, " +
        //                                         "status, observer, frame_type, x_bin, y_bin, " +
        //                                         "repoint_coords, repoint_times, is_filter_v " + 
        //                                         "FROM robophot_tasks ORDER BY time_start DESC LIMIT 50";
        //     using var con = ConnectToDb();
        //     var com = new NpgsqlCommand(queryForLoadDbTable, con);
        //     using var reader = com.ExecuteReader();
        //     if (!reader.HasRows) return;
        //     var dt = new DataTable();
        //     dt.Load(reader);
        //     Tasker.DataGridViewTasker.Invoke((MethodInvoker)delegate
        //     {
        //         Tasker.DataGridViewTasker.DataSource = dt;
        //         Tasker.PaintTable();
        //     });
        // }
        
        DataTable dt;

        // DB work is serialized by Loc, but UI updates must not happen under this lock:
        // otherwise UI-thread can be waiting for Loc while this thread is waiting for UI via Invoke => deadlock.
        lock (DbLoc)
        {
            using var con = ConnectToDb();
            const string queryForLoadDbTable = "SELECT task_id, get_hms_dms(start_coord2000) as coord2000, " +
                                               "time_add, time_start, time_end, " +
                                               "duration, exp_time, done_frames, all_frames, time_last_exp, " +
                                               "is_filter_g, is_filter_r, is_filter_i, object_name, object_type, " +
                                               "status, observer, frame_type, x_bin, y_bin, " +
                                               "repoint_coords, repoint_times, is_filter_v " + 
                                               "FROM robophot_tasks ORDER BY time_start DESC LIMIT 50";
            var com = new NpgsqlCommand(queryForLoadDbTable, con);
            using var reader = com.ExecuteReader();
            if (!reader.HasRows) return;

            dt = new DataTable();
            dt.Load(reader);
        }

        void Apply()
        {
            // Defensive: during shutdown the control can be disposed.
            if (Tasker.DataGridViewTasker.IsDisposed) return;
            Tasker.DataGridViewTasker.DataSource = dt;
            Tasker.PaintTable();
        }

        // Prefer BeginInvoke to avoid blocking the caller thread and reduce risk of secondary deadlocks.
        if (Tasker.DataGridViewTasker.InvokeRequired)
            Tasker.DataGridViewTasker.BeginInvoke((MethodInvoker)Apply);
        else
            Apply();
    }

    public static DataTable GetTableForThinking()
    {   
        const string queryGetTableForThinking = "SELECT task_id, get_hms_dms(start_coord2000) as coord2000, " +
                                                "time_add, time_start, time_end, " +
                                                "duration, exp_time, done_frames, all_frames, time_last_exp, " +
                                                "is_filter_g, is_filter_r, is_filter_i, object_name, object_type, " +
                                                "status, observer, frame_type, x_bin, y_bin, " +
                                                "repoint_coords, repoint_times, is_filter_v " +
                                                "FROM robophot_tasks WHERE status < 2 order by time_start";
        lock (DbLoc)
        {
            using var con = ConnectToDb();
            var com = new NpgsqlCommand(queryGetTableForThinking, con);
            using var reader = com.ExecuteReader();
            var dt = new DataTable();
            if (reader.HasRows)
            {
                dt.Load(reader);
            }
            return dt;
        }
    }
    
   public static ObservationTask GetTaskFromDb(int taskId)
    {
        using var conn = ConnectToDb();
        string query = "SELECT * FROM robophot_tasks WHERE task_id = @task_id";
        using var cmd = new NpgsqlCommand(query, conn);
        cmd.Parameters.AddWithValue("task_id", taskId);

        using var reader = cmd.ExecuteReader();
        if (!reader.Read())
            throw new Exception($"Задание с task_id = {taskId} не найдено в БД");

        var spoint = reader.Get<Spoint>("start_coord2000");

        var task = new ObservationTask
        {
            TaskNumber = taskId,
            Ra = spoint.TargetRa * 24 / 360,
            Dec = spoint.TargetDec,
            RaDec = $"{ASCOM.Tools.Utilities.HoursToHMS(spoint.TargetRa * 24 / 360)} {ASCOM.Tools.Utilities.DegreesToDMS(spoint.TargetDec)}",

            TimeAdd = reader.Get<DateTime>("time_add"),
            TimeStart = reader.Get<DateTime>("time_start"),
            TimeEnd = reader.Get<DateTime>("time_end"),
            TimeLastExp = reader.Get<DateTime>("time_last_exp"),

            Duration = reader.Get<float>("duration"),
            Exp = reader.Get<short>("exp_time"),
            DoneFrames = reader.Get<short>("done_frames"),
            AllFrames = reader.Get<short>("all_frames"),

            Object = reader.Get<string>("object_name"),
            Observer = reader.Get<string>("observer"),
            FrameType = reader.Get<string>("frame_type"),
            ObjectType = reader.Get<string>("object_type"),
            Status = reader.Get<short>("status"),
            Xbin = reader.Get<short>("x_bin"),
            Ybin = reader.Get<short>("y_bin"),

            Filters = string.Join(" ", new[]
            {
                reader.Get<bool>("is_filter_g") ? StringHolder.FilG : null,
                reader.Get<bool>("is_filter_v") ? StringHolder.FilV : null,
                reader.Get<bool>("is_filter_r") ? StringHolder.FilR : null,
                reader.Get<bool>("is_filter_i") ? StringHolder.FilI : null
            }.Where(f => f != null)),

            RepointCoords = reader.Get<string[]>("repoint_coords")?.ToList() ?? new(),
            RepointTimes = reader.Get<DateTime[]>("repoint_times")?.ToList() ?? new()
        };

        return task;
    }
   
    // public static void UpdateTaskFromDb(ref ObservationTask observationTask)   
    // {
    //     try
    //     {
    //         lock (Loc)
    //         {
    //             var query = "SELECT task_id, get_hms_dms(start_coord2000) as coord2000, " +
    //                         "time_add, time_start, time_end, " +
    //                         "duration, exp_time, done_frames, all_frames, time_last_exp, " +
    //                         "is_filter_g, is_filter_r, is_filter_i, object_name, object_type, " +
    //                         "status, observer, frame_type, x_bin, y_bin, repoint_coords, repoint_times, is_filter_v " +
    //                         $"FROM robophot_tasks WHERE task_id = {observationTask.TaskNumber}";
    //             using var con = ConnectToDb();
    //             var com = new NpgsqlCommand(query, con);
    //             using var reader = com.ExecuteReader();
    //             var dt = new DataTable();
    //             if (reader.HasRows)
    //             {
    //                 dt.Load(reader);
    //             }
    //             Tasker.GetTaskFromRow(dt.Rows[0], ref observationTask);
    //         }
    //     }
    //     catch (Exception e)
    //     {
    //         Logger.AddError("UpdateTaskFromDb", e);
    //     }
    // }

    public static bool AddTaskToDb(ObservationTask observationTask)
    {

        try
        {
            lock (DbLoc)
            {
                const string query = "INSERT INTO robophot_tasks " +
                                     "(start_coord2000, " +
                                     "time_add, time_start, time_end, time_last_exp, " +
                                     "duration, exp_time, done_frames, all_frames, " +
                                     "is_filter_g, is_filter_r, is_filter_i, " +
                                     "object_name, object_type, status, observer, frame_type, x_bin, y_bin, " +
                                     "repoint_coords, repoint_times, is_filter_v) VALUES " +
                                     "($1, $2, $3, $4, $5, $6, $7, $8, $9, $10, $11, $12, $13, " +
                                     "$14, $15, $16, $17, $18, $19, $20, $21, $22)  " +
                                     "RETURNING task_id";
                using var con = ConnectToDb();
                var com = new NpgsqlCommand(query, con)
                {
                    Parameters = { 
                        new NpgsqlParameter { Value = new Spoint
                        {
                            TargetRa = observationTask.Ra*360/24,
                            TargetDec = observationTask.Dec
                        }, DataTypeName = "spoint_domen"},
                        new NpgsqlParameter { Value = observationTask.TimeAdd.ToUniversalTime(), NpgsqlDbType = NpgsqlDbType.TimestampTz },
                        new NpgsqlParameter { Value = observationTask.TimeStart.ToUniversalTime(), NpgsqlDbType = NpgsqlDbType.TimestampTz },
                        new NpgsqlParameter { Value = observationTask.TimeEnd.ToUniversalTime(), NpgsqlDbType = NpgsqlDbType.TimestampTz },
                        new NpgsqlParameter { Value = observationTask.TimeLastExp.ToUniversalTime(), NpgsqlDbType = NpgsqlDbType.TimestampTz },
                        new NpgsqlParameter { Value = observationTask.Duration },
                        new NpgsqlParameter { Value = observationTask.Exp },
                        new NpgsqlParameter { Value = observationTask.DoneFrames },
                        new NpgsqlParameter { Value = observationTask.AllFrames },
                        new NpgsqlParameter { Value = observationTask.Filters.Contains(StringHolder.FilG) },
                        new NpgsqlParameter { Value = observationTask.Filters.Contains(StringHolder.FilR) },
                        new NpgsqlParameter { Value = observationTask.Filters.Contains(StringHolder.FilI) },
                        new NpgsqlParameter { Value = observationTask.Object is null ? 
                            DBNull.Value : observationTask.Object, NpgsqlDbType = NpgsqlDbType.Text},
                        new NpgsqlParameter { Value = observationTask.ObjectType  is null ? 
                            DBNull.Value : observationTask.ObjectType, NpgsqlDbType = NpgsqlDbType.Text },
                        new NpgsqlParameter { Value = observationTask.Status, NpgsqlDbType = NpgsqlDbType.Smallint },
                        new NpgsqlParameter { Value = observationTask.Observer, NpgsqlDbType = NpgsqlDbType.Text },
                        new NpgsqlParameter { Value = observationTask.FrameType, NpgsqlDbType = NpgsqlDbType.Text },
                        new NpgsqlParameter { Value = observationTask.Xbin, NpgsqlDbType = NpgsqlDbType.Smallint },
                        new NpgsqlParameter { Value = observationTask.Ybin, NpgsqlDbType = NpgsqlDbType.Smallint },
                        new NpgsqlParameter { Value = observationTask.RepointCoords?.ToArray() ?? Array.Empty<string>(), 
                            NpgsqlDbType = NpgsqlDbType.Array | NpgsqlDbType.Text }, 
                        new NpgsqlParameter { Value = observationTask.RepointTimes?.ToArray() ?? Array.Empty<DateTime>(),
                            NpgsqlDbType = NpgsqlDbType.Array | NpgsqlDbType.Timestamp
                        },
                        new NpgsqlParameter { Value = observationTask.Filters.Contains(StringHolder.FilV) }
                    }
                };
                using var reader = com.ExecuteReader();
                while (reader.Read()) observationTask.TaskNumber = Convert.ToInt32(reader[0]);
                Logger.LogTaskSummary(observationTask, "added");
            }
        }
        catch (Exception e)
        {
            Logger.AddError("AddTaskToDb", e);
            return false;
        }
        LoadDbTable();
        return true;
    }

    public static bool UpdateTaskInDb(ObservationTask observationTask)
    {   
        try
        {
            lock (DbLoc)
            {
                // var sus = TaskQueryBuilder(observationTask);
                var query = "UPDATE robophot_tasks " +
                            "SET (start_coord2000, " +
                            "time_add, time_start, time_end, time_last_exp, " +
                            "duration, exp_time, done_frames, all_frames, " +
                            "is_filter_g, is_filter_r, is_filter_i, " +
                            "object_name, object_type, status, observer, frame_type, x_bin, y_bin, " +
                            "repoint_coords, repoint_times, is_filter_v) = " +
                            "($1, $2, $3, $4, $5, $6, $7, $8, $9, $10, $11, $12, $13, " +
                            "$14, $15, $16, $17, $18, $19, $20, $21, $22)  " +
                            $" WHERE task_id = {observationTask.TaskNumber}";
                using var con = ConnectToDb();
                var com = new NpgsqlCommand(query, con)
                {
                    Parameters = { 
                        new NpgsqlParameter { Value = new Spoint
                        {
                            TargetRa = observationTask.Ra*360/24,
                            TargetDec = observationTask.Dec
                        }, DataTypeName = "spoint_domen"},
                        new NpgsqlParameter { Value = observationTask.TimeAdd.ToUniversalTime(), NpgsqlDbType = NpgsqlDbType.TimestampTz},
                        new NpgsqlParameter { Value = observationTask.TimeStart.ToUniversalTime(), NpgsqlDbType = NpgsqlDbType.TimestampTz },
                        new NpgsqlParameter { Value = observationTask.TimeEnd.ToUniversalTime(), NpgsqlDbType = NpgsqlDbType.TimestampTz },
                        new NpgsqlParameter { Value = observationTask.TimeLastExp.ToUniversalTime(), NpgsqlDbType = NpgsqlDbType.TimestampTz },
                        new NpgsqlParameter { Value = observationTask.Duration },
                        new NpgsqlParameter { Value = observationTask.Exp },
                        new NpgsqlParameter { Value = observationTask.DoneFrames },
                        new NpgsqlParameter { Value = observationTask.AllFrames },
                        new NpgsqlParameter { Value = observationTask.Filters.Contains(StringHolder.FilG) },
                        new NpgsqlParameter { Value = observationTask.Filters.Contains(StringHolder.FilR) },
                        new NpgsqlParameter { Value = observationTask.Filters.Contains(StringHolder.FilI) },
                        new NpgsqlParameter { Value = observationTask.Object is null ? 
                            DBNull.Value : observationTask.Object, NpgsqlDbType = NpgsqlDbType.Text},
                        new NpgsqlParameter { Value = observationTask.ObjectType  is null ? 
                            DBNull.Value : observationTask.ObjectType, NpgsqlDbType = NpgsqlDbType.Text },
                        new NpgsqlParameter { Value = observationTask.Status, NpgsqlDbType = NpgsqlDbType.Smallint },
                        new NpgsqlParameter { Value = observationTask.Observer, NpgsqlDbType = NpgsqlDbType.Text },
                        new NpgsqlParameter { Value = observationTask.FrameType, NpgsqlDbType = NpgsqlDbType.Text },
                        new NpgsqlParameter { Value = observationTask.Xbin, NpgsqlDbType = NpgsqlDbType.Smallint },
                        new NpgsqlParameter { Value = observationTask.Ybin, NpgsqlDbType = NpgsqlDbType.Smallint },
                        new NpgsqlParameter { Value = observationTask.RepointCoords?.ToArray() ?? Array.Empty<string>(), 
                            NpgsqlDbType = NpgsqlDbType.Array | NpgsqlDbType.Text },
                        new NpgsqlParameter { Value = observationTask.RepointTimes?.ToArray() ?? Array.Empty<DateTime>(),
                            NpgsqlDbType = NpgsqlDbType.Array | NpgsqlDbType.Timestamp
                        },
                        new NpgsqlParameter { Value = observationTask.Filters.Contains(StringHolder.FilV) }
                    }
                };
                com.ExecuteReader();
                Logger.LogTaskSummary(observationTask, "updated");
            }
        }
        catch (Exception e)
        {
            Logger.AddError("UpdateTaskInDb", e);
            return false;
        }
        LoadDbTable();
        return true;
    }
    
    public static double GetActualAmbientTemp()
    {   
        const string query = @"
                SELECT amb_temp
                FROM robophot_weather
                WHERE time_utc > now() AT TIME ZONE 'UTC' - interval '10 minutes'
                ORDER BY time_utc DESC
                LIMIT 1;
            ";
        double amb = 100;

        try
        {
            using var con = ConnectToDb();
            using var com = new NpgsqlCommand(query, con);
            using var reader = com.ExecuteReader();
            while (reader.Read()) amb = Convert.ToDouble(reader[0]);
            return amb;
        }
        catch (Exception ex)
        {
            // Логировать ошибку при необходимости
            Console.WriteLine($"Ошибка при получении температуры: {ex.Message}");
        }

        return 100f; // Возврат 100 по умолчанию
    }
            
    public static int AddFrameToDb(ObservationTask observationTask, string path, double ra, double dec, 
        string fil, DateTime date, double ext, double temp, string sn) 
    {       
        int id = 0;
        try
        {
            lock (DbLoc)
            {
                
                var query = "INSERT INTO robophot_frames (fk_task_id, " +
                            "frame_path, coord2000, frame_filter, " +
                            "date_utc, extinction, ccd_temp, camera_sn, is_focus, is_looking_east) VALUES " + 
                            $"({observationTask.TaskNumber}, '{path}', ({ra*360/24}, {dec})::spoint_domen, '{fil}', " +
                            $"'{date}'::timestamp, {ext}, {temp}, '{sn}', {observationTask.FrameType == StringHolder.Focus}, " +
                            $"{MountDataCollector.IsLookingEast}) RETURNING frame_id";
                using var con = ConnectToDb();
                using var com = new NpgsqlCommand(query, con);
                using var reader = com.ExecuteReader();
                while (reader.Read()) id = Convert.ToInt32(reader[0]);
            }
        }
        catch (Exception e)
        {
            Logger.AddError("AddFrameToDb", e);
            return 0;
        }
        return id;
    }
    
    public static bool AddSexToDb(int frameId, double fwhm, double ell, double bkg, int starsNum) 
    {       
        try
        {
            lock (DbLoc)
            {
                var query =
                    $"""
                    UPDATE robophot_frames SET (sex_fwhm, sex_ell, sex_background, n_stars,
                    is_do_sex) = ({fwhm}, {ell}, {bkg}, {starsNum}, true) WHERE frame_id = {frameId}
                    """;
                using var con = ConnectToDb();
                using var com = new NpgsqlCommand(query, con);
                com.ExecuteReader();
            }
        }
        catch (Exception e)
        {
            Logger.AddError("AddSexToDb", e);
            return false;
        }
        return true;
    }

    public static bool AddMFrameToBd(ObservationTask observationTask)
    {
        lock (DbLoc)
        {
            try
            {
                var fils = observationTask.Filters.Split(' ');
                foreach (var fil in fils)
                {
                    if (string.IsNullOrEmpty(fil)) continue;
                    var cam = CameraControl.cams.Last(c => c.Filter == fil);
                    var query = "INSERT INTO robophot_master_frames (m_frame_type, m_frame_filter, fk_task_id, " +
                                $"m_camera_sn, m_x_bin, m_y_bin, m_exp_time) VALUES " +
                                $"('m_{observationTask.FrameType}', '{fil}', {observationTask.TaskNumber}, '{cam.SerialNumber}'," +
                                $"{observationTask.Xbin}, {observationTask.Ybin}, {observationTask.Exp})";
                    using var con = ConnectToDb();
                    using var com = new NpgsqlCommand(query, con);
                    com.ExecuteReader();
                }
            }
            catch (Exception e)
            {
                Logger.AddError("AddMFrameToBd", e);
                return false;
            }
            return true;
        }
    }

    public static bool CanDoDarkFlat(bool isDark, int exp)
    {
        lock (DbLoc)
        {
            double time = 0;
            var query = "SELECT EXTRACT(EPOCH FROM (NOW() - time_start)::INTERVAL)/3600 " +
                        $"FROM robophot_tasks WHERE exp_time = {exp} AND " +
                        $"frame_type = '{(isDark ? StringHolder.Dark : StringHolder.Flat)}' ORDER BY task_id DESC LIMIT 1";
            using (var con = ConnectToDb())
            {
                var com = new NpgsqlCommand(query, con);
                using (var reader = com.ExecuteReader())
                {
                    while (reader.Read()) time = Convert.ToDouble(reader[0]);
                }
            }
            return isDark ? time > Settings.DarkTasksTimeout : time > Settings.FlatTasksTimeout;
        }
    }

    public static string GetPath2FirstAssFrame(int task)
    {
        string path = null;
        lock (DbLoc)
        {
            var query = "SELECT robophot_frames.calibration_frame_path " +
                        $"FROM robophot_frames WHERE fk_task_id = {task} AND is_do_astrometry " +
                        $"AND robophot_frames.is_looking_east = {MountDataCollector.IsLookingEast} " +
                        $"ORDER BY frame_id ASC LIMIT 1";
            using var con = ConnectToDb();
            var com = new NpgsqlCommand(query, con);
            using var reader = com.ExecuteReader();
            while (reader.Read()) path = Convert.ToString(reader[0]);
        }
            
        return !string.IsNullOrEmpty(path) ? path : null;
    }
        
}
public class Spoint
{
    public double TargetRa { get; set; }
    public double TargetDec { get; set; }
}

public static class NpgsqlReaderHelper
{
    public static T Get<T>(this NpgsqlDataReader reader, string columnName)
    {
        try
        {
            int ordinal = reader.GetOrdinal(columnName);
            if (reader.IsDBNull(ordinal))
                return default!;
            return reader.GetFieldValue<T>(ordinal);
        }
        catch (Exception ex)
        {
            Logger.AddLogEntry($"[ERROR] Ошибка чтения поля '{columnName}': {ex.Message}");
            return default!;
        }
    }
}

