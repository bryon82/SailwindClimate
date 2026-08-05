using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine.SceneManagement;

namespace Climate
{
    [BepInPlugin(PLUGIN_GUID, PLUGIN_NAME, PLUGIN_VERSION)]
    public class Climate_Plugin : BaseUnityPlugin
    {
        public const string PLUGIN_GUID = "com.raddude.climate";
        public const string PLUGIN_NAME = "Climate";
        public const string PLUGIN_VERSION = "1.3.0";

        internal static Climate_Plugin Instance { get; private set; }
        private static ManualLogSource _logger;

        internal static void LogDebug(string message) => _logger.LogDebug(message);
        internal static void LogInfo(string message) => _logger.LogInfo(message);
        internal static void LogWarning(string message) => _logger.LogWarning(message);
        internal static void LogError(string message) => _logger.LogError(message);

        internal static WindService.WindSample[,] WindGrid => WindService.windGrid;
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
        public static float GetMaxWindSpeed() => WindService.GetMaxWindSpeed();
        public static float GetMinWindSpeed() => WindService.GetMinWindSpeed();
        public static void WriteWindSpeedsToFile() => WindService.WriteSpeedsToFile();

        // Expose min/max latitude and longitude from WindService
        public static int MaxWindLatitude  { get => WindService.maxLatitude; set => WindService.maxLatitude = value; }
        public static int MinWindLatitude { get => WindService.minLatitude; set => WindService.minLatitude = value; }
        public static int MaxWindLongitude { get => WindService.maxLongitude; set => WindService.maxLongitude = value; }
        public static int MinWindLongitude { get => WindService.minLongitude; set => WindService.minLongitude = value; }

        // Exposed from PressureCell
        public static int MaxPressureCells
        { 
            get => PressureCell.maxPressureCells;
            set => PressureCell.maxPressureCells = value;
        }
        public static int MaxCellSpawnLatitude { get => PressureCell.maxSpawnLatitude; set => PressureCell.maxSpawnLatitude = value; }
        public static int MinCellSpawnLatitude { get => PressureCell.minSpawnLatitude; set => PressureCell.minSpawnLatitude = value; }
        public static int MaxCellSpawnLongitude { get => PressureCell.maxSpawnLongitude; set => PressureCell.maxSpawnLongitude = value; }
        public static int MinCellSpawnLongitude { get => PressureCell.minSpawnLongitude; set => PressureCell.minSpawnLongitude = value; }

        // Expose AddPressureSystem method from PressureSystem
        public static void AddPressureSystem(
            float s_x0, float s_y0, float s_amp, float s_sigmaX, float s_sigmaY, float s_thetaDeg,
            float w_x0, float w_y0, float w_amp, float w_sigmaX, float w_sigmaY, float w_thetaDeg,
            float posWiggle, float ampWiggle) => 
                PressureSystem.AddPressureSystem(
                    s_x0, s_y0, s_amp, s_sigmaX, s_sigmaY, s_thetaDeg,
                    w_x0, w_y0, w_amp, w_sigmaX, w_sigmaY, w_thetaDeg,
                    posWiggle, ampWiggle);


        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            _logger = Logger;

            StartCoroutine(AssetLoader.LoadAssets());

            Configs.InitializeConfigs();

            Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly(), PLUGIN_GUID);
            SceneManager.sceneLoaded += AddShopItems.SceneLoaded;

            Sun.OnNewDay += PressureCell.UpdatePressureCells;
            Sun.OnNewDay += PressureSystem.UpdateAllWiggles;
            Sun.OnNewDay += WindService.UpdateDailyWindField;
            Sun.OnNewDay += DateTextUI.UpdateDateText;
        }
    }
}
