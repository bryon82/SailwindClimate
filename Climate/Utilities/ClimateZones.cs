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

    internal static class ClimateZones
    {
        // Climate profiles for different regions. Temps in Celsius
        internal static readonly ClimateProfile AlAnkh = new ClimateProfile(16f, -2f, 6f, 2f);
        internal static readonly ClimateProfile Emerald = new ClimateProfile(3f, 26f, 1.5f, 1f);
        internal static readonly ClimateProfile Aestrin = new ClimateProfile(6f, 8f, 8f, 6f);

        const float BUFFER = 1.5f;
        const float AA_EA_LON = -0.18f;
        const float AESTRIN_LAT = 35.2f;

        const float DAYS_PER_YEAR = 365f;
        const int PEAK_DAY = 172;

        internal static ClimateProfile GetProfile(Vector3 coords)
        {
            var lat = coords.z;
            var lon = coords.x;

            ClimateProfile region;
            if (lon > AA_EA_LON - BUFFER && lon < AA_EA_LON + BUFFER)
            {
                var t = Mathf.InverseLerp(AA_EA_LON - BUFFER, AA_EA_LON + BUFFER, lon);
                region = Lerp(AlAnkh, Emerald, t);
            }
            else if (lon < AA_EA_LON) 
                region = AlAnkh;
            else 
                region = Emerald;

            if (lat > AESTRIN_LAT - BUFFER && lat < AESTRIN_LAT + BUFFER)
            {
                var t2 = Mathf.InverseLerp(AESTRIN_LAT - BUFFER, AESTRIN_LAT + BUFFER, lat);
                return Lerp(region, Aestrin, t2);
            }
            return lat > AESTRIN_LAT ? Aestrin : region;
        }

        private static ClimateProfile Lerp(ClimateProfile a, ClimateProfile b, float t) =>
            new ClimateProfile(Mathf.Lerp(a.tempAmplitude, b.tempAmplitude, t),
                Mathf.Lerp(a.baseDew, b.baseDew, t),
                Mathf.Lerp(a.seasonalTempAmplitude, b.seasonalTempAmplitude, t),
                Mathf.Lerp(a.seasonalDewAmplitude, b.seasonalDewAmplitude, t));

        internal static float GetSeasonalFactor(int day)
        {
            var dayOfYear = day % DAYS_PER_YEAR;
            return Mathf.Cos(2f * Mathf.PI * (dayOfYear - PEAK_DAY) / DAYS_PER_YEAR);
        }
    }
}
