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
            const float SMALL_SCALE_GRADIENT_SCALE = 40f;
            const float SMALL_SCALE_MAX_CONTRIBUTION = 0.3f;
            const float GRADIENT_SAMPLE_DIST = 1f;

            [HarmonyPostfix]
            [HarmonyPatch("Awake")]
            public static void Awake()
            {
                PressureCell.UpdatePressureCells();
                PressureSystem.UpdateAllWiggles();
                WindService.UpdateDailyWindField();
            }

            [HarmonyPrefix]
            [HarmonyPatch("GetCurrentTradeWind")]
            public static bool GetCurrentTradeWind(ref Vector3 __result)
            {
                if (!enableWinds.Value || !GameState.playing)
                    return true;

                var coords = FloatingOriginManager.instance.GetGlobeCoords(Refs.observerMirror.transform);

                var largeScale = WindService.SampleWind(coords.x, coords.z);

                float P(Vector3 offset) => PressureService.GetPressure(coords + offset, GameState.day, false);

                var gradLat = (P(new Vector3(0, 0, GRADIENT_SAMPLE_DIST)) - P(new Vector3(0, 0, -GRADIENT_SAMPLE_DIST))) / (2f * GRADIENT_SAMPLE_DIST);
                var gradLon = (P(new Vector3(GRADIENT_SAMPLE_DIST, 0, 0)) - P(new Vector3(-GRADIENT_SAMPLE_DIST, 0, 0))) / (2f * GRADIENT_SAMPLE_DIST);
                var smallScale = Vector3.ClampMagnitude(new Vector3(-gradLat, 0f, gradLon) * SMALL_SCALE_GRADIENT_SCALE, SMALL_SCALE_MAX_CONTRIBUTION);

                var combined = largeScale + smallScale;
                __result = combined.sqrMagnitude > 0.0001f ? combined.normalized : Vector3.zero;
                return false;
            }

            [HarmonyPrefix]
            [HarmonyPatch("SetNewWindTarget")]
            public static bool SetNewWindTarget(ref Vector3 ___currentWindTarget)
            {
                if (!enableWinds.Value || !GameState.playing)
                    return true;

                var coords = FloatingOriginManager.instance.GetGlobeCoords(Refs.observerMirror.transform);
                var largeScaleSample = WindService.SampleWind(coords.x, coords.z);

                var windInstance = Wind.instance;

                // --- Wind Chaos ---
                var directionChaos = Weather.instance.currentRegion.windDirChaos;
                var magnitudeChaos = Weather.instance.currentRegion.windChaos;
                var vector = Vector3.Lerp(new Vector3(Random.Range(-1f, 1f), 0f, Random.Range(-1f, 1f)).normalized, largeScaleSample.normalized, windStability.Value);
                var vectorAdj = Vector3.Lerp(Wind.currentBaseWind.normalized, vector.normalized, directionChaos).normalized;

                var num = Random.Range(largeScaleSample.magnitude - magnitudeChaos, largeScaleSample.magnitude + magnitudeChaos);
                if (num < windInstance.minimumMagnitude)
                    num = windInstance.minimumMagnitude;
                else if (num > maxWindSpeed.Value)
                    num = maxWindSpeed.Value;

                Wind.currentBaseWind = vectorAdj * num;
                windInstance.outCurrentBaseWind = Wind.currentBaseWind;

                // --- Adjust Wind Magnitude For Storm/Land Distance ---
                var adjSum = 0f;

                var stormDist = Mathf.InverseLerp(13000f, 500f, WeatherStorms.currentStormDistance);
                var stormInfluence = 26f * stormDist;
                adjSum += stormInfluence;
                if (stormInfluence > 0f)
                    LogInfo($"Wind: storm magnitude is {stormInfluence} lerp is {stormDist}");

                var landDist = Mathf.InverseLerp(1500f, 4000f, GameState.distanceToLand);
                var landInfluence = largeScaleSample.magnitude * landDist * 0.66f;
                adjSum += landInfluence;
                if (landDist > 0f)
                    LogInfo($"Wind: ocean magnitude is {landInfluence} lerp is {landDist}");

                if (adjSum > 20f)
                    adjSum = 20f;

                var windTarget = Wind.currentBaseWind.normalized * (Wind.currentBaseWind.magnitude + adjSum);
                ___currentWindTarget = windTarget;

                return false;
            }
        }
    }
}
