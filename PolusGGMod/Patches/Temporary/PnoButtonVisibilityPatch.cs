using System.Linq;
using HarmonyLib;
using PolusGG.Behaviours.Ino;

namespace PolusGG.Patches.Temporary {
    [HarmonyPatch(typeof(HudManager), nameof(HudManager.SetHudActive))]
    public class PnoButtonVisibilityPatch {
        [HarmonyPostfix]
        public static void SetHudActive([HarmonyArgument(0)] bool active) {
            HudManager.Instance.KillButton.gameObject.SetActive(false);
            foreach (PogusClickBehaviour btn in PogusClickBehaviour.Buttons.Where(btn => btn.netTransform._aspectPosition.Alignment != 0)) {
                btn.gameObject.SetActive(active);
            }
        }
    }
}