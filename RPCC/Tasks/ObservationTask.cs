using System;

namespace RPCC.Tasks
{
    public class ObservationTask
    {
        private double Ra;  //decimal deg
        private double Dec;
        private DateTime timeAdd;
        private DateTime timeStart;
        private DateTime timeEnd;
        private DateTime timeLastExp;
        private float exp;
        private short AllFrames;
        private short DoneFrames;
        private short Status;  //0 = Wait, 1 = In progress, 5 = Below horizont, 6 = Ended not complete,
                               //2 = Ended complete, 3 = Rejected by observer, 4 = Not observed
        private string Object;
        private string Observer;
        private int TaskNumber;
        private string FrameType;  //light, dark, flat, bias, focus, test
        private string Filters;
        private short Xbin; //in pix
        private short Ybin;
        private short XSubframeStart;
        private short YSubframeStart;
        private short XSubframeEnd;
        private short YSubframeEnd;
        private int ExposuresNumber;
        private float Duration; //in hours
        // private int Defocus; //in steps

        public ObservationTask()
        {
            
        }

        private string GetInHms()
        {
            //h:m:s.ss
            return ""; 
        }

        private string GetInDms()
        {
            //d:m:s.ss
            return "";
        }
    }
}