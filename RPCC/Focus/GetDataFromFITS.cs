using System;
using RPCC.Cams;
using RPCC.Comms;
using RPCC.Tasks;
using RPCC.Utils;


/*
 * фалик чтобы вытащить из фитса центроиды, фвхм, эллиптичность
 */
namespace RPCC.Focus
{
    public class GetDataFromFits
    {
        private const float MaxEll = 0.5f;
        private const int MinStars = 4;
        private const float FwhmFocused = 3.3f;

        
        public bool Status { get; }
        public int Focus { get; }
        public bool Focused { get; }
        public float Fwhm { get; }
        public float Ell { get; }
        public int StarsNum { get; }
        public float Bkg { get; }
        public bool Quality { get; }

        public GetDataFromFits(ICameraDevice cam)
        {
            // if (string.IsNullOrEmpty(path2Fits)) return;
            var resp = DonutsRunner.GetImageMetrics(cam.LatestImageFilename);
            // var resp = DonutsSocket.GetImageFwhm(cam.LatestImageFilename);
            Focus = resp.Focus;
            Fwhm = resp.Fwhm;
            Ell = resp.Ell;
            StarsNum = resp.Stars;
            Bkg = resp.Bkg;
            DbCommunicate.AddSexToDb(cam.LastImageId, Fwhm, Ell, Bkg);
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
                return Fwhm < FwhmFocused + 1 || Fwhm < CameraFocus.Seeing + 1;
            }

            // if (Fwhm < FwhmFocused || Fwhm < CameraFocus.Seeing)
            // {
            //     Logger.AddLogEntry($"FOCUS: image is focused, fwhm = {Fwhm}");
            // }
            // else
            // {
            //     Logger.AddLogEntry($"FOCUS: image is not focused, fwhm = {Fwhm}");
            // }
            return Fwhm < FwhmFocused || Fwhm < CameraFocus.Seeing;
        }
    }
}