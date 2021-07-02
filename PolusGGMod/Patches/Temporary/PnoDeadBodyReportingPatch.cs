using HarmonyLib;
using PolusGG.Behaviours.Ino;

namespace PolusGG.Patches.Temporary {
    [HarmonyPatch(typeof(DeadBody), nameof(DeadBody.OnClick))]
    public class PnoDeadBodyReportingPatch {
        [HarmonyPrefix]
        public static bool OnClock(DeadBody __instance) {
            if (__instance.Reported)
                return false;
            if (__instance.ParentId != 255) return true;
            __instance.GetComponent<PogusDeadBody>().OnReported();
            return false;

        }
    }
}