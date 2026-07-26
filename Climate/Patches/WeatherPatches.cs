using HarmonyLib;
using UnityEngine;
using static Climate.Climate_Plugin;

namespace Climate
{
    internal class WeatherPatches
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

        [HarmonyPatch(typeof(OceanColorBlender), "ApplyPalette")]
        internal static class FogOverlayPatch
        {
            private static float smoothedFog;
            const float SMOOTH_RATE = 0.15f;
            const float RAIN_SUPPRESSION_THRESHOLD = 0.5f; // rainIntensity at which fog is fully suppressed

            public static void Postfix()
            {
                if (!GameState.playing)
                    return;

                var coords = FloatingOriginManager.instance.GetGlobeCoords(Refs.observerMirror.transform);
                var temp = TemperatureService.GetTemperature(coords, Sun.sun.localTime, GameState.day);
                var dew = DewPointService.GetDewPoint(coords, GameState.day);
                var normalizedPressure = PressureService.GetNormalizedPressure(coords, GameState.day);
                var target = EffectsService.GetFogDensity(temp, dew, normalizedPressure);

                smoothedFog = Mathf.Lerp(smoothedFog, target, 1f - Mathf.Exp(-SMOOTH_RATE * Time.deltaTime));
                var rainSuppression = Mathf.Clamp01(GameState.rainIntensity / RAIN_SUPPRESSION_THRESHOLD);
                var effectiveFog = smoothedFog * (1f - rainSuppression);

                RenderSettings.fogDensity = Mathf.Max(RenderSettings.fogDensity, effectiveFog);

                if (effectiveFog > RenderSettings.fogDensity)                
                    RenderSettings.fogDensity = effectiveFog;
            }
        }

        [HarmonyPatch(typeof(Weather), "ApplyWeather")]
        internal static class RainOverlayPatch
        {
            private static float smoothedRain;
            private static float smoothedCloud;
            const float SMOOTH_RATE_RAIN = 0.1f;
            const float SMOOTH_RATE_CLOUD = 0.08f; // clouds build slightly slower than rain hits

            public static void Postfix(
                ParticleSystem ___rain,
                ParticleSystem ___outerRain,
                ParticleSystem ___rainSplash,
                ParticleSystem ___lowerClouds,
                ParticleSystem ___upperClouds)
            {
                if (!GameState.playing)
                    return;

                var coords = FloatingOriginManager.instance.GetGlobeCoords(Refs.observerMirror.transform);

                var rh = HumidityService.GetRelativeHumidity(coords, Sun.sun.localTime, GameState.day);
                var liftFactor = 1f - PressureService.GetNormalizedPressure(coords, GameState.day);

                // --- Rain ---
                var rainTarget = EffectsService.GetPhysicalRainDensity(rh, liftFactor);
                smoothedRain = Mathf.Lerp(smoothedRain, rainTarget, 1f - Mathf.Exp(-SMOOTH_RATE_RAIN * Time.deltaTime));

                var combinedRain = Mathf.Max(GameState.rainIntensity, smoothedRain);
                if (combinedRain > GameState.rainIntensity)
                {
                    var em = ___rain.emission; em.rateOverTime = combinedRain * 75f;
                    var em2 = ___outerRain.emission; em2.rateOverTime = combinedRain * 125f;
                    var em3 = ___rainSplash.emission; em3.rateOverTime = combinedRain * 250f;
                    GameState.rainIntensity = combinedRain;
                }

                // --- Clouds ---
                var cloudTarget = EffectsService.GetPhysicalCloudDensity(rh, liftFactor);
                smoothedCloud = Mathf.Lerp(smoothedCloud, cloudTarget, 1f - Mathf.Exp(-SMOOTH_RATE_CLOUD * Time.deltaTime));

                var lowerEm = ___lowerClouds.emission;
                var combinedCloud = Mathf.Max(lowerEm.rateOverTime.constant, smoothedCloud);
                if (combinedCloud > lowerEm.rateOverTime.constant)
                {
                    lowerEm.rateOverTime = combinedCloud;
                    var upperEm = ___upperClouds.emission;
                    upperEm.rateOverTime = combinedCloud * 2f;
                }
            }
        }
    }
}
