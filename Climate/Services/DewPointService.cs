using UnityEngine;

namespace Climate
{
    internal static class DewPointService
    {
        const float NOISE_AMP = 3f;

        internal static float GetDewPoint(Vector3 coords, int day)
        {
            // Region profile
            var region = ClimateZones.GetProfile(coords);

            // Seasonal variation
            var seasonal = ClimateZones.GetSeasonalFactor(day) * region.seasonalDewAmplitude;
            
            // Noise
            var airmass =
                (Mathf.PerlinNoise(day * ClimateProfile.AIRMASS_FREQ, region.airmassNoiseSeed) - 0.5f) * 2f;
            var dewNoise = (Mathf.PerlinNoise(day * 0.10f, region.dewNoiseSeed) - 0.5f) * 2f;
            var noise =
                (airmass * ClimateProfile.NOISE_CORRELATION
                + dewNoise * (1f - ClimateProfile.NOISE_CORRELATION))
                * NOISE_AMP;

            return region.baseDew + seasonal + noise;
        }
    }
}
