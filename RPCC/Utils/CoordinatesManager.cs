using System;
using ASCOM.Tools;
using ASCOM.Tools.Novas31;
using RPCC.Comms;
using RPCC.Tasks;

namespace RPCC.Utils
{
    public static class CoordinatesManager
    {
        public static readonly Transform Trans = new Transform();
        public static double ObjectDistance2Moon;
        public static double MoonIllumination;
        private static readonly Object3 Moon;   
        // public static Object3 Sun;
        private static readonly OnSurface Observatory;
        

        static CoordinatesManager()
        {
            Observatory.Height = 290;
            Observatory.Latitude = 57.03669;
            Observatory.Longitude = 59.54727;
            Observatory.Pressure = 1000;
            Observatory.Temperature = 0;

            Trans.SiteElevation = Observatory.Height;
            Trans.SiteLatitude = Observatory.Latitude;
            Trans.SiteLongitude = Observatory.Longitude;

            Moon.Name = "Moon";
            Moon.Number = Body.Moon;
            Moon.Type = ObjectType.MajorPlanetSunOrMoon;

            // Sun.Name = "Sun";
            // Sun.Number = Body.Sun;
            // Sun.Type = ObjectType.MajorPlanetSunOrMoon;
        }

        public static void CalculateObjectDistance2Moon(ObservationTask task)
        {
            // Возвращает угловое расстояние в градусах между целью и луной на данный момент
            var jd = Utilities.JulianDateUtc;
            double moonRa = 0, moonDec = 0, moonDist = 0;   
            var stat = Novas.LocalPlanet(jd, Moon, 0.0, Observatory, Accuracy.Reduced,
                ref moonRa, ref moonDec, ref moonDist);
            moonRa *= 15 * Math.PI / 180;
            moonDec *= Math.PI / 180;

            var tarRa = task.Ra * 15 * Math.PI / 180;
            var tarDec = task.Dec * Math.PI / 180;
            ObjectDistance2Moon = Math.Acos(Math.Sin(tarDec)*Math.Sin(moonDec) +
                                            Math.Cos(tarDec)*Math.Cos(moonDec) * Math.Cos(moonRa-tarRa))*180/Math.PI;
            // return Math.Acos(Math.Sin(tarDec)*Math.Sin(moonDec) +
            //                  Math.Cos(tarDec)*Math.Cos(moonDec) * Math.Cos(moonRa-tarRa))*180/Math.PI;
        }

        public static bool CheckElevateLimit(double ra, double dec, DateTime time)
        {
            Trans.SetJ2000(ra, dec);
            // Trans.JulianDateUTC = Novas.JulianDate((short) time.Year, 
            //     (short) time.Month, (short) time.Day, time.Hour+time.Minute/60d + time.Second/3600d);
            Trans.JulianDateUTC = Utilities.JulianDateFromDateTime(time);
            var el = Trans.ElevationTopocentric;
            return el >= 30;
        }
            
        public static double CalculateObjectDistance2Mount(ObservationTask task)
        {   
            /*
              Возвращает угловое расстояние в минутах между направлением трубы и целью
             */
            var mountRa = MountDataCollector.RightAsc * 15 * Math.PI / 18;
            var mountDec= MountDataCollector.Declination * Math.PI / 180;

            var tarRa = task.Ra * 15 * Math.PI / 180;
            var tarDec = task.Dec * Math.PI / 180;
            return (Math.Acos(Math.Sin(tarDec) * Math.Sin(mountDec) +
                              Math.Cos(tarDec) * Math.Cos(mountDec) * Math.Cos(mountRa - tarRa)) * 180 / Math.PI) * 60;
        }
    }
}