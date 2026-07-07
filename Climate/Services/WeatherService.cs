using UnityEngine;

namespace Climate
{
    /// <summary>
    /// Provides weather-related services such as temperature, pressure, humidity, wind chill, heat index, and dew point calculations.
    /// </summary>
    public class WeatherService
    {
        /// <summary>
        /// Gets the barometric pressure normalized to the range of 26 - 31.9 inHg.
        /// </summary>
        /// <returns>A float in the range of 0 - 1 representing the pressure.</returns>
        public static float GetNormalizedPressure()
        {
            return PressureService.GetNormalizedPressure();
        }

        /// <summary>
        /// Gets the barometric pressure in millibars.
        /// </summary>
        /// <returns>A float representing the pressure in millibars.</returns>
        public static float GetPressureMb()
        {
            var pressure = PressureService.GetPressure();
            return ConvertInHgToMb(pressure);
        }

        /// <summary>
        /// Gets the barometric pressure in inches of mercury.
        /// </summary>
        /// <returns>A float representing the pressure in inches of mercury.</returns>
        public static float GetPressureInHg()
        {
            return PressureService.GetPressure();
        }

        /// <summary>
        /// Gets the temperature normalized to the range of 10 - 115 °F.
        /// </summary>
        /// <param name="coords">Coordinates of the location of where to get the temperature.</param>
        /// <returns>A float in the range of 0 - 1 representing the temperature..</returns>
        public static float GetNormalizedTemperature(Vector3 coords)
        {
            return TemperatureService.GetNormalizedTemperature(coords);
        }

        /// <summary>
        /// Gets the temperature in degrees Farenheit.
        /// </summary>
        /// <param name="coords">Coordinates of the location of where to get the temperature.</param>
        /// <returns>A float representing the temperature in degrees Farenheit.</returns>
        public static float GetTemperatureF(Vector3 coords)
        {
            return TemperatureService.GetTemperature(coords);
        }

        /// <summary>
        /// Gets the temperature in degrees Celcius.
        /// </summary>
        /// <param name="coords">Coordinates of the location of where to get the temperature.</param>
        /// <returns>A float representing the temperature in degrees Celcius.</returns>
        public static float GetTemperatureC(Vector3 coords)
        {
            return TemperatureService.GetTemperatureC(coords);
        }

        /// <summary>
        /// Gets the temperature in degrees Kelvin.
        /// </summary>
        /// <param name="coords">Coordinates of the location of where to get the temperature.</param>
        /// <returns>A float representing the temperature in degrees Kelvin.</returns>
        public static float GetTemperatureK(Vector3 coords)
        {
            var tempC = TemperatureService.GetTemperatureC(coords);
            return ConvertCtoK(tempC);
        }

        /// <summary>
        /// Gets the wind chill in degrees Farenheit.
        /// </summary>
        /// <param name="coords">Coordinates of the location of where to get the wind chill.</param>
        /// <returns>A float representing the wind chill in degrees Farenheit.</returns>
        public static float GetWindChillF(Vector3 coords)
        {
            var tempF = TemperatureService.GetTemperature(coords);
            var windSpeedMph = ConvertKnotsToMph(Wind.currentWind.magnitude);
            if (tempF > 50f || windSpeedMph < 3f)
                return tempF;

            return GetWindChill(tempF, windSpeedMph);
        }

        /// <summary>
        /// Gets the wind chill in degrees Celcius.
        /// </summary>
        /// <param name="coords">Coordinates of the location of where to get the wind chill.</param>
        /// <returns>A float representing the wind chill in degrees Celcius.</returns>
        public static float GetWindChillC(Vector3 coords)
        {
            var windChillF = GetWindChillF(coords);
            return ConvertFtoC(windChillF);
        }

        /// <summary>
        /// Gets the heat index in degrees Farenheit.
        /// </summary>
        /// <param name="coords">Coordinates of the location of where to get the heat index.</param>
        /// <returns>A float representing the heat index in degrees Farenheit.</returns>
        public static float GetHeatIndexF(Vector3 coords)
        {
            var tempF = TemperatureService.GetTemperature(coords);
            var humidity = HumidityService.GetRelativeHumidity(coords);
            if (tempF < 80f || humidity < 40f)
                return tempF;

            return GetHeatIndex(tempF, humidity);
        }

        /// <summary>
        /// Gets the heat index in degrees Celcius.
        /// </summary>
        /// <param name="coords">Coordinates of the location of where to get the heat index.</param>
        /// <returns>A float representing the heat index in degrees Celcius.</returns>
        public static float GetHeatIndexC(Vector3 coords)
        {
            var heatIndexF = GetHeatIndexF(coords);
            return ConvertFtoC(heatIndexF);
        }

        /// <summary>
        /// Gets the apparent temperature in degrees Farenheit.
        /// </summary>
        /// <param name="coords">Coordinates of the location of where to get the apparent temperature.</param>
        /// <returns>A float representing the apparent temperature in degrees Farenheit.</returns>
        public static float GetApparentTemperatureF(Vector3 coords)
        {
            var tempF = TemperatureService.GetTemperature(coords);
            var humidity = HumidityService.GetRelativeHumidity(coords);
            var windSpeedMph = ConvertKnotsToMph(Wind.currentWind.magnitude);
            if (tempF <= 50f && windSpeedMph >= 3f)
                return GetWindChill(tempF, windSpeedMph);
            if (tempF >= 80f && humidity >= 40f)
                return GetHeatIndex(tempF, humidity);
            return tempF;
        }

        /// <summary>
        /// Gets the apparent temperature in degrees Celcius.
        /// </summary>
        /// <param name="coords">Coordinates of the location of where to get the apparent temperature.</param>
        /// <returns>A float representing the apparent temperature in degrees Celcius.</returns>
        public static float GetApparentTemperatureC(Vector3 coords)
        {
            var apparentTempF = GetApparentTemperatureF(coords);
            return ConvertFtoC(apparentTempF);
        }

        /// <summary>
        /// Gets the relative humidity.
        /// </summary>
        /// <param name="coords">Coordinates of the location of where to get the humidity.</param>
        /// <returns>A float representing the relative humidity.</returns>
        public static float GetRelativeHumidity(Vector3 coords)
        {
            return HumidityService.GetRelativeHumidity(coords);
        }

        /// <summary>
        /// Gets the dew point in degrees Celcius.
        /// </summary>
        /// <param name="coords">Coordinates of the location of where to get the dew point.</param>
        /// <returns>A float representing the dew point in degrees Celcius.</returns>
        public static float GetDewPointC(Vector3 coords)
        {
            return DewPointService.GetDewPointC(coords);
        }

        /// <summary>
        /// Gets the dew point in degrees Farenheit.
        /// </summary>
        /// <param name="coords">Coordinates of the location of where to get the dew point.</param>
        /// <returns>A float representing the dew point in degrees Farenheit.</returns>
        public static float GetDewPointF(Vector3 coords)
        {
            var dewPointC = DewPointService.GetDewPointC(coords);
            return ConvertCtoF(dewPointC);
        }


        //// Helper calculations

        private static float GetWindChill(float tempF, float windSpeedMph)
        {
            var windChill = 35.74f + 0.6215f * tempF - 35.75f * Mathf.Pow(windSpeedMph, 0.16f) + 0.4275f * tempF * Mathf.Pow(windSpeedMph, 0.16f);
            return windChill;
        }

        private static float GetHeatIndex(float tempF, float humidity)
        {
            var heatIndex =
                -42.379f
                + 2.04901523f * tempF
                + 10.14333127f * humidity
                - 0.22475541f * tempF * humidity
                - 0.00683783f * tempF * tempF
                - 0.05481717f * humidity * humidity
                + 0.00122874f * tempF * tempF * humidity
                + 0.00085282f * tempF * humidity * humidity
                - 0.00000199f * tempF * tempF * humidity * humidity;

            return heatIndex;
        }

        private static float ConvertCtoF(float tempC)
        {
            return tempC * 9f / 5f + 32f;
        }

        private static float ConvertCtoK(float tempC)
        {
            return tempC + 273.15f;
        }

        private static float ConvertFtoC(float tempF)
        {
            return (tempF - 32f) * 5f / 9f;
        }

        private static float ConvertKnotsToMph(float knots)
        {
            return knots * 1.150779f;
        }

        private static float ConvertInHgToMb(float inHg)
        {
            return inHg * 33.8639f;
        }
    }
}
