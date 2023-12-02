using Npgsql;
using System.Globalization;
using ASCOM.Tools;


namespace RPCC.Tasks
{
    public class DB_Communicate
    {
        public static string RoboPhotServer = "192.168.240.5";    
        public static string Port = "5432";
        public static string UserId = "";
        public static string Password = "";
        public static string Database = "postrges";
        public static NpgsqlConnection con;
        
        public static void ConnectToDB()
        {
            var _connString =
                $"Server={RoboPhotServer};Port={Port};User Id={UserId};Password={Password}; Database={Database};";
            con = new NpgsqlConnection(_connString);
            con.Open();
        }

        public static bool WriteTask(ObservationTask observationTask)
        {
            var qwery = "INSERT INTO robophot_tasks VALUES "; //TODO
            qwery += $"(default, ({observationTask.Ra*360/24}, {observationTask.Dec})::spoint_domen, " +
                    $"'2023-01-12 00:00:00'::timestamp, " +
                    $"'2023-01-12 01:00:00'::timestamp, " +
                    $"'2023-01-12 02:00:00'::timestamp, NULL, " +
                    $"'1 hour'::interval, '10 second'::interval, 0, 3000, 'FALSE', 'TRUE', 'TRUE', " +
                    $"'fuck', 'transit', 0, 'Chazov N.', 'Object', 2, 2)";
            return false;
        }
        
        public static bool WriteFrame(ObservationTask observationTask)
        {      
            var qwery = "INSERT INTO robophot_frames VALUES ";
            qwery += $"(default, 1, 1, 1, '', '', (0,0.01)::spoint_domen, 'r', " +
                        $"'2023-01-12 01:00:00'::timestamp, '', " +
                        $"null, null, null, null, null, -30, 'FALSE', 'FALSE', 'FALSE', 0)";
            return false;
        }
        
    }
}