using HarmonyLib;

namespace Polus.Patches.Temporary {
    [HarmonyPatch(typeof(HudManager), nameof(HudManager.SetHudActive))]
    public class SetHudActiveFixPatch {
        [HarmonyPrefix]
        public static bool Prefix(HudManager __instance, [HarmonyArgument(0)] ref bool isActive) {
            __instance.UseButton.gameObject.SetActive(UseButtonTargetPatch.useButtonEnabled ? isActive : false);
            __instance.UseButton.Refresh();
            __instance.ReportButton.gameObject.SetActive(ReportButtonDisablePatch.enabled ? isActive : false);
            PlayerControl localPlayer = PlayerControl.LocalPlayer;
            GameData.PlayerInfo playerInfo = (localPlayer != null) ? localPlayer.Data : null;
            __instance.KillButton.gameObject.SetActive(isActive && playerInfo.IsImpostor && !playerInfo.IsDead);
            __instance.TaskText.transform.parent.gameObject.SetActive(TaskPanelUpdatePatch.enabled ? isActive : false);
            __instance.roomTracker.gameObject.SetActive(isActive);
            return false;
        }
    }
}