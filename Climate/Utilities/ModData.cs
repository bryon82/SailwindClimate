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
        public static void AddListEntry(string dataName, IModDataSaveable[] data)
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
                if (parts.Length < 9)
                    continue;
                var pressureCell = new PressureCell()
                {
                    origin = new Vector2(float.Parse(parts[0], CultureInfo.InvariantCulture), float.Parse(parts[1], CultureInfo.InvariantCulture)),
                    velocity = new Vector2(float.Parse(parts[2], CultureInfo.InvariantCulture), float.Parse(parts[3], CultureInfo.InvariantCulture)),
                    radius = float.Parse(parts[4], CultureInfo.InvariantCulture),
                    intensity = float.Parse(parts[5], CultureInfo.InvariantCulture),
                    moistureDelta = float.Parse(parts[6], CultureInfo.InvariantCulture),
                    spawnDay = int.Parse(parts[7]),
                    lifespanDays = int.Parse(parts[8])
                };
                result.Add(pressureCell);
            }
            return result;
        }

        public static List<PressureSystemSaveData> GetPressureSystemListEntry(string dataName)
        {
            var result = new List<PressureSystemSaveData>();
            if (!GameState.modData.ContainsKey(dataName))
            {
                LogWarning($"GetModDataEntry: {dataName} not found in modData");
                return result;
            }
            var dataString = GameState.modData[dataName];
            var lines = dataString.Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);
            for (var i = 0; i < lines.Length; i++)
            {
                var parts = lines[i].Trim().Split('|');
                if (parts.Length < 3)
                    continue;
                var pressureSystemSaveData =
                    new PressureSystemSaveData(
                        float.Parse(parts[0], CultureInfo.InvariantCulture),
                        float.Parse(parts[1], CultureInfo.InvariantCulture),
                        float.Parse(parts[2], CultureInfo.InvariantCulture));
                
                result.Add(pressureSystemSaveData);
            }
            return result;
        }
    }
}
