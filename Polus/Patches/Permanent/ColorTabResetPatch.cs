using HarmonyLib;

namespace Polus.Patches.Permanent {
    [HarmonyPatch(typeof(PlayerTab), nameof(PlayerTab.UpdateAvailableColors))]
    public class ColorTabResetPatch {
        [HarmonyPostfix]
        public static void SelectColor(PlayerTab __instance) {
            __instance.HatImage.SetHat(SaveManager.LastHat, PlayerControl.LocalPlayer.Data.ColorId);
        }
    }
}