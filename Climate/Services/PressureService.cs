using System.Collections.Generic;
using UnityEngine;
using static Climate.Climate_Plugin;

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
        internal static readonly List<PressureCell> cells = new List<PressureCell>();

        internal static float GetPressure(Vector3 coords, int day, bool includeStorm = true)
        {
            var pos = new Vector2(coords.z, coords.x);
            var ambientPressure = BASELINE;

            foreach (var cell in cells)
            {
                var age = day - cell.spawnDay;
                if (age < 0f || age > cell.lifespanDays)
                    continue;

                var center = cell.origin + cell.velocity * age;
                var falloff = Mathf.Clamp01(1f - Vector2.Distance(pos, center) / cell.radius);
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

        internal static bool IsNearStorm() => NormalizedDistanceToStorm < 0.68f;

        internal static void UpdatePressureCells()
        {
            cells.RemoveAll(cell => GameState.day - cell.spawnDay > cell.lifespanDays);

            while (cells.Count < 6)
            {
                var newCell = new PressureCell
                {
                    origin = new Vector2(Random.Range(28f, 42f), Random.Range(-6f, 6f)),
                    velocity = new Vector2(Random.Range(-2f, 2f), Random.Range(-2f, 2f)),
                    radius = Random.Range(4f, 9f),
                    intensity = Random.Range(-1.5f, 1.8f),
                    spawnDay = GameState.day,
                    lifespanDays = Random.Range(2, 6)
                };

                cells.Add(newCell);
            }
        }

        internal static void SavePressureCells() =>        
            ModData.AddPressureCellListEntry($"{PLUGIN_GUID}.PressureCells", cells);

        internal static void LoadPressureCells()
        {
            var loadedCells = ModData.GetPressureCellListEntry($"{PLUGIN_GUID}.PressureCells");
            cells.Clear();
            cells.AddRange(loadedCells);
        }
    }
}
