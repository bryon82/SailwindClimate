using System.IO;
using System.Text;
using UnityEngine;
using static Climate.Climate_Plugin;
using static Climate.Configs;

namespace Climate
{
    internal static class WindService
    {
        const float INFLOW_ANGLE_DEG = 15f;

        public static int minLatitude = 25;
        public static int maxLatitude = 50;
        public static int minLongitude = -15;
        public static int maxLongitude = 35;

        internal static Vector3[,] windGrid = new Vector3[maxLatitude - minLatitude + 1, maxLongitude - minLongitude + 1];

        internal static void UpdateDailyWindField()
        {
            var numLatitudes = maxLatitude - minLatitude + 1;
            var numLongitudes = maxLongitude - minLongitude + 1;
            if (windGrid.GetLength(0) != numLatitudes || windGrid.GetLength(1) != numLongitudes)
                windGrid = new Vector3[numLatitudes, numLongitudes];

            var K = maxWindSpeed.Value;
            var inflow = INFLOW_ANGLE_DEG * Mathf.Deg2Rad;
            var cosA = Mathf.Cos(inflow);
            var sinA = Mathf.Sin(inflow);

            for (var lat = minLatitude; lat <= maxLatitude; lat++)
            {
                for (var lon = minLongitude; lon <= maxLongitude; lon++)
                {
                    float dPdx = 0f, dPdy = 0f;
                    foreach (var system in PressureSystem.systems)
                    {
                        system.Gradient(lon, lat, GameState.day, out var sysDPdx, out var sysDPdy);
                        dPdx += sysDPdx;
                        dPdy += sysDPdy;
                    }

                    var u = -K * dPdy;
                    var v = K * dPdx;

                    var uRot = u * cosA - v * sinA;
                    var vRot = u * sinA + v * cosA;

                    windGrid[lat - minLatitude, lon - minLongitude] = new Vector3(uRot, 0f, vRot);
                }
            }
        }

        internal static Vector3 SampleWind(float lon, float lat)
        {
            if (lat > maxLatitude || lat < minLatitude || lon < minLongitude || lon > maxLongitude)
                return Vector3.zero;
            return windGrid[Mathf.RoundToInt(lat) - minLatitude, Mathf.RoundToInt(lon) - minLongitude];
        }

        ////// used for debugging
        public static float GetMaxWindSpeed()
        {
            float max = 0f;
            foreach (var sample in windGrid)
            {
                if (sample.magnitude > max)
                    max = sample.magnitude;
            }
            return max;
        }

        public static float GetMinWindSpeed()
        {
            float min = 100f;
            foreach (var sample in windGrid)
            {
                if (sample.magnitude < min)
                    min = sample.magnitude;
            }
            return min;
        }

        public static void CheckWindVector()
        {
            var coords = FloatingOriginManager.instance.GetGlobeCoords(Refs.observerMirror.transform);
            var windSample = SampleWind(coords.x, coords.z);

            LogDebug($"Wind check at lat: {coords.z}, lon: {coords.x} - direction: {windSample.normalized} magnitude: {windSample.magnitude}");
        }        

        public static void WriteWindGridToFile(bool magnitude = false, bool normalized = false)
        {
            var filePath = Path.Combine(Application.persistentDataPath, "windGrid_directions.csv");
            var rows = windGrid.GetLength(0);
            var cols = windGrid.GetLength(1);

            var sb = new StringBuilder();

            for (int y = 0; y < rows; y++)
            {
                for (int x = 0; x < cols; x++)
                {
                    var dir = windGrid[y, x];
                    if (normalized)
                        dir = dir.normalized;

                    if (!magnitude)
                        sb.Append($"{dir.x} {dir.y} {dir.z}");
                    else
                        sb.Append($"{dir.magnitude}");
                    if (x < cols - 1)
                        sb.Append(',');
                }
                sb.Append('\n');
            }

            try
            {
                File.WriteAllText(filePath, sb.ToString());
                LogDebug($"[WindGridExporter] Wrote {rows}x{cols} wind speed grid to {filePath}");
            }
            catch (IOException ex)
            {
                LogError($"[WindGridExporter] Failed to write wind grid to {filePath}: {ex.Message}");
            }
        }
    }
}
