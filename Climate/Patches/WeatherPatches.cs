using HarmonyLib;
using UnityEngine;
using static Climate.Climate_Plugin;
using static Climate.Configs;

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
                var baseGameFog = RenderSettings.fogDensity;

                var coords = FloatingOriginManager.instance.GetGlobeCoords(Refs.observerMirror.transform);
                var temp = TemperatureService.GetTemperature(coords, Sun.sun.localTime, GameState.day);
                var dew = DewPointService.GetDewPoint(coords, GameState.day);
                var stabilityFactor = PressureService.GetStabilizingFactor(coords, GameState.day);
                var target = EffectsService.GetFogDensity(temp, dew, stabilityFactor);

                smoothedFog = Mathf.Lerp(smoothedFog, target, 1f - Mathf.Exp(-SMOOTH_RATE * Time.deltaTime));
                var rainSuppression = Mathf.Clamp01(GameState.rainIntensity / RAIN_SUPPRESSION_THRESHOLD);
                var effectiveFog = smoothedFog * (1f - rainSuppression);

                DebugProps.FogDensity = effectiveFog;
                DebugProps.TargetFogDensity = target;
                DebugProps.ApplyingFogDensity = effectiveFog > baseGameFog;

                RenderSettings.fogDensity = Mathf.Max(baseGameFog, effectiveFog);
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
                var liftFactor = PressureService.GetLiftFactor(coords, GameState.day);

                // --- Rain ---
                var baseGameRainIntensity = GameState.rainIntensity;
                var rainTarget = EffectsService.GetPhysicalRainDensity(rh, liftFactor);
                smoothedRain = Mathf.Lerp(smoothedRain, rainTarget, 1f - Mathf.Exp(-SMOOTH_RATE_RAIN * Time.deltaTime));

                var combinedRain = Mathf.Max(GameState.rainIntensity, smoothedRain);
                if (combinedRain > baseGameRainIntensity)
                {
                    var em =___rain.emission;
                    em.rateOverTime = combinedRain * 75f;

                    var em2 = ___outerRain.emission;
                    em2.rateOverTime = combinedRain * 125f;

                    var em3 = ___rainSplash.emission;
                    em3.rateOverTime = combinedRain * 250f;

                    GameState.rainIntensity = combinedRain;
                }

                // --- Clouds ---
                var cloudTarget = EffectsService.GetPhysicalCloudDensity(rh, liftFactor);
                smoothedCloud = Mathf.Lerp(smoothedCloud, cloudTarget, 1f - Mathf.Exp(-SMOOTH_RATE_CLOUD * Time.deltaTime));

                var lowerEm = ___lowerClouds.emission;
                var baseGameCloudRate = lowerEm.rateOverTime.constant;
                var combinedCloud = Mathf.Max(lowerEm.rateOverTime.constant, smoothedCloud);
                if (combinedCloud > lowerEm.rateOverTime.constant)
                {
                    lowerEm.rateOverTime = combinedCloud;
                    var upperEm = ___upperClouds.emission;
                    upperEm.rateOverTime = combinedCloud * 2f;
                }

                DebugProps.RainIntensity = smoothedRain;
                DebugProps.CloudRate = smoothedCloud;
                DebugProps.TargetRainIntensity = rainTarget;
                DebugProps.TargetCloudRate = cloudTarget;
                DebugProps.ApplyingRainIntensity = smoothedRain > baseGameRainIntensity;
                DebugProps.ApplyingCloudRate = smoothedCloud > baseGameCloudRate;
            }
        }

        [HarmonyPatch(typeof(Wind))]
        internal static class ReplaceWindPatches
        {
            [HarmonyPostfix]
            [HarmonyPatch("Awake")]
            public static void Awake()
            {
                PressureSystem.UpdateAllWiggles();
                WindService.UpdateDailyWindField();
                PressureCell.UpdatePressureCells();
            }

            [HarmonyPrefix]
            [HarmonyPatch("GetCurrentTradeWind")]
            public static bool GetCurrentTradeWind(ref Vector3 __result)
            {
                if (!enableWinds.Value || !GameState.playing)
                    return true;

                return false;
            }

            [HarmonyPrefix]
            [HarmonyPatch("SetNewWindTarget")]
            public static bool SetNewWindTarget(ref Vector3 ___currentWindTarget)
            {
                if (!enableWinds.Value || !GameState.playing)
                    return true;

                var windInstance = Wind.instance;
                var coords = FloatingOriginManager.instance.GetGlobeCoords(Refs.observerMirror.transform);

                // --- Base Winds ---
                var pressureSystemWind = WindService.SampleWind(coords);
                var pressureCellWind = PressureCell.GetWindContribution(coords);
                var combinedWind = pressureSystemWind + pressureCellWind;

                // --- Wind Chaos ---
                var region = Weather.instance.currentRegion;
                var directionChaos = region.windDirChaos;
                var magnitudeChaos = region.windChaos;

                var randomDirection = Random.insideUnitSphere;
                randomDirection.y = 0f;
                randomDirection.Normalize();

                var newWindVector = Vector3.Lerp(randomDirection, combinedWind.normalized, windStability.Value);
                var windVector = Vector3.Lerp(Wind.currentBaseWind.normalized, newWindVector, directionChaos).normalized;

                var windMagnitude = Mathf.Clamp(
                    Random.Range(combinedWind.magnitude - magnitudeChaos, combinedWind.magnitude + magnitudeChaos),
                    windInstance.minimumMagnitude,
                    maxWindSpeed.Value);

                var newBaseWind = windVector * windMagnitude;
                Wind.currentBaseWind = newBaseWind;
                windInstance.outCurrentBaseWind = Wind.currentBaseWind;

                // --- Adjust Wind Magnitude For Storm/Land Distance ---
                var stormDist = Mathf.InverseLerp(13000f, 500f, WeatherStorms.currentStormDistance);
                var stormInfluence = 26f * stormDist;

                var landDist = Mathf.InverseLerp(1500f, 4000f, GameState.distanceToLand);
                var landInfluence = combinedWind.magnitude * landDist * 0.66f;

                var adjSum = Mathf.Min(stormInfluence + landInfluence, 20f);

                if (stormInfluence > 0f)
                    LogInfo($"Wind: storm magnitude is {stormInfluence} lerp is {stormDist}");

                if (landDist > 0f)
                    LogInfo($"Wind: ocean magnitude is {landInfluence} lerp is {landDist}");

                var windTarget = Wind.currentBaseWind.normalized * (Wind.currentBaseWind.magnitude + adjSum);
                ___currentWindTarget = windTarget;

                DebugProps.PressureSystemWind = WindService.WindString(pressureSystemWind);
                DebugProps.PressureCellWind = WindService.WindString(pressureCellWind);
                DebugProps.BaseWind = WindService.WindString(newBaseWind);
                DebugProps.StormWindMagnitude = $"{stormInfluence:F2}";
                DebugProps.LandDistWindMagnitude = $"{landInfluence:F2}";

                return false;
            }
        }
    }
}
