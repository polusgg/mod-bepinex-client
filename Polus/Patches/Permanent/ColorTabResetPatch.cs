using HarmonyLib;
using Polus.Extensions;

namespace Polus.Patches.Permanent {
    [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.SetColor))]
    public class ColorTabResetPatch {
        [HarmonyPrefix]
        public static void SetColor(PlayerControl __instance, [HarmonyArgument(0)] int bodyColor) {
            // __instance.HatRenderer.GetSecondary().SetColor(bodyColor);
        }
    }
}