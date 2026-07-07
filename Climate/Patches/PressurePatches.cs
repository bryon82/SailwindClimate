using HarmonyLib;

namespace Climate
{
    internal class PressurePatches
    {
        [HarmonyPatch(typeof(WeatherStorms), "GetNormalizedDistance")]
        private class GetNormalizedDistancePatch
        {
            public static void Postfix(float __result, WanderingStorm ___currentStorm, float ___currentStormRange)
            {
                if (!GameState.playing)
                    return;
                PressureService.NormalizedDistanceToStorm = __result;
                var stormRadius = ___currentStorm.GetRadius();
                if (PressureService.CurrentStormRadius != stormRadius)
                    PressureService.CurrentStormRadius = stormRadius;
                if (PressureService.CurrentStormRange != ___currentStormRange)
                    PressureService.CurrentStormRange = ___currentStormRange;
            }        
        }
    }
}
