using HarmonyLib;
using System;

namespace Climate
{
    internal class PrefabLoadingPatches
    {
        const int NEW_PREFAB_DIR_SIZE = 825 + 1;

        [HarmonyPatch(typeof(PrefabsDirectory), "PopulateShipItems")]
        internal class PrefabDirectoryPatches
        {
            public static void Prefix(PrefabsDirectory __instance)
            {
                if (__instance.directory.Length <= NEW_PREFAB_DIR_SIZE)
                    Array.Resize(ref __instance.directory, NEW_PREFAB_DIR_SIZE);

                __instance.directory[820] = Items.Barometer;
                __instance.directory[821] = Items.Thermometer;
                __instance.directory[822] = Items.Hygrometer;
                __instance.directory[823] = Items.WinterWindMap;
                __instance.directory[824] = Items.SpringAutumnWindMap;
                __instance.directory[825] = Items.SummerWindMap;
            }
        }
    }
}
