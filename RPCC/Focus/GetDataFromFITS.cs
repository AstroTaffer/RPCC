using System;
using RPCC.Comms;
using RPCC.Utils;


/*
 * фалик чтобы вытащить из фитса центроиды, фвхм, эллиптичность
 */
namespace RPCC.Focus
{
    public class GetDataFromFits
    {
        private const double MaxEll = 0.3;
        private const int MinStars = 1;
        private const double FwhmFocused = 3.3;

        
        public bool Status { get; }
        public int Focus { get; }
        public bool Focused { get; }
        public double Fwhm { get; }
        public double Ell { get; }
        public int StarsNum { get; }
        public double Bkg { get; }
        public bool Quality { get; }

        public GetDataFromFits(string path2Fits)
        {
            if (string.IsNullOrEmpty(path2Fits)) return;
            var resp = DonutsSocket.GetImageFwhm(path2Fits);
            Focus = (int) resp[0];
            Fwhm = Math.Round(resp[1], 2);
            Ell = Math.Round(resp[2], 2);
            StarsNum = (int) resp[3];
            Bkg = resp[4];
            if (StarsNum == 0)
            {
                Status = false;
                Quality = false;
                return;
            }
            Status = true;
            Quality = CheckImageQuality();
            Focused = CheckFocused();
        }
        
        private bool CheckImageQuality()
        {
            if (Ell > MaxEll)
            {
                // игнорируем
                Logger.AddLogEntry($"Images stretched, ell = {Ell}");
                return false;
            }

            if (StarsNum < MinStars)
            {
                //мало звезд на обоих кадрах, игнорируем, скорее всего облако. Но может обе в диком дефокусе!
                Logger.AddLogEntry($"Few stars, {StarsNum}<{MinStars}");
                return false;
            }

            // Logger.AddLogEntry("FOCUS: Focus image is ok!");
            return true;
        }
        
        private bool CheckFocused()
        {
            if (CameraFocus.DeFocus != 0)
            {
                return Fwhm < FwhmFocused + 1;
            }

            if (Fwhm < FwhmFocused || Fwhm < CameraFocus.seeing)
            {
                Logger.AddLogEntry($"FOCUS: image is focused, fwhm = {Fwhm}");
            }
            else
            {
                Logger.AddLogEntry($"FOCUS: image is not focused, fwhm = {Fwhm}");
            }
            return Fwhm < FwhmFocused;
        }
    }
}