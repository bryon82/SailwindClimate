using BepInEx.Configuration;

namespace Climate
{
    internal class Configs
    {
        internal static ConfigEntry<int> yearLength;
        internal static ConfigEntry<bool> enableWinds;
        internal static ConfigEntry<int> maxWindSpeed;

        internal static void InitializeConfigs()
        {
            var config = Climate_Plugin.Instance.Config;

            yearLength = config.Bind(
                "Settings",
                "Days In A Year",
                92,
                new ConfigDescription("The length of a year in days.", new AcceptableValueList<int>(92, 365)));

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
                20,
                new ConfigDescription(windSpeedDesc, new AcceptableValueRange<int>(1, 40)));
        }
    }
}
