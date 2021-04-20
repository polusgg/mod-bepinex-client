using HarmonyLib;

namespace PolusggSlim.Patches.Misc
{
    public static class TextMeshProPatch
    {
        [HarmonyPatch(typeof(TextBoxTMP), nameof(TextBoxTMP.IsCharAllowed))]
        public class TextBoxTMP_IsCharSupport
        {
            public static bool Prefix(TextBoxTMP __instance, out bool __result)
            {
                return !(__result = __instance.allowAllCharacters);
            }
        }
    }
}