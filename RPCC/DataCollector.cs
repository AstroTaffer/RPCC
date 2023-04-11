using System.Globalization;
using System.Timers;

namespace RPCC
{
    public class DataCollector
    {
        /**
         * Class for collect and update weather data
         */
        private RpccSocketClient _socketClient;
        private static readonly Timer MeteoTimer = new Timer(); //clock timer and status check timer
        //Data

        public float Sky { get; set; }
        public float SkyStd { get;set; }
        public float Extinction { get;set; }
        public float ExtinctionStd { get;set; }
        public float Seeing { get;set; }
        public float SeeingExtinction { get;set; }
        public float Wind { get;set; }
        public float Sun { get; set;}
        public bool Obs { get; set;}
        public bool Flat { get; set;}

        public DataCollector(RpccSocketClient rpccSocketClient)
        {
            _socketClient = rpccSocketClient;
            
            //create timer for main loop
            MeteoTimer.Elapsed += OnTimedEvent_Clock;
            MeteoTimer.Interval = 10000;
            MeteoTimer.Start();

        }

        private void OnTimedEvent_Clock(object sender, ElapsedEventArgs e)
        {
            UpdateData();
        }

        private void UpdateData()
        {
            Sky = float.Parse(_socketClient.DomeGetData("sky"), CultureInfo.InvariantCulture.NumberFormat);
            SkyStd = float.Parse(_socketClient.DomeGetData("sky std"), CultureInfo.InvariantCulture.NumberFormat);
            Extinction = float.Parse(_socketClient.DomeGetData("extinction"), CultureInfo.InvariantCulture.NumberFormat);
            ExtinctionStd = float.Parse(_socketClient.DomeGetData("extinction std"), CultureInfo.InvariantCulture.NumberFormat);
            Seeing = float.Parse(_socketClient.DomeGetData("seeing"), CultureInfo.InvariantCulture.NumberFormat);
            SeeingExtinction = float.Parse(_socketClient.DomeGetData("seeing_extinction"), CultureInfo.InvariantCulture.NumberFormat);
            Wind = float.Parse(_socketClient.DomeGetData("wind"), CultureInfo.InvariantCulture.NumberFormat);
            Sun = float.Parse(_socketClient.DomeGetData("sun"), CultureInfo.InvariantCulture.NumberFormat);
            Obs = bool.Parse(_socketClient.DomeGetData("obs"));
            Flat = bool.Parse(_socketClient.DomeGetData("flat"));
        }

        public void Dispose()
        {
            MeteoTimer.Dispose();
        }
    }
}