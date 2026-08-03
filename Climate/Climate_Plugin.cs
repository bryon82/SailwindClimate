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
        public const string PLUGIN_VERSION = "1.2.0";

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
