using HarmonyLib;
using Polus.Behaviours.Inner;

namespace Polus.Patches.Temporary {
    [HarmonyPatch(typeof(DeadBody), nameof(DeadBody.OnClick))]
    public class PnoDeadBodyReportingPatch {
        [HarmonyPrefix]
        public static bool OnClock(DeadBody __instance) {
            if (__instance.Reported)
                return false;
            if (__instance.ParentId != 255) return true;
            __instance.GetComponent<PolusDeadBody>().OnReported();
            return false;

        }
    }
}