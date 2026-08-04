using System.Collections.Generic;
using System.Globalization;
using UnityEngine;
using static Climate.Climate_Plugin;
using static Climate.Configs;

namespace Climate
{
    internal readonly struct PressureSystemParams
    {
        internal readonly float x0, y0, amplitude, sigmaX, sigmaY, theta;

        internal PressureSystemParams(float x0, float y0, float amp, float sigmaX, float sigmaY, float thetaDeg)
        {
            this.x0 = x0;
            this.y0 = y0;
            amplitude = amp;
            this.sigmaX = sigmaX;
            this.sigmaY = sigmaY;
            theta = thetaDeg * Mathf.Deg2Rad;
        }
    }

    internal readonly struct PressureSystemSaveData
    {
        internal readonly float dx, dy, da;
        internal PressureSystemSaveData(float dx, float dy, float da)
        {
            this.dx = dx;
            this.dy = dy;
            this.da = da;
        }
    }

    public class PressureSystem : IModDataSaveable
    {
        private readonly PressureSystemParams winter;
        private readonly PressureSystemParams summer;
        private readonly float posWiggle;
        private readonly float ampWiggle;
        private readonly float persistence;

        internal float dx, dy, da; // noise state

        internal static readonly List<PressureSystem> systems = new List<PressureSystem>
        {
            // Azores high 1
            new PressureSystem(
                new PressureSystemParams(-3f, 27f, 3f, 5f, 3f, 45f),
                new PressureSystemParams(-1f, 27f, 1f, 7f, 4f, 45f),
                posWiggle: 0.5f, ampWiggle: 0.25f),

            // Azores high 2
            new PressureSystem(
                new PressureSystemParams(10f, 33f, 5f, 15f, 7.5f, -10f),
                new PressureSystemParams(10f, 36f, 5f, 15f, 7.5f, 10f),
                posWiggle: 2f, ampWiggle: 1f),

            // North America
            new PressureSystem(
                new PressureSystemParams(-17f, 36f, 4f, 4f, 8f, 0f),
                new PressureSystemParams(-14f, 33f, -4f, 5f, 10f, 0f),
                posWiggle: 2f, ampWiggle: 1f),

            // Iceland low 1
            new PressureSystem(
                new PressureSystemParams(-7f, 41f, -3f, 5f, 7f, -45f),
                new PressureSystemParams(-6f, 39f, -2f, 4f, 6f, -30f),
                posWiggle: 3f, ampWiggle: 2f),

            // Iceland low 2
            new PressureSystem(
                new PressureSystemParams(6f, 44f, -5f, 12f, 6f, 0f),
                new PressureSystemParams(12f, 46f, -2f, 10f, 7f, 0f),
                posWiggle: 4f, ampWiggle: 2f),

            // Tropics
            new PressureSystem(
                new PressureSystemParams(10f, 5f, -3f, 20f, 15f, -10f),
                new PressureSystemParams(10f, 20f, -7f, 20f, 15f, 10f),
                posWiggle: 3f, ampWiggle: 1f),
        };

        internal PressureSystem(PressureSystemParams winter, PressureSystemParams summer,
            float posWiggle = 0f, float ampWiggle = 0f, float persistence = 0.93f)
        {
            this.winter = winter;
            this.summer = summer;
            this.posWiggle = posWiggle;
            this.ampWiggle = ampWiggle;
            this.persistence = persistence;
        }

        private void ComputeState(int day, out PressureSystemParams s, out float theta)
        {
            var alpha = 0.5f * (1f - Mathf.Cos(2f * Mathf.PI * day / yearLength.Value));
            s = new PressureSystemParams(
                Mathf.Lerp(winter.x0, summer.x0, alpha),
                Mathf.Lerp(winter.y0, summer.y0, alpha),
                Mathf.Lerp(winter.amplitude, summer.amplitude, alpha),
                Mathf.Lerp(winter.sigmaX, summer.sigmaX, alpha),
                Mathf.Lerp(winter.sigmaY, summer.sigmaY, alpha),
                0f); // theta blended separately below
            theta = Mathf.Lerp(winter.theta, summer.theta, alpha);
        }

        private float ComputeP(
            float x,
            float y,
            PressureSystemParams s,
            float theta,
            out float xp,
            out float yp,
            out float c,
            out float st)
        {
            var x0 = s.x0 + dx;
            var y0 = s.y0 + dy;
            var amplitude = s.amplitude + da;

            var ddx = x - x0;
            var ddy = y - y0;
            c = Mathf.Cos(theta);
            st = Mathf.Sin(theta);
            xp = ddx * c + ddy * st;
            yp = -ddx * st + ddy * c;

            var sx2 = s.sigmaX * s.sigmaX;
            var sy2 = s.sigmaY * s.sigmaY;
            return amplitude * Mathf.Exp(-0.5f * (xp * xp / sx2 + yp * yp / sy2));
        }

        internal float Value(float x, float y, int day)
        {
            ComputeState(day, out var s, out var theta);
            return ComputeP(x, y, s, theta, out _, out _, out _, out _);
        }

        internal void Gradient(float x, float y, int day, out float dPdx, out float dPdy)
        {
            ComputeState(day, out var s, out var theta);
            var p = ComputeP(x, y, s, theta, out var xp, out var yp, out var c, out var st);
            var sx2 = s.sigmaX * s.sigmaX;
            var sy2 = s.sigmaY * s.sigmaY;
            dPdx = p * (-(xp * c / sx2) + (yp * st / sy2));
            dPdy = p * (-(xp * st / sx2) - (yp * c / sy2));
        }

        private static float NextGaussian()
        {
            var u1 = 1f - Random.value;
            var u2 = Random.value;
            return Mathf.Sqrt(-2f * Mathf.Log(u1)) * Mathf.Cos(2f * Mathf.PI * u2);
        }

        internal void UpdateWiggles()
        {
            var p = persistence;
            dx = p * dx + (1f - p) * posWiggle * NextGaussian();
            dy = p * dy + (1f - p) * posWiggle * NextGaussian();
            da = p * da + (1f - p) * ampWiggle * NextGaussian();
        }

        internal static void UpdateAllWiggles()
        {
            foreach (var system in systems)
                system.UpdateWiggles();
        }

        internal static float GetPressureSystemInfluence(float lon, float lat, int day)
        {
            var total = 0f;
            foreach (var system in systems)
                total += system.Value(lon, lat, day);
            return total;
        }

        public static void AddPressureSystem(
            float s_x0, float s_y0, float s_amp, float s_sigmaX, float s_sigmaY, float s_thetaDeg,
            float w_x0, float w_y0, float w_amp, float w_sigmaX, float w_sigmaY, float w_thetaDeg,
            float posWiggle, float ampWiggle)
        {
            var summerParams = new PressureSystemParams(s_x0, s_y0, s_amp, s_sigmaX, s_sigmaY, s_thetaDeg);
            var winterParams = new PressureSystemParams(w_x0, w_y0, w_amp, w_sigmaX, w_sigmaY, w_thetaDeg);
            var system = new PressureSystem(summerParams, winterParams, posWiggle, ampWiggle);
            systems.Add(system);
        }

        string IModDataSaveable.SaveString()
        {
            System.FormattableString fs = $"{dx}|{dy}|{da}";
            return fs.ToString(CultureInfo.InvariantCulture);
        }

        internal static void SavePressureSystems() =>
            ModData.AddListEntry($"{PLUGIN_GUID}.PressureSystems", systems.ToArray());

        internal static void LoadPressureSystems()
        {
            var loadedSystems = ModData.GetPressureSystemListEntry($"{PLUGIN_GUID}.PressureSystems");
            if (loadedSystems.Count == systems.Count)
            {
                for (int i = 0; i < systems.Count; i++)
                {
                    var loaded = loadedSystems[i];
                    var system = systems[i];
                    system.dx = loaded.dx;
                    system.dy = loaded.dy;
                    system.da = loaded.da;
                }
            }
            else
            {
                LogWarning($"Loaded {loadedSystems.Count} systems, expected {systems.Count}. Using default.");
            }

            WindService.UpdateDailyWindField();
        }
    }
}