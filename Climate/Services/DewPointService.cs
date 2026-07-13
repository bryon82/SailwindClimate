using UnityEngine;

namespace Climate
{
    internal static class DewPointService
    {
        const float NoiseAmplitude = 3f;

        internal static float GetDewPoint(Vector3 coords, int day)
        {
            var region = ClimateZones.GetProfile(coords);
            var seasonal = ClimateZones.GetSeasonalFactor(day) * region.seasonalDewAmplitude;
            var noise = (Mathf.PerlinNoise(day * 0.10f, region.dewNoiseSeed) - 0.5f) * 2f * NoiseAmplitude;

            return region.baseDew + seasonal + noise;
        }
    }
}
