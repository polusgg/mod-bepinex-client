using HarmonyLib;
using PolusGG.Behaviours.Inner;

namespace PolusGG.Patches.Temporary {
    [HarmonyPatch(typeof(HudManager), nameof(HudManager.SetHudActive))]
    public class PnoButtonVisibilityPatch {
        public static void SetHudActive([HarmonyArgument(0)] bool active) {
            foreach (PolusClickBehaviour btn in PolusClickBehaviour.Buttons) {
                btn.gameObject.SetActive(btn.netTransform._aspectPosition.Alignment != 0 && active);
            }
        }
    }
}