using UnityEngine;
using static Climate.Configs;

namespace Climate
{
    internal class DateTextUI
    {
        internal static TextMesh textMesh;

        internal static void UpdateDateText()
        {
            var day = GameState.day % yearLength.Value;
            var year = GameState.day / yearLength.Value;
            if (yearLength.Value == 365 )
            {
                textMesh.text = $"Year: {year}  Day: {day}";
                return;
            }
            var daysPerSeason = Mathf.FloorToInt(yearLength.Value / 4);
            GetSeasonInfo(day, daysPerSeason, out var season, out var seasonDay);
            textMesh.text = $"Year: {year}  Day: {day}    {season} ({seasonDay}/{daysPerSeason})";
        }

        private static void GetSeasonInfo(int day, int daysPerSeason, out string season, out int seasonDay)
        {
            if (day < daysPerSeason)
            {
                season = "Winter";
                seasonDay = day + 1;
            }
            else if (day < 2 * daysPerSeason)
            {
                season = "Spring";
                seasonDay = day - daysPerSeason + 1;
            }
            else if (day < 3 * daysPerSeason)
            {
                season = "Summer";
                seasonDay = day - 2 * daysPerSeason + 1;
            }
            else
            {
                season = "Autumn";
                seasonDay = day - 3 * daysPerSeason + 1;
            }
        }
    }
}
