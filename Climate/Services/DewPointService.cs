using UnityEngine;
using static Climate.Configs;

namespace Climate
{
    internal static class DewPointService
    {
        const float DEW_POINT_NOISE_AMP = 3f;
        private static float NoiseFreq => yearLength.Value == 92 ? 0.397f : 0.1f;

        internal static float GetDewPoint(Vector3 coords, int day)
        {
            // Region profile
            var region = ClimateZones.GetProfile(coords);

            // Seasonal variation
            var seasonal = ClimateZones.GetSeasonalFactor(day) * region.seasonalDewAmplitude;

            // Noise
            var airmass =
                (Mathf.PerlinNoise(day * ClimateProfile.AirMassFreq, region.airmassNoiseSeed) - 0.5f) * 2f;
            var dewNoise = (Mathf.PerlinNoise(day * NoiseFreq, region.dewNoiseSeed) - 0.5f) * 2f;
            var noise =
                (airmass * ClimateProfile.NOISE_CORRELATION
                + dewNoise * (1f - ClimateProfile.NOISE_CORRELATION))
                * DEW_POINT_NOISE_AMP;

            return region.baseDew + seasonal + noise + GetPressureCellMoisture(coords, day);
        }

        internal static float GetPressureCellMoisture(Vector3 coords, int day)
        {
            var pos = new Vector2(coords.z, coords.x);
            var shift = 0f;

            foreach (var cell in PressureCell.cells)
            {
                var age = day - cell.spawnDay;
                if (age < 0f || age > cell.lifespanDays)
                    continue;

                var center = cell.origin + cell.velocity * age;
                var sqrDist = (pos - center).sqrMagnitude;
                if (sqrDist >= cell.radius * cell.radius)
                    continue;

                var falloff = 1f - Mathf.Sqrt(sqrDist) / cell.radius;
                var lifeFactor = Mathf.Sin(Mathf.PI * age / cell.lifespanDays);
                shift += cell.moistureDelta * falloff * falloff * lifeFactor;
            }
            return shift;
        }
    }
}
