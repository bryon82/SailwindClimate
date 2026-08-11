using System;
using System.IO;
using System.Linq;
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
            {
                LogDebug($"Resizing windGrid to {numLatitudes}x{numLongitudes}");
                windGrid = new Vector3[numLatitudes, numLongitudes];
            }

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

        internal static Vector3 SampleWind(Vector3 coords) => SampleWind(coords.z, coords.x);

        internal static Vector3 SampleWind(float lat, float lon)
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
            var windSample = SampleWind(coords);
            LogDebug($"Wind check at lat: {coords.z}, lon: {coords.x} - direction:{windSample.normalized} {GetWindDirectionDegrees(windSample.normalized)} magnitude: {windSample.magnitude}");
        }

        public static void WriteWindGridToFile(bool magnitude = false, bool normalized = false, bool degrees = false)
        {
            var filePath = Path.Combine(Application.persistentDataPath, "windGrid.csv");
            var rows = windGrid.GetLength(0) - 1;
            var cols = windGrid.GetLength(1);

            var sb = new StringBuilder();
            sb.Append(" ,");
            sb.AppendLine(String.Join(",", Enumerable.Range(minLongitude, maxLongitude - minLongitude + 1).ToList()));

            for (int y = rows; y >= 0; y--)
            {
                sb.Append($"{minLatitude + y},");
                for (int x = 0; x < cols; x++)
                {
                    var dir = windGrid[y, x];
                    if (normalized && !magnitude && !degrees)
                        sb.Append(dir.normalized);
                    else if (magnitude && !normalized && !degrees)
                        sb.Append($"{dir.magnitude}");                    
                    else if (degrees && !normalized && !magnitude)
                        sb.Append($"{GetWindDirectionDegrees(dir)}");
                    else
                        sb.Append($"{dir.x} {dir.y} {dir.z}");
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

        internal static float GetWindDirectionDegrees(Vector3 dir)
        {
            var angle = Mathf.Atan2(dir.x, dir.z) * Mathf.Rad2Deg;
            angle = (angle + 360f + 180f) % 360f;
            return angle;
        }

        private static string GetWindArrow(Vector3 dir)
        {
            var angle = GetWindDirectionDegrees(dir);

            if (angle >= 348.75f || angle < 11.25f)
                return "↑";
            if (angle < 33.75f)
                return "N↗";
            if (angle < 56.25f)
                return "↗";
            if (angle < 78.75f)
                return "E↗";
            if (angle < 101.25f)
                return "→";
            if (angle < 123.75f)
                return "E↘";
            if (angle < 146.25f)
                return "↘";
            if (angle < 168.75f)
                return "S↘";
            if (angle < 191.25f)
                return "↓";
            if (angle < 213.75f)
                return "S↙";
            if (angle < 236.25f)
                return "↙";
            if (angle < 258.75f)
                return "W↙";
            if (angle < 281.25f)
                return "←";
            if (angle < 303.75f)
                return "W↖";
            if (angle < 326.25f)
                return "↖";
            if (angle < 348.75f)
                return "N↖";

            return "↑";
        }
    }
}
