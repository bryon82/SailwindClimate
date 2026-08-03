using HarmonyLib;
using UnityEngine;

namespace Climate
{
    internal class UIPatches
    {
        [HarmonyPatch(typeof(DayLogs), "Awake")]
        internal class DayLogsAwakePatch
        {
            [HarmonyPostfix]
            public static void Postfix()
            {
                var textObject = GameObject.Instantiate(DayLogs.instance.dayText.gameObject, DayLogs.instance.dayText.transform.parent);
                var textMesh = textObject.GetComponent<TextMesh>();
                textMesh.name = "ClimateDateText";
                textMesh.alignment = TextAlignment.Left;
                textMesh.anchor = TextAnchor.UpperCenter;
                textMesh.transform.localPosition = new Vector3(0.5f, 0.24f, -0.007f);

                DateTextUI.textMesh = textMesh;
                DateTextUI.UpdateDateText();
            }
        }
    }
}
