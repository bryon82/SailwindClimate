using HarmonyLib;

namespace Climate.Patches
{
    internal class SaveLoadPatches
    {
        [HarmonyPatch(typeof(SaveLoadManager))]
        private class SaveLoadManagerPatches
        {
            [HarmonyPrefix]
            [HarmonyPatch("SaveModData")]
            public static void SaveModData()
            {
                PressureCell.SavePressureCells();
                PressureSystem.SavePressureSystems();
            }

            [HarmonyPrefix]
            [HarmonyPatch("LoadModData")]
            public static void LoadModData()
            {
                PressureSystem.LoadPressureSystems();
                PressureCell.LoadPressureCells();                
                DateTextUI.UpdateDateText();
            }
        }
    }
}
