using System;
using HarmonyLib;

namespace Polus.Patches.Permanent {
    [HarmonyPatch(typeof(SaveManager), nameof(SaveManager.LastLanguage), MethodType.Getter)]
    public static class LastLanguageFixPatch {
        [HarmonyPrefix]
        public static bool Awake(out uint __result) {
            try {
                SaveManager.LoadPlayerPrefs();
                if (SaveManager.lastLanguage > 13) SaveManager.lastLanguage = TranslationController.SelectDefaultLanguage();

                __result = SaveManager.lastLanguage;
            } catch {
                __result = 0;
            }

            return false;
        }
    }
}