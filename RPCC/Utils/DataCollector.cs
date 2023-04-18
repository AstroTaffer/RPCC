using System;
using System.Timers;

namespace RPCC.Utils
{
    public class DataCollector
    {
        /**
         * Class for collect and update weather data
         */
        private RpccSocketClient _socketClient;
        private static readonly Timer MeteoTimer = new Timer(); //clock timer and status check timer
        private Logger _logger;
        
        //Data
        public string Sky { get; set; }
        public string SkyStd { get;set; }
        public string Extinction { get;set; }
        public string ExtinctionStd { get;set; }
        public string Seeing { get;set; }
        public string SeeingExtinction { get;set; }
        public string Wind { get;set; }
        public string Sun { get; set;}
        public string Obs { get; set;}
        public string Flat { get; set;}

        public DataCollector(RpccSocketClient rpccSocketClient, Logger logger)
        {
            _socketClient = rpccSocketClient;
            _logger = logger;
            
            //create timer for main loop
            MeteoTimer.Elapsed += OnTimedEvent_Clock;
            MeteoTimer.Interval = 10000;
            MeteoTimer.Start();

        }

        private void OnTimedEvent_Clock(object sender, ElapsedEventArgs e)
        {
            UpdateData();
            // _logger.AddLogEntry($"Sky: {Sky}");
            // _logger.AddLogEntry($"Flat: {Flat}");
        }

        private void UpdateData()
        {
            // var buf = "";
            Sky = _socketClient.DomeGetData("sky");
             // = float.Parse(buf, CultureInfo.InvariantCulture.NumberFormat);
             SkyStd = _socketClient.DomeGetData("sky std");
             // = float.Parse(buf, CultureInfo.InvariantCulture.NumberFormat);
             Extinction = _socketClient.DomeGetData("extinction");
            // Extinction = float.Parse(buf, CultureInfo.InvariantCulture.NumberFormat);
            ExtinctionStd = _socketClient.DomeGetData("extinction std");
             // = float.Parse(buf, CultureInfo.InvariantCulture.NumberFormat);
             Seeing = _socketClient.DomeGetData("seeing");
             // = float.Parse(buf, CultureInfo.InvariantCulture.NumberFormat);

            try
            {
                SeeingExtinction = _socketClient.DomeGetData("seeing_extinction");
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                // throw;
            }

            Wind = _socketClient.DomeGetData("wind");
             // = float.Parse(buf, CultureInfo.InvariantCulture.NumberFormat);
             Sun = _socketClient.DomeGetData("sun");
             // = float.Parse(buf, CultureInfo.InvariantCulture.NumberFormat);
             Obs = _socketClient.DomeGetData("obs");
             // = bool.Parse(buf);
             Flat = _socketClient.DomeGetData("flat");
             // = bool.Parse(buf);
        }

        public static void Dispose()
        {
            MeteoTimer.Dispose();
        }
    }
}