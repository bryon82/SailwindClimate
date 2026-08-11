using BepInEx.Configuration;

namespace Climate
{
    internal class Configs
    {
        internal static ConfigEntry<int> yearLength;
        internal static ConfigEntry<bool> enableWinds;
        internal static ConfigEntry<int> maxWindSpeed;
        internal static ConfigEntry<float> windStability;
        internal static ConfigEntry<int> pressureCellmaxWindContr;

        internal static void InitializeConfigs()
        {
            var config = Climate_Plugin.Instance.Config;

            var yearLengthDesc =
                "The length of a year in days. Affects the length of seasons and the timing of weather patterns.";
            yearLength = config.Bind(
                "Settings",
                "Days In A Year",
                92,
                new ConfigDescription(yearLengthDesc, new AcceptableValueList<int>(92, 365)));

            enableWinds = config.Bind(
                "Settings",
                "Enable Custom Winds",
                true,
                "Disables the default wind system and enables the custom wind system.");

            var windSpeedDesc = 
                "The maximum possible trade wind speed. Other factors will also influence this speed, " +
                "think of this as the maximum baseline wind speed. You will need to wait until midnight " +
                "or save and reload the game for changes to take effect.";
            maxWindSpeed = config.Bind(
                "Wind Settings",
                "Maximum Trade Wind Speed",
                22,
                new ConfigDescription(windSpeedDesc, new AcceptableValueRange<int>(1, 40)));

            var pressureCellMaxContrDesc =
                "This is the maximum wind speed that can be added to the base trade winds from a pressure cell.";
            pressureCellmaxWindContr = config.Bind(
                "Wind Settings",
                "Pressure Cell Max Wind Contribution",
                8,
                new ConfigDescription(pressureCellMaxContrDesc, new AcceptableValueRange<int>(1, 20)));

            var stabilityDesc =
                "A value of 0 means the winds are completely chaotic, while a value of 1 means the winds will " +
                "nearly always align with the trade winds. Base game has this set to 0.25.";
            windStability = config.Bind(
                "Wind Settings",
                "Wind Stability",
                0.45f,
                new ConfigDescription(stabilityDesc, new AcceptableValueRange<float>(0f, 1f)));
        }
    }
}
