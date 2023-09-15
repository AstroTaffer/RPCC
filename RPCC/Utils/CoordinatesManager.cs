using System;
using ASCOM.Tools;
using ASCOM.Tools.Novas31;
using RPCC.Tasks;

namespace RPCC.Utils
{
    public static class CoordinatesManager
    {
        public static readonly Transform Trans = new Transform();
        public static Object3 Moon;
        public static Object3 Sun;
        public static OnSurface Observatory;

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

            Sun.Name = "Sun";
            Sun.Number = Body.Sun;
            Sun.Type = ObjectType.MajorPlanetSunOrMoon;
        }

        public static double CalculateObjectDistance2Moon(ObservationTask task)
        {
            var jd = Utilities.JulianDateUtc;
            double moonRa = 0, moonDec = 0, moonDist = 0;   
            var stat = Novas.LocalPlanet(jd, Moon, 0.0, Observatory, Accuracy.Reduced,
                ref moonRa, ref moonDec, ref moonDist);
            moonRa *= 15 * Math.PI / 180;
            moonDec *= Math.PI / 180;

            var tarRa = task.Ra * 15 * Math.PI / 180;
            var tarDec = task.Dec * Math.PI / 180;
            return Math.Acos(Math.Sin(tarDec)*Math.Sin(moonDec) +
                             Math.Cos(tarDec)*Math.Cos(moonDec) * Math.Cos(moonRa-tarRa))*180/Math.PI;
        }

        public static bool CheckElevateLimit(double ra, double dec, DateTime time)
        {
            Trans.SetJ2000(ra, dec);
            Trans.JulianDateUTC = Utilities.JulianDateFromDateTime(time);
            var el = Trans.ElevationTopocentric;
            return el >= 30;
        }
    }
}