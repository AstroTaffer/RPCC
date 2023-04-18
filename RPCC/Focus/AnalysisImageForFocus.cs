using RPCC.Utils;

namespace RPCC.Focus
{
    public class AnalysisImageForFocus
    {
        private const double MaxEll = 0.25;
        private const int MinStars = 20;

        private Logger _log;
        // переменные кадра

        public double Fwhm { get; }

        private const double FwhmFocused = 3.0;

        public AnalysisImageForFocus(GetDataFromFits getDataFromFits, Logger logger)
        {
            GetDataFromFits = getDataFromFits;
            _log = logger;
            StarsNum = GetDataFromFits.GetStarsNum();
            Ell = GetDataFromFits.GetEllipticity();
            Fwhm = GetDataFromFits.GetMedianFwhm();
        }

        // числа в конфиг
        // private const double FwhmGood = 3.2;

        public GetDataFromFits GetDataFromFits { get; }

        private double Ell { get; }

        private double StarsNum { get; }

        public bool CheckImageQuality()
        {
            if (Ell > MaxEll)
            {
                // игнорируем
                _log.AddLogEntry("FOCUS: Images stretched");
                return false;
            }

            if (StarsNum < MinStars)
            {
                //мало звезд на обоих кадрах, игнорируем, скорее всего облако. Но может обе в диком дефокусе!
                _log.AddLogEntry("FOCUS: Few stars on both images");
                return false;
            }
            _log.AddLogEntry("FOCUS: Focus image is ok!");
            return true;
        }

        // public bool CheckStarsNum()
        // {
        //     return StarsNum > MinStars;
        // }

        public bool CheckFocused()
        {
            return Fwhm < FwhmFocused;
        }
    }
}