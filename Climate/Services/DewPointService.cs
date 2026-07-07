using UnityEngine;

namespace Climate
{
    internal static class DewPointService
    {
        const float NoiseAmplitudeC = 3f;

        internal static float GetDewPointC(Vector3 coords)
        {
            var region = ClimateZones.GetProfile(coords);
            float noise = (Mathf.PerlinNoise(GameState.day * 0.10f, region.dewNoiseSeed) - 0.5f) * 2f * NoiseAmplitudeC;
            return region.baseDewC + noise;
        }
    }
}
