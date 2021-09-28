using HarmonyLib;

namespace Polus.Patches.Temporary {
    [HarmonyPatch(typeof(HudManager), nameof(HudManager.SetHudActive))]
    public class SetHudActiveFixPatch {
        [HarmonyPrefix]
        public static bool Prefix(HudManager __instance, [HarmonyArgument(0)] ref bool isActive) {
            __instance.UseButton.gameObject.SetActive(SetHudVisibilityPatches.UseButtonEnabled && isActive);
            __instance.UseButton.Refresh();
            __instance.ReportButton.gameObject.SetActive(SetHudVisibilityPatches.ReportButtonDisablePatch.Enabled && isActive);
            __instance.TaskText.transform.parent.gameObject.SetActive(SetHudVisibilityPatches.TaskPanelUpdatePatch.Enabled && isActive);
            __instance.roomTracker.gameObject.SetActive(isActive);
            return false;
        }
    }
}