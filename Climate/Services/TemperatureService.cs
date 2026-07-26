using UnityEngine;

namespace Climate
{
    internal static class TemperatureService
    {
        const float MIN_TEMP = -12.2222f; // 10°F
        const float MAX_TEMP = 46.1111f;  // 115°F
        const float NOISE_AMP = 3.8889f; // 7°F
        const float REF_LAT = 31f;
        const float REF_TEMP = 30f;
        const float TEMP_LAT_CONV = 1.2f;
        const float LOW_PRESSURE_COOLING_MAX = 5f;
        const float RADIATIVE_COOLING_MAX = 7f;
        const float NIGHT_LENGTH_HOURS = 12f;

        internal static float GetTemperature(Vector3 coords, float time, int day)
        {
            // Base temp and region profile
            var baseTemp = REF_TEMP - (coords.z - REF_LAT) * TEMP_LAT_CONV;
            var region = ClimateZones.GetProfile(coords);

            // Seasonal and diurnal variations
            var seasonal = ClimateZones.GetSeasonalFactor(day) * region.seasonalTempAmplitude;
            var diurnal = Mathf.Sin(time / 24f * Mathf.PI * 2f - Mathf.PI / 2f) * (region.tempAmplitude / 2f);

            // Noise
            var airmass = 
                (Mathf.PerlinNoise(day * ClimateProfile.AIRMASS_FREQ, region.airmassNoiseSeed) - 0.5f) * 2f;
            var tempNoise = (Mathf.PerlinNoise(day * 0.15f, region.tempNoiseSeed) - 0.5f) * 2f;
            var noise = 
                (airmass * ClimateProfile.NOISE_CORRELATION 
                + tempNoise * (1f - ClimateProfile.NOISE_CORRELATION)) 
                * NOISE_AMP;

            // Pressure effects
            var normalizedPressure = PressureService.GetNormalizedPressure(coords, day);
            var pressureCooling = (1f - normalizedPressure) * LOW_PRESSURE_COOLING_MAX;
            var radiativeCooling = normalizedPressure * RADIATIVE_COOLING_MAX * GetNightProgress(time);

            return baseTemp + seasonal + diurnal + noise - pressureCooling - radiativeCooling;
        }

        private static float GetNightProgress(float time)
        {
            if (time > 6f && time < 18f)
                return 0f;

            var hoursSinceSunset = (time + 6f) % 24f;
            return Mathf.SmoothStep(0f, 1f, hoursSinceSunset / NIGHT_LENGTH_HOURS);
        }

        internal static float GetNormalizedTemperature(Vector3 coords, float time, int day)
        {
            var temp = GetTemperature(coords, time, day);
            return Mathf.InverseLerp(MIN_TEMP, MAX_TEMP, temp);
        }
    }
}
