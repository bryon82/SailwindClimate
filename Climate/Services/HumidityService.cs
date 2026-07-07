using UnityEngine;

namespace Climate
{
    internal static class HumidityService
    {
        internal static float GetRelativeHumidity(Vector3 coords)
        {
            float tempC = TemperatureService.GetTemperatureC(coords);
            float dewC = DewPointService.GetDewPointC(coords);

            // Dew point can never physically exceed air temperature
            dewC = Mathf.Min(dewC, tempC);

            return Mathf.Clamp(MagnusRH(tempC, dewC), 0.05f, 1f);
        }

        private static float MagnusRH(float tempC, float dewC)
        {
            const float a = 17.625f;
            const float b = 243.04f;
            float numerator = Mathf.Exp((a * dewC) / (b + dewC));
            float denominator = Mathf.Exp((a * tempC) / (b + tempC));
            return numerator / denominator;
        }
    }
}
