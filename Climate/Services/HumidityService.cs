using UnityEngine;

namespace Climate
{
    internal static class HumidityService
    {
        internal static float GetRelativeHumidity(Vector3 coords, float time, int day)
        {
            var temp = TemperatureService.GetTemperature(coords, time, day);
            var dew = DewPointService.GetDewPoint(coords, day);

            // Dew point can never physically exceed air temperature
            dew = Mathf.Min(dew, temp);

            return Mathf.Clamp(MagnusRH(temp, dew), 0.05f, 1f);
        }

        private static float MagnusRH(float temp, float dew)
        {
            const float a = 17.625f;
            const float b = 243.04f;
            var numerator = Mathf.Exp((a * dew) / (b + dew));
            var denominator = Mathf.Exp((a * temp) / (b + temp));
            return numerator / denominator;
        }
    }
}
