using UnityEngine;
using static Climate.Configs;

namespace Climate
{
    internal static class ClimateZones
    {
        // Temps in Celsius. baseDew, seasonalTempAmp, seasonalDewAmp, pressureCoolingFactor
        internal static readonly ClimateProfile AlAnkh = new ClimateProfile(-2f, 6f, 2f, 1f);
        internal static readonly ClimateProfile Emerald = new ClimateProfile(23f, 1.5f, 1f, 0.3f);
        internal static readonly ClimateProfile Aestrin = new ClimateProfile(10f, 8f, 6f, 0.6f);

        const float BLEND_BUFFER = 1.5f;
        const float AA_EA_LON = -0.18f;
        const float AESTRIN_LAT = 35.2f;

        private static int PeakDay => yearLength.Value == 92 ? 43 : 172;

        internal static ClimateProfile GetProfile(Vector3 coords)
        {
            var lat = coords.z;
            var lon = coords.x;

            ClimateProfile region;
            if (lon > AA_EA_LON - BLEND_BUFFER && lon < AA_EA_LON + BLEND_BUFFER)
            {
                var t = Mathf.InverseLerp(AA_EA_LON - BLEND_BUFFER, AA_EA_LON + BLEND_BUFFER, lon);
                region = Lerp(AlAnkh, Emerald, t);
            }
            else if (lon < AA_EA_LON) 
                region = AlAnkh;
            else 
                region = Emerald;

            if (lat > AESTRIN_LAT - BLEND_BUFFER && lat < AESTRIN_LAT + BLEND_BUFFER)
            {
                var t2 = Mathf.InverseLerp(AESTRIN_LAT - BLEND_BUFFER, AESTRIN_LAT + BLEND_BUFFER, lat);
                return Lerp(region, Aestrin, t2);
            }
            return lat > AESTRIN_LAT ? Aestrin : region;
        }

        private static ClimateProfile Lerp(ClimateProfile a, ClimateProfile b, float t) =>
            new ClimateProfile(
                Mathf.Lerp(a.baseDew, b.baseDew, t),
                Mathf.Lerp(a.seasonalTempAmplitude, b.seasonalTempAmplitude, t),
                Mathf.Lerp(a.seasonalDewAmplitude, b.seasonalDewAmplitude, t),
                Mathf.Lerp(a.pressureCoolingFactor, b.pressureCoolingFactor, t));

        internal static float GetSeasonalFactor(int day)
        {
            var dayOfYear = day % yearLength.Value;
            return Mathf.Cos(2f * Mathf.PI * (dayOfYear - PeakDay) / yearLength.Value);
        }
    }
}
