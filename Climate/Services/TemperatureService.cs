using UnityEngine;

namespace Climate
{
    internal static class TemperatureService
    {
        internal static float minTemp = -12.2222f; // 10°F
        internal static float maxTemp = 46.1111f;  // 115°F
        const float NoiseAmplitude = 3.8889f; // 7°F

        internal static float GetTemperature(Vector3 coords, float time, int day)
        {
            var region = ClimateZones.GetProfile(coords);
            var seasonal = ClimateZones.GetSeasonalFactor(day % 365) * region.seasonalTempAmplitude;
            var diurnal = Mathf.Sin(time / 24f * Mathf.PI * 2f - Mathf.PI / 2f) * (region.tempAmplitude / 2f);
            var noise = (Mathf.PerlinNoise(day * 0.15f, region.tempNoiseSeed) - 0.5f) * 2f * NoiseAmplitude;
            var lowPressure = PressureService.IsNearStorm() ? Mathf.Lerp(-5f, 0f, PressureService.GetNormalizedPressure()) : 0f;

            return region.baseTemp + seasonal + diurnal + noise + lowPressure;
        }

        internal static float GetNormalizedTemperature(Vector3 coords, float time, int day)
        {
            var temp = GetTemperature(coords, time, day);
            return Mathf.InverseLerp(minTemp, maxTemp, temp);
        }
    }
}
