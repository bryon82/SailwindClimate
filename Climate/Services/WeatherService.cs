using System;
using UnityEngine;

namespace Climate
{
    /// <summary>
    /// Provides weather-related services such as temperature, pressure, humidity, wind chill, heat 
    /// index, and dew-point calculations.
    /// </summary>
    public class WeatherService
    {
        /// <summary>
        /// Gets the current barometric pressure normalized to the range of 26 - 31.9 inHg.
        /// </summary>
        /// <returns>A float in the range of 0 - 1 representing the pressure.</returns>
        public static float GetCurrentNormalizedPressure() => PressureService.GetNormalizedPressure();

        /// <summary>
        /// Gets the current barometric pressure in millibars.
        /// </summary>
        /// <returns>A float representing the pressure in millibars.</returns>
        public static float GetCurrentPressureMb() => ConvertInHgToMb(GetCurrentPressureInHg());

        /// <summary>
        /// Gets the current barometric pressure in inches of mercury.
        /// </summary>
        /// <returns>A float representing the pressure in inches of mercury.</returns>
        public static float GetCurrentPressureInHg() => PressureService.GetPressure();

        /// <summary>
        /// Gets the temperature normalized to the range of -12.2222 - 40.1111 °C (10 - 115 °F).
        /// </summary>
        /// <param name="coords">Coordinates of the location of where to get the temperature.</param>
        /// <param name="time">The time of day in hours (0 - 24).</param>
        /// <param name="day">The day. Non-negative; internally normalized to a 365-day year.</param>
        /// <returns>A float in the range of 0 - 1 representing the temperature.</returns>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <paramref name="time"/> is not within 0-24, or <paramref name="day"/> is negative.
        /// </exception>
        public static float GetNormalizedTemperature(Vector3 coords, float time, int day)
        {
            ValidateTime(time);
            ValidateDay(day);
            return TemperatureService.GetNormalizedTemperature(coords, time, day);
        }

        /// <summary>
        /// Gets the current temperature normalized to the range of -12.2222 - 40.1111 °C (10 - 115 °F).
        /// </summary>
        /// <param name="coords">Coordinates of the location of where to get the temperature.</param>
        /// <returns>A float in the range of 0 - 1 representing the temperature.</returns>
        public static float GetCurrentNormalizedTemperature(Vector3 coords) =>
            GetNormalizedTemperature(coords, Sun.sun.localTime, GameState.day);

        /// <summary>
        /// Gets the temperature in degrees Celsius.
        /// </summary>
        /// <param name="coords">Coordinates of the location of where to get the temperature.</param>
        /// <param name="time">The time of day in hours (0 - 24).</param>
        /// <param name="day">The day. Non-negative; internally normalized to a 365-day year.</param>
        /// <returns>A float representing the temperature in degrees Celsius.</returns>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <paramref name="time"/> is not within 0-24, or <paramref name="day"/> is negative.
        /// </exception>
        public static float GetTemperatureC(Vector3 coords, float time, int day)
        {
            ValidateTime(time);
            ValidateDay(day);
            return TemperatureService.GetTemperature(coords, time, day);
        }

        /// <summary>
        /// Gets the current temperature in degrees Celsius.
        /// </summary>
        /// <param name="coords">Coordinates of the location of where to get the temperature.</param>
        /// <returns>A float representing the temperature in degrees Celsius.</returns>
        public static float GetCurrentTemperatureC(Vector3 coords) =>
            GetTemperatureC(coords, Sun.sun.localTime, GameState.day);

        /// <summary>
        /// Gets the temperature in degrees Fahrenheit.
        /// </summary>
        /// <param name="coords">Coordinates of the location of where to get the temperature.</param>
        /// <param name="time">The time of day in hours (0 - 24).</param>
        /// <param name="day">The day. Non-negative; internally normalized to a 365-day year.</param>
        /// <returns>A float representing the temperature in degrees Fahrenheit.</returns>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <paramref name="time"/> is not within 0-24, or <paramref name="day"/> is negative.
        /// </exception>
        public static float GetTemperatureF(Vector3 coords, float time, int day) =>
            ConvertCtoF(GetTemperatureC(coords, time, day));

        /// <summary>
        /// Gets the current temperature in degrees Fahrenheit.
        /// </summary>
        /// <param name="coords">Coordinates of the location of where to get the temperature.</param>
        /// <returns>A float representing the temperature in degrees Fahrenheit.</returns>
        public static float GetCurrentTemperatureF(Vector3 coords) => ConvertCtoF(GetCurrentTemperatureC(coords));

        /// <summary>
        /// Gets the wind chill in degrees Celsius.
        /// </summary>
        /// <param name="windSpeedKnots">The wind speed in knots.</param>
        /// <param name="coords">Coordinates of the location of where to get the wind chill.</param>
        /// <param name="time">The time of day in hours (0 - 24).</param>
        /// <param name="day">The day. Non-negative; internally normalized to a 365-day year.</param>
        /// <returns>A float representing the wind chill in degrees Celsius.</returns>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <paramref name="windSpeedKnots"/> is negative, <paramref name="time"/> is not within 0-24,
        /// or <paramref name="day"/> is negative.
        /// </exception>
        public static float GetWindChillC(float windSpeedKnots, Vector3 coords, float time, int day) =>
            ConvertFtoC(GetWindChillF(windSpeedKnots, coords, time, day));

        /// <summary>
        /// Gets the current wind chill in degrees Celsius.
        /// </summary>
        /// <param name="coords">Coordinates of the location of where to get the wind chill.</param>
        /// <returns>A float representing the wind chill in degrees Celsius.</returns>
        public static float GetCurrentWindChillC(Vector3 coords) => ConvertFtoC(GetCurrentWindChillF(coords));

        /// <summary>
        /// Gets the wind chill in degrees Fahrenheit.
        /// </summary>
        /// <param name="windSpeedKnots">The wind speed in knots.</param>
        /// <param name="coords">Coordinates of the location of where to get the wind chill.</param>
        /// <param name="time">The time of day in hours (0 - 24).</param>
        /// <param name="day">The day. Non-negative; internally normalized to a 365-day year.</param>
        /// <returns>A float representing the wind chill in degrees Fahrenheit.</returns>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <paramref name="windSpeedKnots"/> is negative, <paramref name="time"/> is not within 0-24,
        /// or <paramref name="day"/> is negative.
        /// </exception>
        public static float GetWindChillF(float windSpeedKnots, Vector3 coords, float time, int day)
        {
            ValidateWindSpeed(windSpeedKnots);
            ValidateTime(time);
            ValidateDay(day);

            var tempF = GetTemperatureF(coords, time, day);
            var windSpeedMph = ConvertKnotsToMph(windSpeedKnots);
            if (tempF > 50f || windSpeedMph < 3f)
                return tempF;

            return GetWindChill(tempF, windSpeedMph);
        }

        /// <summary>
        /// Gets the current wind chill in degrees Fahrenheit.
        /// </summary>
        /// <param name="coords">Coordinates of the location of where to get the wind chill.</param>
        /// <returns>A float representing the wind chill in degrees Fahrenheit.</returns>
        public static float GetCurrentWindChillF(Vector3 coords) =>
            GetWindChillF(Wind.currentWind.magnitude, coords, Sun.sun.localTime, GameState.day);

        /// <summary>
        /// Gets the heat index in degrees Celsius.
        /// </summary>
        /// <param name="coords">Coordinates of the location of where to get the heat index.</param>
        /// <param name="time">The time of day in hours (0 - 24).</param>
        /// <param name="day">The day. Non-negative; internally normalized to a 365-day year.</param>
        /// <returns>A float representing the heat index in degrees Celsius.</returns>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <paramref name="time"/> is not within 0-24, or <paramref name="day"/> is negative.
        /// </exception>
        public static float GetHeatIndexC(Vector3 coords, float time, int day) =>
            ConvertFtoC(GetHeatIndexF(coords, time, day));

        /// <summary>
        /// Gets the current heat index in degrees Celsius.
        /// </summary>
        /// <param name="coords">Coordinates of the location of where to get the heat index.</param>
        /// <returns>A float representing the heat index in degrees Celsius.</returns>
        public static float GetCurrentHeatIndexC(Vector3 coords) =>
            ConvertFtoC(GetCurrentHeatIndexF(coords));

        /// <summary>
        /// Gets the heat index in degrees Fahrenheit.
        /// </summary>
        /// <param name="coords">Coordinates of the location of where to get the heat index.</param>
        /// <param name="time">The time of day in hours (0 - 24).</param>
        /// <param name="day">The day. Non-negative; internally normalized to a 365-day year.</param>
        /// <returns>A float representing the heat index in degrees Fahrenheit.</returns>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <paramref name="time"/> is not within 0-24, or <paramref name="day"/> is negative.
        /// </exception>
        public static float GetHeatIndexF(Vector3 coords, float time, int day)
        {
            ValidateTime(time);
            ValidateDay(day);

            var tempF = GetTemperatureF(coords, time, day);
            var humidity = HumidityService.GetRelativeHumidity(coords, time, day) * 100;
            if (tempF < 80f || humidity < 40f)
                return tempF;

            return GetHeatIndex(tempF, humidity);
        }

        /// <summary>
        /// Gets the current heat index in degrees Fahrenheit.
        /// </summary>
        /// <param name="coords">Coordinates of the location of where to get the heat index.</param>
        /// <returns>A float representing the heat index in degrees Fahrenheit.</returns>
        public static float GetCurrentHeatIndexF(Vector3 coords) =>
            GetHeatIndexF(coords, Sun.sun.localTime, GameState.day);

        /// <summary>
        /// Gets the apparent temperature in degrees Celsius.
        /// </summary>
        /// <param name="windSpeedKnots">The wind speed in knots.</param>
        /// <param name="coords">Coordinates of the location of where to get the apparent temperature.</param>
        /// <param name="time">The time of day in hours (0 - 24).</param>
        /// <param name="day">The day. Non-negative; internally normalized to a 365-day year.</param>
        /// <returns>A float representing the apparent temperature in degrees Celsius.</returns>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <paramref name="time"/> is not within 0-24, or <paramref name="day"/> is negative.
        /// </exception>
        public static float GetApparentTemperatureC(float windSpeedKnots, Vector3 coords, float time, int day) =>
            ConvertFtoC(GetApparentTemperatureF(windSpeedKnots, coords, time, day));

        /// <summary>
        /// Gets the current apparent temperature in degrees Celsius.
        /// </summary>
        /// <param name="coords">Coordinates of the location of where to get the apparent temperature.</param>
        /// <returns>A float representing the apparent temperature in degrees Celsius.</returns>
        public static float GetCurrentApparentTemperatureC(Vector3 coords) =>
            ConvertFtoC(GetCurrentApparentTemperatureF(coords));

        /// <summary>
        /// Gets the apparent temperature in degrees Fahrenheit.
        /// </summary>
        /// <param name="windSpeedKnots">The wind speed in knots.</param>
        /// <param name="coords">Coordinates of the location of where to get the apparent temperature.</param>
        /// <param name="time">The time of day in hours (0 - 24).</param>
        /// <param name="day">The day. Non-negative; internally normalized to a 365-day year.</param>
        /// <returns>A float representing the apparent temperature in degrees Fahrenheit.</returns>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <paramref name="time"/> is not within 0-24, or <paramref name="day"/> is negative.
        /// </exception>
        public static float GetApparentTemperatureF(float windSpeedKnots, Vector3 coords, float time, int day)
        {
            ValidateWindSpeed(windSpeedKnots);
            ValidateTime(time);
            ValidateDay(day);

            var tempF = GetTemperatureF(coords, time, day);
            var humidity = GetRelativeHumidity(coords, time, day) * 100;
            var windSpeedMph = ConvertKnotsToMph(windSpeedKnots);
            if (tempF <= 50f && windSpeedMph >= 3f)
                return GetWindChill(tempF, windSpeedMph);
            if (tempF >= 80f && humidity >= 40f)
                return GetHeatIndex(tempF, humidity);
            return tempF;
        }

        /// <summary>
        /// Gets the current apparent temperature in degrees Fahrenheit.
        /// </summary>
        /// <param name="coords">Coordinates of the location of where to get the apparent temperature.</param>
        /// <returns>A float representing the apparent temperature in degrees Fahrenheit.</returns>
        public static float GetCurrentApparentTemperatureF(Vector3 coords) =>
            GetApparentTemperatureF(Wind.currentWind.magnitude, coords, Sun.sun.localTime, GameState.day);

        /// <summary>
        /// Gets the relative humidity.
        /// </summary>
        /// <param name="coords">Coordinates of the location of where to get the humidity.</param>
        /// <param name="time">The time of day in hours (0 - 24).</param>
        /// <param name="day">The day. Non-negative; internally normalized to a 365-day year.</param>
        /// <returns>A float in the range of 0 - 1 representing the relative humidity.</returns>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <paramref name="time"/> is not within 0-24, or <paramref name="day"/> is negative.
        /// </exception>
        public static float GetRelativeHumidity(Vector3 coords, float time, int day)
        {
            ValidateTime(time);
            ValidateDay(day);
            return HumidityService.GetRelativeHumidity(coords, time, day);
        }

        /// <summary>
        /// Gets the current relative humidity.
        /// </summary>
        /// <param name="coords">Coordinates of the location of where to get the humidity.</param>
        /// <returns>A float in the range of 0 - 1 representing the relative humidity.</returns>
        public static float GetCurrentRelativeHumidity(Vector3 coords) =>
            HumidityService.GetRelativeHumidity(coords, Sun.sun.localTime, GameState.day);

        /// <summary>
        /// Gets the dew-point in degrees Celsius.
        /// </summary>
        /// <param name="coords">Coordinates of the location of where to get the dew-point.</param>
        /// <param name="day">The day. Non-negative; internally normalized to a 365-day year.</param>
        /// <returns>A float representing the dew-point in degrees Celsius.</returns>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="day"/> is negative.</exception>
        public static float GetDewPointC(Vector3 coords, int day)
        {
            ValidateDay(day);
            return DewPointService.GetDewPoint(coords, day);
        }

        /// <summary>
        /// Gets the current dew-point in degrees Celsius.
        /// </summary>
        /// <param name="coords">Coordinates of the location of where to get the dew-point.</param>
        /// <returns>A float representing the dew-point in degrees Celsius.</returns>
        public static float GetCurrentDewPointC(Vector3 coords) => GetDewPointC(coords, GameState.day);

        /// <summary>
        /// Gets the dew-point in degrees Fahrenheit.
        /// </summary>
        /// <param name="coords">Coordinates of the location of where to get the dew-point.</param>
        /// <param name="day">The day. Non-negative; internally normalized to a 365-day year.</param>
        /// <returns>A float representing the dew-point in degrees Fahrenheit.</returns>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="day"/> is negative.</exception>
        public static float GetDewPointF(Vector3 coords, int day) => ConvertCtoF(GetDewPointC(coords, day));

        /// <summary>
        /// Gets the current dew-point in degrees Fahrenheit.
        /// </summary>
        /// <param name="coords">Coordinates of the location of where to get the dew-point.</param>
        /// <returns>A float representing the dew-point in degrees Fahrenheit.</returns>
        public static float GetCurrentDewPointF(Vector3 coords) => ConvertCtoF(GetCurrentDewPointC(coords));


        //// Parameter validation

        private static void ValidateTime(float time)
        {
            if (time < 0f || time > 24f)
                throw new ArgumentOutOfRangeException(nameof(time), time, "Time must be between 0 and 24 hours (inclusive).");
        }

        private static void ValidateDay(int day)
        {
            if (day < 0)
                throw new ArgumentOutOfRangeException(nameof(day), day, "Day must be non-negative.");
        }

        private static void ValidateWindSpeed(float windSpeedKnots)
        {
            if (windSpeedKnots < 0f)
                throw new ArgumentOutOfRangeException(nameof(windSpeedKnots), windSpeedKnots, "Wind speed cannot be negative.");
        }


        //// Helper calculations

        private static float GetWindChill(float tempF, float windSpeedMph) => 
            35.74f + 0.6215f * tempF - 35.75f * Mathf.Pow(windSpeedMph, 0.16f) + 0.4275f * tempF * Mathf.Pow(windSpeedMph, 0.16f);

        private static float GetHeatIndex(float tempF, float humidity) =>
            -42.379f
            + 2.04901523f * tempF
            + 10.14333127f * humidity
            - 0.22475541f * tempF * humidity
            - 0.00683783f * tempF * tempF
            - 0.05481717f * humidity * humidity
            + 0.00122874f * tempF * tempF * humidity
            + 0.00085282f * tempF * humidity * humidity
            - 0.00000199f * tempF * tempF * humidity * humidity;

        private static float ConvertCtoF(float tempC) => tempC * 9f / 5f + 32f;

        private static float ConvertFtoC(float tempF) => (tempF - 32f) * 5f / 9f;

        private static float ConvertKnotsToMph(float knots) => knots * 1.150779f;

        private static float ConvertInHgToMb(float inHg) => inHg * 33.8639f;
    }
}