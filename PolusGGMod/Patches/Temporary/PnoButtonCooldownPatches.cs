using System.Linq;
using HarmonyLib;
using PolusGG.Behaviours.Inner;
using PolusGG.Enums;
using PolusGG.Mods.Patching;

namespace PolusGG.Patches.Temporary {
    [HarmonyPatch(typeof(HudManager), nameof(HudManager.SetHudActive))]
    public class PnoButtonVisibilityPatch {
        [HarmonyPostfix]
        public static void SetHudActive([HarmonyArgument(0)] bool active) {
            HudManager.Instance.KillButton.gameObject.SetActive(false);
            foreach (PolusClickBehaviour btn in PolusClickBehaviour.Buttons.Where(btn => btn.netTransform._aspectPosition.Alignment != 0)) {
                try {
                    btn.gameObject.SetActive(active);
                } catch { /* lol */ }
            }
        }
    }
}