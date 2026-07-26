using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using UnityEngine;
using static Climate.Climate_Plugin;

namespace Climate
{
    internal class ModData
    {
        public static void AddPressureCellListEntry(string dataName, List<PressureCell> data)
        {
            var sb = new StringBuilder();
            foreach (var item in data)
                sb.AppendLine($"{item.SaveString()}");
            var dataString = sb.ToString();
            if (GameState.modData.ContainsKey(dataName))
                GameState.modData[dataName] = dataString;
            else
                GameState.modData.Add(dataName, dataString);
        }

        public static List<PressureCell> GetPressureCellListEntry(string dataName)
        {
            var result = new List<PressureCell>();
            if (!GameState.modData.ContainsKey(dataName))
            {
                LogWarning($"GetModDataEntry: {dataName} not found in modData");
                return result;
            }
            var dataString = GameState.modData[dataName];
            var lines = dataString.Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var line in lines)
            {
                var parts = line.Trim().Split('|');
                if (parts.Length < 5)
                    continue;
                var port = new PressureCell()
                {
                    origin = new Vector2(float.Parse(parts[0], CultureInfo.InvariantCulture), float.Parse(parts[1], CultureInfo.InvariantCulture)),
                    velocity = new Vector2(float.Parse(parts[2], CultureInfo.InvariantCulture), float.Parse(parts[3], CultureInfo.InvariantCulture)),
                    radius = float.Parse(parts[4], CultureInfo.InvariantCulture),
                    intensity = float.Parse(parts[5], CultureInfo.InvariantCulture),
                    spawnDay = int.Parse(parts[6]),
                    lifespanDays = int.Parse(parts[7])
                };
                result.Add(port);
            }
            return result;
        }
    }
}
