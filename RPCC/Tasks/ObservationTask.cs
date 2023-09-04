using System;

namespace RPCC.Tasks
{
    public class ObservationTask
    {
        // public double Ra //decimal deg
        // {
        //     get;
        //     set;
        // }
        //
        // public double Dec
        // {
        //     get;
        //     set;
        // }
        
        public string RaDec { get; set; }

        public DateTime TimeAdd
        {
            get;
            set;
        }

        public DateTime TimeStart
        {
            get;
            set;
        }

        public DateTime TimeEnd
        {
            get;
            set;
        }

        public DateTime TimeLastExp
        {
            get;
            set;
        }
    
        public short Exp //in sec
        {
            get;
            set;
        }

        public short AllFrames
        {
            get;
            set;
        }

        public short DoneFrames
        {
            get;
            set;
        }
//0 = Wait, 1 = In progress, 5 = Ended not complete,
//2 = Ended complete, 3 = Rejected by observer, 4 = Not observed
        public short Status
        {
            get;
            set;
        }

        public string Object
        {
            get;
            set;
        }

        public string Observer
        {
            get;
            set;
        }

        public int TaskNumber
        {
            get;
            set;
        }
//light, dark, flat
        public string FrameType
        {
            get;
            set;
        }

        public string Filters
        {
            get;
            set;
        }
//in pix
        public short Xbin
        {
            get;
            set;
        }

        public short Ybin
        {
            get;
            set;
        }

        public short XSubframeStart
        {
            get;
            set;
        }

        public short YSubframeStart
        {
            get;
            set;
        }

        public short XSubframeEnd
        {
            get;
            set;
        }

        public short YSubframeEnd
        {
            get;
            set;
        }
        
//in hours
        public float Duration
        {
            get;
            set;
        }
        

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