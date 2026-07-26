using System;
using System.Globalization;
using UnityEngine;

namespace Climate
{
    internal struct PressureCell
    {
        internal Vector2 origin;
        internal Vector2 velocity;
        internal float radius;
        internal float intensity; // + for high, - for low, inHg delta from baseline
        internal int spawnDay;
        internal int lifespanDays;

        internal string SaveString()
        {
            FormattableString fs = $"{origin.x}|{origin.y}|{velocity.x}|{velocity.y}|{radius}|{intensity}|{spawnDay}|{lifespanDays}";
            return fs.ToString(CultureInfo.InvariantCulture);
        }
    }
}
