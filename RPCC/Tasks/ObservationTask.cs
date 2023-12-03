using System;
using ASCOM.Tools;

namespace RPCC.Tasks
{
    public class ObservationTask
    {
        public double Ra //decimal h
        {
            get;
            set;
        }

        public double Dec //decimal deg
        {
            get;
            set;
        }

        public string RaDec { get; set; }

        public DateTime TimeAdd { get; set; }

        public DateTime TimeStart { get; set; }

        public DateTime TimeEnd { get; set; }

        public DateTime TimeLastExp { get; set; }

        public short Exp //in sec
        {
            get;
            set;
        }

        public short AllFrames { get; set; }

        public short DoneFrames { get; set; }

//0 = Wait, 1 = In progress, 5 = Ended not complete,
//2 = Ended complete, 3 = Rejected by observer, 4 = Not observed
        public short Status { get; set; } = 0;
        
        public string ObjectType { get; set; }

        public string Object { get; set; }

        public string Observer { get; set; }

        public int TaskNumber { get; set; }

//Object, Dark, Flat, Focus, Test, bias
        public string FrameType { get; set; }

        public string Filters { get; set; } = "g r i";

//in pix
// Бин камер должен быть от 1 до 16
        public int Xbin { get; set; } = 2;

        public int Ybin { get; set; } = 2;

        // public short XSubframeStart { get; set; } = 0;
        //
        // public short YSubframeStart { get; set; } = 0;
        //
        // public short XSubframeEnd { get; set; } = 2047;
        //
        // public short YSubframeEnd { get; set; } = 2047;

//in hours
        public float Duration { get; set; }

        public void ComputeRaDec(string coordsString)
        {
            RaDec = coordsString;
            var radec = RaDec.Split(' ');
            Ra = Utilities.HMSToHours(radec[0]);
            Dec = Utilities.DMSToDegrees(radec[1]);
        }

        public void Update()
        {
            Tasker.UpdateTaskFromTable(this);
        }
    }
}