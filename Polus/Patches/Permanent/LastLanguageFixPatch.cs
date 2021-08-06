using System;
using HarmonyLib;
using Steamworks;

namespace Polus.Patches.Permanent {
    [HarmonyPatch(typeof(TranslationController), nameof(TranslationController.SelectDefaultLanguage), MethodType.Getter)]
    public static class LastLanguageFixPatch {
        [HarmonyPrefix]
        public static bool Awake(out uint __result) {
            try {
                if (Enum.TryParse(SteamApps.GetCurrentGameLanguage(), true, out SupportedLangs result))
                    __result = (uint) result;
                else
                    __result = 0;
            } catch {
                __result = 0;
            }

            return false;
        }
    }
}