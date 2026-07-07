using UnityEngine;

namespace Climate
{
    internal static class TemperatureService
    {
        internal static float minTempF = 10f;
        internal static float maxTempF = 115f;

        const float NoiseAmplitudeF = 7f;

        internal static float GetTemperature(Vector3 coords)
        {
            var region = ClimateZones.GetProfile(coords);

            var diurnal = Mathf.Sin(Sun.sun.localTime / 24f * Mathf.PI * 2f - Mathf.PI / 2f) * (region.tempAmplitude / 2f);
            var noise = (Mathf.PerlinNoise(GameState.day * 0.15f, region.tempNoiseSeed) - 0.5f) * 2f * NoiseAmplitudeF;
            var lowPressure = PressureService.IsNearStorm() ? Mathf.Lerp(-9, 0, PressureService.GetNormalizedPressure()) : 0f;

            return region.baseTempF + diurnal + noise + lowPressure;
        }

        internal static float GetTemperatureC(Vector3 coords)
        {
            return (GetTemperature(coords) - 32f) * 5f / 9f;
        }

        internal static float GetNormalizedTemperature(Vector3 coords)
        {
            var tempF = GetTemperature(coords);
            return Mathf.InverseLerp(minTempF, maxTempF, tempF);
        }
    }
}
