using UnityEngine;

namespace Climate
{
    internal static class PressureService
    {
        internal static float CurrentStormRadius { get; set; }
        internal static float NormalizedDistanceToStorm { get; set; }
        internal static float CurrentStormRange { get; set; }

        const float MIN_PRESSURE = 26f;
        const float MAX_PRESSURE = 31.9f;
        const float BASELINE = 29.7f;
        const float LIFT_ONSET = 0.6f;  // above this: no meaningful lift
        const float LIFT_FULL = 0.3f;   // at/below this: full lift
        const float STABILITY_ONSET = 0.3f; // normalizedPressure below this: too low for fog
        const float STABILITY_FULL = 0.6f; // normalizedPressure at/above this: fog unimpeded

        internal static float GetPressure(Vector3 coords, int day, bool includeStorm = true)
        {
            var pos = new Vector2(coords.z, coords.x);
            var ambientPressure = BASELINE;

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

                ambientPressure += cell.intensity * falloff * falloff * lifeFactor;
            }

            if (includeStorm)
                ambientPressure -= GetStormDip();

            return Mathf.Clamp(ambientPressure, MIN_PRESSURE, MAX_PRESSURE);
        }

        internal static float GetStormDip()
        {
            if (CurrentStormRadius <= 0f)
                return 0f;

            float pressure;
            if (NormalizedDistanceToStorm <= 0f)
            {
                var distanceToCenter = Mathf.Clamp01(WeatherStorms.currentStormDistance / CurrentStormRadius);
                pressure = Mathf.Lerp(3.7f, 2.05f, distanceToCenter);
            }
            else
                pressure = Mathf.Lerp(2.05f, 0f, NormalizedDistanceToStorm);

            return pressure;
        }

        internal static float GetNormalizedPressure(Vector3 coords, int day, bool includeStorm = true)
        {
            var pressure = GetPressure(coords, day, includeStorm);
            return Mathf.InverseLerp(MIN_PRESSURE, MAX_PRESSURE, pressure);
        }

        internal static float GetLiftFactor(Vector3 coords, int day) =>
            Mathf.InverseLerp(LIFT_ONSET, LIFT_FULL, GetNormalizedPressure(coords, day));

        internal static float GetStabilizingFactor(Vector3 coords, int day) =>
            Mathf.InverseLerp(STABILITY_ONSET, STABILITY_FULL, GetNormalizedPressure(coords, day));
    }
}
