using UnityEngine;

namespace Climate
{
    public class WeatherService
    {
        public static float GetNormalizedPressure()
        {
            return PressureService.GetNormalizedPressure();
        }

        public static float GetPressureMb()
        {
            var pressure = PressureService.GetPressure();
            return pressure * 33.8639f;
        }

        public static float GetPressureInHg()
        {
            return PressureService.GetPressure();
        }

        public static float GetNormalizedTemperature(Vector3 coords)
        {
            return TemperatureService.GetNormalizedTemperature(coords);
        }

        public static float GetTemperatureF(Vector3 coords)
        {
            return TemperatureService.GetTemperature(coords);
        }

        public static float GetTemperatureC(Vector3 coords)
        {
            return TemperatureService.GetTemperatureC(coords);
        }

        public static float GetWindChill(Vector3 coords)
        {
            var tempF = TemperatureService.GetTemperature(coords);
            var windSpeedMph = Wind.currentWind.magnitude * 1.150779f; // Convert knots to mph
            if (tempF > 50f || windSpeedMph < 3f)
                return tempF;

            return GetWindChill(tempF, windSpeedMph);
        }

        private static float GetWindChill(float tempF, float windSpeedMph)
        {            
            var windChill = 35.74f + 0.6215f * tempF - 35.75f * Mathf.Pow(windSpeedMph, 0.16f) + 0.4275f * tempF * Mathf.Pow(windSpeedMph, 0.16f);
            return windChill;
        }

        public static float GetWindChillC(Vector3 coords)
        {
            var windChillF = GetWindChill(coords);
            return ConvertFtoC(windChillF);
        }

        public static float GetHeatIndex(Vector3 coords)
        {
            var tempF = TemperatureService.GetTemperature(coords);
            var humidity = HumidityService.GetRelativeHumidity(coords);
            if (tempF < 80f || humidity < 40f)
                return tempF;

            return GetHeatIndex(tempF, humidity);
        }

        private static float GetHeatIndex(float tempF, float humidity)
        {
            var heatIndex = -42.379f + 2.04901523f * tempF + 10.14333127f * humidity - 0.22475541f * tempF * humidity - 0.00683783f * tempF * tempF - 0.05481717f * humidity * humidity + 0.00122874f * tempF * tempF * humidity + 0.00085282f * tempF * humidity * humidity - 0.00000199f * tempF * tempF * humidity * humidity;
            return heatIndex;
        }

        public static float GetHeatIndexC(Vector3 coords)
        {
            var heatIndexF = GetHeatIndex(coords);
            return ConvertFtoC(heatIndexF);
        }

        public static float GetApparentTemperature(Vector3 coords, float windSpeedMph)
        {
            var tempF = TemperatureService.GetTemperature(coords);
            var humidity = HumidityService.GetRelativeHumidity(coords);
            if (tempF <= 50f && windSpeedMph >= 3f)
                return GetWindChill(tempF, windSpeedMph);
            if (tempF >= 80f && humidity >= 40f)
                return GetHeatIndex(tempF, humidity);
            return tempF;
        }

        public static float GetApparentTemperatureC(Vector3 coords, float windSpeedMph)
        {
            var apparentTempF = GetApparentTemperature(coords, windSpeedMph);
            return ConvertFtoC(apparentTempF);
        }

        public static float GetRelativeHumidity(Vector3 coords)
        {
            return HumidityService.GetRelativeHumidity(coords);
        }

        public static float GetDewPointC(Vector3 coords)
        {
            return DewPointService.GetDewPointC(coords);
        }

        public static float GetDewPointF(Vector3 coords)
        {
            var dewPointC = DewPointService.GetDewPointC(coords);
            return ConvertCtoF(dewPointC);
        }

        private static float ConvertCtoF(float tempC)
        {
            return tempC * 9f / 5f + 32f;
        }

        private static float ConvertFtoC(float tempF)
        {
            return (tempF - 32f) * 5f / 9f;
        }
    }
}
