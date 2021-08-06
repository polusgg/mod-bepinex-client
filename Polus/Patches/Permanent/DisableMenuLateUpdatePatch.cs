using HarmonyLib;

namespace Polus.Patches.Permanent {
    [HarmonyPatch(typeof(MainMenuManager), nameof(MainMenuManager.LateUpdate))]
    public class DisableMenuLateUpdatePatch {
        [HarmonyPrefix]
        public static bool Main() {
            return false;
        }
    }
}