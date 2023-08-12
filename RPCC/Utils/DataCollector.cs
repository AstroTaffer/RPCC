using System;
using System.Timers;

namespace RPCC.Utils
{
    public class DataCollector
    {
        private static readonly Timer MeteoTimer = new Timer(); //clock timer and status check timer
        private Logger _logger;

        /**
         * Class for collect and update weather data
         */
        private readonly RpccSocketClient _socketClient;

        public DataCollector(RpccSocketClient rpccSocketClient, Logger logger)
        {
            _socketClient = rpccSocketClient;
            _logger = logger;

            //create timer for main loop
            MeteoTimer.Elapsed += OnTimedEvent_Clock;
            MeteoTimer.Interval = 10000;
            MeteoTimer.Start();
        }

        //Data
        // TODO: Add pre-defiened values
        public double Sky { get; set; }
        public double SkyStd { get; set; }
        public double Extinction { get; set; }
        public double ExtinctionStd { get; set; }
        public double Seeing { get; set; }
        public double SeeingExtinction { get; set; }
        public double Wind { get; set; }
        public double Sun { get; set; }
        public bool Obs { get; set; }
        public bool Flat { get; set; }

        private void OnTimedEvent_Clock(object sender, ElapsedEventArgs e)
        {
            UpdateData();
            // _logger.AddLogEntry($"Sky: {Sky}");
            // _logger.AddLogEntry($"Flat: {Flat}");
        }

        private void UpdateData()
        {
            // var buf = "";
            Sky = double.Parse(_socketClient.DomeGetData("sky"));
            // = float.Parse(buf, CultureInfo.InvariantCulture.NumberFormat);
            SkyStd = double.Parse(_socketClient.DomeGetData("sky std"));
            // = float.Parse(buf, CultureInfo.InvariantCulture.NumberFormat);
            Extinction = double.Parse(_socketClient.DomeGetData("extinction"));
            // Extinction = float.Parse(buf, CultureInfo.InvariantCulture.NumberFormat);
            ExtinctionStd = double.Parse(_socketClient.DomeGetData("extinction std"));
            // = float.Parse(buf, CultureInfo.InvariantCulture.NumberFormat);
            Seeing = double.Parse(_socketClient.DomeGetData("seeing"));
            // = float.Parse(buf, CultureInfo.InvariantCulture.NumberFormat);

            try
            {
                SeeingExtinction = double.Parse(_socketClient.DomeGetData("seeing_extinction"));
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                // throw;
            }

            Wind = double.Parse(_socketClient.DomeGetData("wind"));
            // = float.Parse(buf, CultureInfo.InvariantCulture.NumberFormat);
            Sun = double.Parse(_socketClient.DomeGetData("sun"));
            // = float.Parse(buf, CultureInfo.InvariantCulture.NumberFormat);
            Obs = bool.Parse(_socketClient.DomeGetData("obs"));
            // = bool.Parse(buf);
            Flat = bool.Parse(_socketClient.DomeGetData("flat"));
            // = bool.Parse(buf);
        }

        public void Dispose()
        {
            MeteoTimer.Dispose();
        }
    }
}