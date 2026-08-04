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

        internal readonly struct WindSample
        {
            internal readonly Vector3 direction;
            internal readonly float speed;

            internal WindSample(Vector3 direction, float speed)
            {
                this.direction = direction;
                this.speed = speed;
            }
        }

        internal static readonly WindSample[,] windGrid = new WindSample[maxLatitude - minLatitude + 1, maxLongitude - minLongitude + 1];

        internal static void UpdateDailyWindField()
        {
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

                    var speed = Mathf.Sqrt(uRot * uRot + vRot * vRot);
                    var direction = speed > 0.0001f ? new Vector3(uRot, 0f, vRot) / speed : Vector3.zero;

                    windGrid[lat - minLatitude, lon - minLongitude] = new WindSample(direction, speed);
                }
            }
        }

        internal static WindSample SampleWind(float lon, float lat)
        {
            var lat0 = Mathf.FloorToInt(lat);
            var lon0 = Mathf.FloorToInt(lon);
            var tLat = lat - lat0;
            var tLon = lon - lon0;

            WindSample Get(int la, int lo)
            {
                if (la >= minLatitude && la <= maxLatitude && lo >= minLongitude && lo <= maxLongitude)
                    return windGrid[la - minLatitude, lo - minLongitude];
                return new WindSample(Vector3.zero, 0f);
            }

            var s00 = Get(lat0, lon0);
            var s01 = Get(lat0, lon0 + 1);
            var s10 = Get(lat0 + 1, lon0);
            var s11 = Get(lat0 + 1, lon0 + 1);

            // Interpolate direction and speed independently, then recombine -
            // avoids artificially damping speed at points where surrounding cells' directions diverge.
            var dirLat0 = Vector3.Lerp(s00.direction, s01.direction, tLon);
            var dirLat1 = Vector3.Lerp(s10.direction, s11.direction, tLon);
            var direction = Vector3.Lerp(dirLat0, dirLat1, tLat);

            var speedLat0 = Mathf.Lerp(s00.speed, s01.speed, tLon);
            var speedLat1 = Mathf.Lerp(s10.speed, s11.speed, tLon);
            var speed = Mathf.Lerp(speedLat0, speedLat1, tLat);

            return new WindSample(direction.sqrMagnitude > 0.0001f ? direction.normalized : Vector3.zero, speed);
        }

        ////// used for debugging
        public static float GetMaxWindSpeed()
        {
            float max = 0f;
            foreach (var sample in windGrid)
            {
                if (sample.speed > max)
                    max = sample.speed;
            }
            return max;
        }

        public static float GetMinWindSpeed()
        {
            float min = 100f;
            foreach (var sample in windGrid)
            {
                if (sample.speed < min)
                    min = sample.speed;
            }
            return min;
        }

        public static void WriteSpeedsToFile()
        {
            var filePath = Path.Combine(Application.persistentDataPath, "windGrid_speeds.csv");
            var rows = windGrid.GetLength(0);
            var cols = windGrid.GetLength(1);

            var sb = new StringBuilder();

            for (int y = 0; y < rows; y++)
            {
                for (int x = 0; x < cols; x++)
                {
                    sb.Append(windGrid[y, x].speed);
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
