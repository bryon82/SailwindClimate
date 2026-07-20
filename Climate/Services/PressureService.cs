using UnityEngine;

namespace Climate
{
    internal class PressureService
    {
        internal static float CurrentStormRadius { get; set; }
        internal static float NormalizedDistanceToStorm { get; set; }
        internal static float CurrentStormRange { get; set; }

        const float MIN_PRESSURE = 26f;
        const float MAX_PRESSURE = 31.9f;

        internal static float GetPressure()
        {
            float pressure;
            if (NormalizedDistanceToStorm >= 1f)
            {
                var distanceToStorm = Mathf.Clamp01(
                    (WeatherStorms.currentStormDistance - CurrentStormRadius - CurrentStormRange) / 12000f);
                pressure = Mathf.Lerp(29.7f, MAX_PRESSURE, distanceToStorm);
            }
            else if (NormalizedDistanceToStorm <= 0f)
            {
                var distanceToCenter = Mathf.Clamp01(WeatherStorms.currentStormDistance / CurrentStormRadius);
                pressure = Mathf.Lerp(MIN_PRESSURE, 27.65f, distanceToCenter);
            }
            else
            {
                pressure = Mathf.Lerp(27.65f, 29.7f, NormalizedDistanceToStorm);
            }
            return pressure;
        }

        internal static float GetNormalizedPressure()
        {
            var pressure = GetPressure();
            return Mathf.InverseLerp(MIN_PRESSURE, MAX_PRESSURE, pressure);
        }

        internal static bool IsNearStorm()
        {
            return NormalizedDistanceToStorm < 0.68f;
        }
    }
}
