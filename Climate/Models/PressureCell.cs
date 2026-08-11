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

        public static int maxPressureCells = 6;
        public static int maxSpawnLatitude = 42;
        public static int minSpawnLatitude = 28;
        public static int maxSpawnLongitude = 6;
        public static int minSpawnLongitude = -6;

        const float WIND_CELL_GRADIENT_SCALE = 20f;
        const float WIND_CELL_SAMPLE_DIST = 1f;

        const float PRESSURE_MOISTURE_CORRELATION = 0.6f; // 0 = independent, 1 = fully linked
        const float INTENSITY_SCALE = 1.7f;
        const float MOIST_MAX = 10f;  // strongly cyclonic -> strongly moist advection
        const float DRY_MAX = -5f;   // strongly anticyclonic -> dry subsiding air

        const float SPAWN_BIAS_STRENGTH = 0.5f; // 0 = fully random, 1 = fully deterministic
        const float WIND_STEERING_STRENGTH = 0.8f; // 0 = pure independent drift, 1 = fully wind-steered

        internal static readonly List<PressureCell> cells = new List<PressureCell>();

        internal static void UpdatePressureCells()
        {
            cells.RemoveAll(cell => GameState.day - cell.spawnDay > cell.lifespanDays);

            while (cells.Count < maxPressureCells)
            {
                var lat = Random.Range(minSpawnLatitude, maxSpawnLatitude);
                var lon = Random.Range(minSpawnLongitude, maxSpawnLongitude);

                var pressureSystemInfluence = PressureSystem.GetPressureSystemInfluence(lat, lon, GameState.day);
                var preferredCyclonicity = Mathf.Clamp(-pressureSystemInfluence / maxWindSpeed.Value, -1f, 1f);
                var randomCyclonicity = Random.Range(-1f, 1f);
                var cyclonicity = Mathf.Lerp(randomCyclonicity, preferredCyclonicity, SPAWN_BIAS_STRENGTH);

                var independentIntensity = Random.Range(-1f, 1f);
                var independentMoisture = Random.Range(-1f, 1f);
                var intensityRaw = -cyclonicity * PRESSURE_MOISTURE_CORRELATION + independentIntensity * (1f - PRESSURE_MOISTURE_CORRELATION);
                var moistureRaw = cyclonicity * PRESSURE_MOISTURE_CORRELATION + independentMoisture * (1f - PRESSURE_MOISTURE_CORRELATION);

                // Steer velocity toward the large-scale wind at this cell's spawn point.
                var windSample = WindService.SampleWind(lat, lon);
                var windDirLatLon = new Vector2(windSample.normalized.x, windSample.normalized.z);
                var randomDirLatLon = new Vector2(Random.Range(-2, 2), Random.Range(-2, 2)).normalized;

                var steeredDir = windDirLatLon.sqrMagnitude > 0.0001f
                    ? Vector2.Lerp(randomDirLatLon, windDirLatLon.normalized, WIND_STEERING_STRENGTH).normalized
                    : randomDirLatLon;

                var windSpeedNormalized = Mathf.InverseLerp(0f, maxWindSpeed.Value, windSample.magnitude);
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

        internal static Vector3 GetWindContribution(Vector3 coords)
        {
            float P(Vector3 offset) => PressureService.GetPressure(coords + offset, GameState.day, false);

            var gradLat = (P(new Vector3(0, 0, WIND_CELL_SAMPLE_DIST)) - P(new Vector3(0, 0, -WIND_CELL_SAMPLE_DIST))) / (2f * WIND_CELL_SAMPLE_DIST);
            var gradLon = (P(new Vector3(WIND_CELL_SAMPLE_DIST, 0, 0)) - P(new Vector3(-WIND_CELL_SAMPLE_DIST, 0, 0))) / (2f * WIND_CELL_SAMPLE_DIST);
            var smallScale = Vector3.ClampMagnitude(new Vector3(-gradLat, 0f, gradLon) * WIND_CELL_GRADIENT_SCALE, pressureCellmaxWindContr.Value);

            return smallScale;
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

        public static void CheckPressureCellWindContribution()
        {
            var coords = FloatingOriginManager.instance.GetGlobeCoords(Refs.observerMirror.transform);
            var contribution = GetWindContribution(coords);            
            LogDebug($"PressureCell wind contribution at lat: {coords.z} lon: {coords.x} - direction: {contribution.normalized} {WindService.GetWindDirectionDegrees(contribution.normalized)} magnitude: {contribution.magnitude}");
        }
    }
}
