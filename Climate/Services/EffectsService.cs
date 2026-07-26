using UnityEngine;

namespace Climate
{
    internal static class EffectsService
    {
        const float RH_RAIN_ONSET = 0.90f;
        const float RH_RAIN_SATURATED = 0.99f;
        
        const float RH_CLOUD_ONSET = 0.75f;
        const float RH_CLOUD_SATURATED = 0.95f;

        const float MAX_PHYSICAL_RAIN = 5f; // WeatherSet.particles.rainDensity
        const float MAX_PHYSICAL_CLOUD_DENSITY = 6f; // WeatherSet.particles.cloudDensity

        const float SPREAD_THRESHOLD = 1.5f;
        const float FULL_FOG_SPREAD = 0.5f;

        const float STABILITY_ONSET = 0.3f; // normalizedPressure below this: too low for fog
        const float STABILITY_FULL = 0.6f; // normalizedPressure at/above this: fog unimpeded

        const float MAX_PHYSICAL_FOG_DENSITY = 0.01f; 

        internal static float GetFogDensity(float temp, float dew, float normalizedPressure)
        {
            var spread = temp - dew;

            var spreadFactor = Mathf.InverseLerp(SPREAD_THRESHOLD, FULL_FOG_SPREAD, spread);
            var stabilityFactor = Mathf.InverseLerp(STABILITY_ONSET, STABILITY_FULL, normalizedPressure);

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
