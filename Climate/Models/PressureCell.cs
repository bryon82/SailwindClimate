using System.Collections.Generic;
using System.Globalization;
using UnityEngine;
using static Climate.Climate_Plugin;
using static Climate.Configs;

namespace Climate
{
    internal class PressureCell : IModDataSaveable
    {
        internal Vector2 origin;
        internal Vector2 velocity;
        internal float radius;
        internal float intensity; // + for high, - for low, inHg delta from baseline
        internal float moistureDelta;   // + = moist air mass, - = dry air mass, °C dew point shift
        internal int spawnDay;
        internal int lifespanDays;

        public static int maxSpawnLatitude = 42;
        public static int minSpawnLatitude = 28;
        public static int maxSpawnLongitude = 6;
        public static int minSpawnLongitude = -6;

        const float PRESSURE_MOISTURE_CORRELATION = 0.6f; // 0 = independent, 1 = fully linked
        const float INTENSITY_SCALE = 1.5f;
        const float MOIST_MAX = 10f;  // strongly cyclonic -> strongly moist advection
        const float DRY_MAX = -5f;   // strongly anticyclonic -> dry subsiding air

        const float PRESSURE_HIGH_LAT = 40f;
        const float PRESSURE_LOW_LAT = 31f;
        const float PRESSURE_BAND_SEASONAL_SHIFT = 2f;   // degrees the low-band migrates with season
        const float SPAWN_BIAS_STRENGTH = 0.5f; // 0 = fully random, 1 = fully deterministic
        const float LARGE_SCALE_ANOMALY_SCALE = 5f; // roughly matches the systems' typical |amplitude|

        const float WIND_STEERING_STRENGTH = 0.8f; // 0 = pure independent drift, 1 = fully wind-steered
        public static int maxPressureCells = 6;


        internal static readonly List<PressureCell> cells = new List<PressureCell>();

        internal static void UpdatePressureCells()
        {
            cells.RemoveAll(cell => GameState.day - cell.spawnDay > cell.lifespanDays);

            while (cells.Count < maxPressureCells)
            {
                //// shared "cyclonicity" factor: +1 = strongly cyclonic (low, moist), -1 = strongly anticyclonic (high, dry)
                //var cyclonicity = Random.Range(-1f, 1f);
                //var independentIntensity = Random.Range(-1f, 1f);
                //var independentMoisture = Random.Range(-1f, 1f);

                //var intensityRaw = -cyclonicity * PRESSURE_MOISTURE_CORRELATION
                //                  + independentIntensity * (1f - PRESSURE_MOISTURE_CORRELATION);
                //var moistureRaw = cyclonicity * PRESSURE_MOISTURE_CORRELATION
                //                 + independentMoisture * (1f - PRESSURE_MOISTURE_CORRELATION);

                //var newCell = new PressureCell
                //{
                //    origin = new Vector2(Random.Range(28f, 42f), Random.Range(-6f, 6f)),
                //    velocity = new Vector2(Random.Range(-2f, 2f), Random.Range(-2f, 2f)),
                //    radius = Random.Range(4f, 9f),
                //    intensity = intensityRaw * INTENSITY_SCALE,
                //    moistureDelta = moistureRaw >= 0f
                //        ? Mathf.Lerp(0f, MOIST_MAX, moistureRaw)
                //        : Mathf.Lerp(0f, DRY_MAX, -moistureRaw),
                //    spawnDay = GameState.day,
                //    lifespanDays = Random.Range(2, 5)
                //};

                //cells.Add(newCell);

                var lat = Random.Range(28f, 42f);
                var lon = Random.Range(-6f, 6f);

                var pressureSystemInfluence = PressureSystem.GetPressureSystemInfluence(lon, lat, GameState.day);
                var preferredCyclonicity = Mathf.Clamp(-pressureSystemInfluence / LARGE_SCALE_ANOMALY_SCALE, -1f, 1f);
                var randomCyclonicity = Random.Range(-1f, 1f);
                var cyclonicity = Mathf.Lerp(randomCyclonicity, preferredCyclonicity, SPAWN_BIAS_STRENGTH);

                var independentIntensity = Random.Range(-1f, 1f);
                var independentMoisture = Random.Range(-1f, 1f);
                var intensityRaw = -cyclonicity * PRESSURE_MOISTURE_CORRELATION + independentIntensity * (1f - PRESSURE_MOISTURE_CORRELATION);
                var moistureRaw = cyclonicity * PRESSURE_MOISTURE_CORRELATION + independentMoisture * (1f - PRESSURE_MOISTURE_CORRELATION);

                // Steer velocity toward the large-scale wind at this cell's spawn point.
                var windSample = WindService.SampleWind(lon, lat);
                var windDirLatLon = new Vector2(windSample.direction.z, windSample.direction.x); // (lat,lon) - matches PressureCell convention
                var randomDirLatLon = new Vector2(Random.Range(-2, 2), Random.Range(-2, 2)).normalized;

                var steeredDir = windDirLatLon.sqrMagnitude > 0.0001f
                    ? Vector2.Lerp(randomDirLatLon, windDirLatLon.normalized, WIND_STEERING_STRENGTH).normalized
                    : randomDirLatLon;

                var windSpeedNormalized = Mathf.InverseLerp(0f, maxWindSpeed.Value, windSample.speed);
                var driftSpeed = Mathf.Lerp(-2, 2, windSpeedNormalized);

                var newCell = new PressureCell
                {
                    origin = new Vector2(lat, lon),
                    velocity = steeredDir * driftSpeed,
                    radius = Random.Range(4f, 9f),
                    intensity = intensityRaw * INTENSITY_SCALE,
                    moistureDelta = moistureRaw >= 0f
                        ? Mathf.Lerp(0f, MOIST_MAX, moistureRaw)
                        : Mathf.Lerp(0f, DRY_MAX, -moistureRaw),
                    spawnDay = GameState.day,
                    lifespanDays = Random.Range(2, 5)
                };

                cells.Add(newCell);
            }
        }

        string IModDataSaveable.SaveString()
        {
            System.FormattableString fs = $"{origin.x}|{origin.y}|{velocity.x}|{velocity.y}|{radius}|{intensity}|{moistureDelta}|{spawnDay}|{lifespanDays}";
            return fs.ToString(CultureInfo.InvariantCulture);
        }

        internal static void SavePressureCells() =>
            ModData.AddListEntry($"{PLUGIN_GUID}.PressureCells", cells.ToArray());

        internal static void LoadPressureCells()
        {
            var loadedCells = ModData.GetPressureCellListEntry($"{PLUGIN_GUID}.PressureCells");
            cells.Clear();
            cells.AddRange(loadedCells);

            if (cells.Count < maxPressureCells)
                UpdatePressureCells();
        }
    }
}
