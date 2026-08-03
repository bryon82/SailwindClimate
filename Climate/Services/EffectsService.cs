using UnityEngine;

namespace Climate
{
    internal static class EffectsService
    {
        const float RH_RAIN_ONSET = 0.90f;
        const float RH_RAIN_SATURATED = 0.99f;

        const float RH_CLOUD_ONSET = 0.75f;
        const float RH_CLOUD_SATURATED = 0.95f;

        const float SPREAD_THRESHOLD = 2f;
        const float FULL_FOG_SPREAD = 1f;

        const float MAX_PHYSICAL_RAIN = 5f; // WeatherSet.particles.rainDensity
        const float MAX_PHYSICAL_CLOUD_DENSITY = 6f; // WeatherSet.particles.cloudDensity
        const float MAX_PHYSICAL_FOG_DENSITY = 0.01f;

        // Aestrin: clear day 0.0012, clear dawn 0.003, cloudy day 0.003, cloudy dawn0.004
        // Emerald: clear day 0.0015, clear dawn 0.002, cloudy day 0.003, cloudy dawn 0.004
        // Al'Ankh: clear day 0.001, clear dawn 0.0025, cloudy day  0.003, cloudy dawn 0.003
        internal static float GetFogDensity(float temp, float dew, float stabilityFactor)
        {
            var spread = temp - dew;

            var spreadFactor = Mathf.InverseLerp(SPREAD_THRESHOLD, FULL_FOG_SPREAD, spread);
            return spreadFactor * stabilityFactor * MAX_PHYSICAL_FOG_DENSITY;
        }

        internal static float GetPhysicalRainDensity(float relativeHumidity, float liftFactor)
        {
            var humidityFactor = Mathf.InverseLerp(RH_RAIN_ONSET, RH_RAIN_SATURATED, relativeHumidity);
            return humidityFactor * Mathf.Clamp01(liftFactor) * MAX_PHYSICAL_RAIN;
        }

        internal static float GetPhysicalCloudDensity(float relativeHumidity, float liftFactor)
        {
            var humidityFactor = Mathf.InverseLerp(RH_CLOUD_ONSET, RH_CLOUD_SATURATED, relativeHumidity);
            return humidityFactor * Mathf.Clamp01(liftFactor) * MAX_PHYSICAL_CLOUD_DENSITY;
        }
    }
}
