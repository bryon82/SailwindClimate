using System.Collections.Generic;
using UnityEngine;

namespace Climate
{
    internal class DebugProps
    {
        internal static Vector3[,] WindGrid => WindService.windGrid;
        internal static List<PressureCell> PressureCells => PressureCell.cells;
        public static float FogDensity { get; internal set; }
        public static float TargetFogDensity { get; internal set; }
        public static float RainIntensity { get; internal set; }
        public static float TargetRainIntensity { get; internal set; }
        public static float CloudRate { get; internal set; }
        public static float TargetCloudRate { get; internal set; }
        public static bool ApplyingFogDensity { get; internal set; }
        public static bool ApplyingRainIntensity { get; internal set; }
        public static bool ApplyingCloudRate { get; internal set; }
        public static string PressureSystemWind { get; internal set; }
        public static string PressureCellWind { get; internal set; }
        public static string BaseWind { get; internal set; }
        public static string StormWindMagnitude { get; internal set; }
        public static string LandDistWindMagnitude { get; internal set; }
        public static float GetMaxWindSpeed() => WindService.GetMaxWindSpeed();
        public static float GetMinWindSpeed() => WindService.GetMinWindSpeed();
        public static void WriteWindGridToFile() => WindService.WriteWindGridToFile();
        public static void WriteWindGridNormalizedToFile() => WindService.WriteWindGridToFile(normalized: true);
        public static void WriteWindGridMagnitudeToFile() => WindService.WriteWindGridToFile(magnitude: true);
        public static void WriteWindGridDegreesToFile() => WindService.WriteWindGridToFile(degrees: true);
        public static void CheckWindVector() => WindService.CheckWindVector();
        public static void CheckPressureCellWindContribution() => PressureCell.CheckPressureCellWindContribution();
        public static void CheckPressureSystemInfluence() => PressureSystem.CheckPressureSystemInfluence();        
    }
}
