using HarmonyLib;

namespace Climate
{
    internal static class Extensions
    {
        public static void SetPrivateField(this object obj, string field, object value)
        {
            Traverse.Create(obj).Field(field).SetValue(value);
        }
    }
}
