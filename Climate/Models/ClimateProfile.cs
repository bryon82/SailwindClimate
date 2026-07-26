namespace Climate
{
    public readonly struct ClimateProfile
    {
        public readonly float tempAmplitude;
        public readonly float baseDew;
        public readonly float seasonalTempAmplitude;
        public readonly float seasonalDewAmplitude;
        public readonly float tempNoiseSeed;
        public readonly float dewNoiseSeed;
        public readonly float airmassNoiseSeed;
        internal static readonly float NOISE_CORRELATION = 0.6f; // 0 = fully independent, 1 = fully locked together
        internal static readonly float AIRMASS_FREQ = 0.12f;

        public ClimateProfile(float tempAmplitude, float baseDew, float seasonalTempAmplitude, float seasonalDewAmplitude)
        {
            this.tempAmplitude = tempAmplitude;
            this.baseDew = baseDew;
            this.seasonalTempAmplitude = seasonalTempAmplitude;
            this.seasonalDewAmplitude = seasonalDewAmplitude;

            tempNoiseSeed = seasonalTempAmplitude * 11.3f + tempAmplitude * 4.9f;
            dewNoiseSeed = seasonalDewAmplitude * 8.6f + baseDew * 3.4f;
            airmassNoiseSeed = seasonalTempAmplitude * 7.1f + seasonalDewAmplitude * 5.3f + baseDew * 2.2f;
        }
    }
}
