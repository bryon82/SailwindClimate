using UnityEngine;

namespace Climate
{
    public readonly struct ClimateProfile
    {
        public readonly float tempAmplitude;
        public readonly float baseDew;
        public readonly float seasonalTempAmplitude;
        public readonly float seasonalDewAmplitude;
        public readonly float tempNoiseSeed;
        public readonly float dewNoiseSeed;

        public ClimateProfile(float tempAmplitude, float baseDew, float seasonalTempAmplitude, float seasonalDewAmplitude)
        {
            this.tempAmplitude = tempAmplitude;
            this.baseDew = baseDew;
            this.seasonalTempAmplitude = seasonalTempAmplitude;
            this.seasonalDewAmplitude = seasonalDewAmplitude;

            tempNoiseSeed = seasonalTempAmplitude * 11.3f + tempAmplitude * 4.9f;
            dewNoiseSeed = seasonalDewAmplitude * 8.6f + baseDew * 3.4f;
        }
    }

    public static class ClimateZones
    {
        // Climate profiles for different regions. Temps in Celsius
        public static readonly ClimateProfile AlAnkh = new ClimateProfile(16f, -2f, 6f, 2f);
        public static readonly ClimateProfile Emerald = new ClimateProfile(3f, 26f, 1.5f, 1f);
        public static readonly ClimateProfile Aestrin = new ClimateProfile(6f, 8f, 8f, 6f);

        const float buffer = 1f;
        const float AlAnkhLon = -0.18f;
        const float AestrinLat = 36f;

        const float DaysPerYear = 365f;
        const int PeakDay = 172;

        public static ClimateProfile GetProfile(Vector3 coords)
        {
            var lat = coords.z;
            var lon = coords.x;

            ClimateProfile region;
            if (lon > AlAnkhLon - buffer && lon < AlAnkhLon + buffer)
            {
                var t = Mathf.InverseLerp(AlAnkhLon - buffer, AlAnkhLon + buffer, lon);
                region = Lerp(AlAnkh, Emerald, t);
            }
            else if (lon < AlAnkhLon) 
                region = AlAnkh;
            else 
                region = Emerald;

            if (lat > AestrinLat - buffer && lat < AestrinLat + buffer)
            {
                var t2 = Mathf.InverseLerp(AestrinLat - buffer, AestrinLat + buffer, lat);
                return Lerp(region, Aestrin, t2);
            }
            return lat > AestrinLat ? Aestrin : region;
        }

        static ClimateProfile Lerp(ClimateProfile a, ClimateProfile b, float t) =>
            new ClimateProfile(Mathf.Lerp(a.tempAmplitude, b.tempAmplitude, t),
                Mathf.Lerp(a.baseDew, b.baseDew, t),
                Mathf.Lerp(a.seasonalTempAmplitude, b.seasonalTempAmplitude, t),
                Mathf.Lerp(a.seasonalDewAmplitude, b.seasonalDewAmplitude, t));

        internal static float GetSeasonalFactor(int day)
        {
            var dayOfYear = day % 365;
            return Mathf.Cos(2f * Mathf.PI * (dayOfYear - PeakDay) / DaysPerYear);
        }
    }
}
