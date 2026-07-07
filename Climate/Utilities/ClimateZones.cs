using UnityEngine;

namespace Climate
{
    public readonly struct ClimateProfile
    {
        public readonly float baseTempF;
        public readonly float tempAmplitude;
        public readonly float baseDewC;
        public readonly float tempNoiseSeed;
        public readonly float dewNoiseSeed;

        public ClimateProfile(float baseTempF, float tempAmplitude, float baseDewC)
        {
            this.baseTempF = baseTempF;
            this.tempAmplitude = tempAmplitude;
            this.baseDewC = baseDewC;
            // different multipliers so temp and dew-point noise don't sync with each other
            this.tempNoiseSeed = baseTempF * 7.31f;
            this.dewNoiseSeed = baseDewC * 5.77f;
        }
    }

    public static class ClimateZones
    {
        public static readonly ClimateProfile AlAnkh = new ClimateProfile(baseTempF: 82f, tempAmplitude: 29f, baseDewC: -2f);
        public static readonly ClimateProfile Emerald = new ClimateProfile(baseTempF: 86f, tempAmplitude: 5.5f, baseDewC: 26f);
        public static readonly ClimateProfile Aestrin = new ClimateProfile(baseTempF: 63f, tempAmplitude: 11f, baseDewC: 8f);
        public static readonly ClimateProfile NewPort = new ClimateProfile(baseTempF: 75f, tempAmplitude: 9f, baseDewC: 14f);
        public static readonly ClimateProfile FireFish = new ClimateProfile(baseTempF: 89f, tempAmplitude: 4f, baseDewC: 26f);

        public static ClimateProfile GetProfile(Vector3 coords)
        {
            float lat = coords.z;
            float lon = coords.x;
            if (lat > 33f) return Aestrin;
            if (lon < -2f) return AlAnkh;
            if (lat < 28f) return FireFish;
            if (lat < 32f) return Emerald;
            return NewPort;
        }
    }
}
