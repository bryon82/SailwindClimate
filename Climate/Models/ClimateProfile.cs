using static Climate.Configs;

namespace Climate
{
    public readonly struct ClimateProfile
    {
        public readonly float baseDew;
        public readonly float seasonalTempAmplitude;
        public readonly float seasonalDewAmplitude;
        public readonly float pressureCoolingFactor;
        public readonly float tempNoiseSeed;
        public readonly float dewNoiseSeed;
        public readonly float airmassNoiseSeed;
        internal static readonly float NOISE_CORRELATION = 0.6f; // 0 = fully independent, 1 = fully locked together
        internal static float AirMassFreq => yearLength.Value == 92 ? 0.476f : 0.12f;

        public ClimateProfile(float baseDew, float seasonalTempAmplitude, float seasonalDewAmplitude, float pressureCoolingFactor)
        {
            this.baseDew = baseDew;
            this.seasonalTempAmplitude = seasonalTempAmplitude;
            this.seasonalDewAmplitude = seasonalDewAmplitude;
            this.pressureCoolingFactor = pressureCoolingFactor;

            tempNoiseSeed = seasonalTempAmplitude * 11.3f;
            dewNoiseSeed = seasonalDewAmplitude * 8.6f + baseDew * 3.4f;
            airmassNoiseSeed = seasonalTempAmplitude * 7.1f + seasonalDewAmplitude * 5.3f + baseDew * 2.2f;
        }
    }
}
