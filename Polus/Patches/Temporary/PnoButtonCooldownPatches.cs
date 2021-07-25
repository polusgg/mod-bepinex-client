using System.Linq;
using HarmonyLib;
using Polus.Behaviours.Inner;

namespace Polus.Patches.Temporary {
    [HarmonyPatch(typeof(HudManager), nameof(HudManager.SetHudActive))]
    public class PnoButtonVisibilityPatch {
        [HarmonyPostfix]
        public static void SetHudActive([HarmonyArgument(0)] bool active) {
            HudManager.Instance.KillButton.gameObject.SetActive(false);
            foreach (PolusClickBehaviour btn in PolusClickBehaviour.Buttons.Where(btn => btn.netTransform.AspectPosition.Alignment != 0)) {
                try {
                    btn.gameObject.SetActive(active);
                } catch { /* lol */ }
            }
        }
    }
}