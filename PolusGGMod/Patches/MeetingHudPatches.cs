using HarmonyLib;

namespace PolusMod.Patches {
    public class MeetingHudPatches {
        [HarmonyPatch(typeof(ShipStatus), nameof(ShipStatus.IsGameOverDueToDeath))]
        public class EndHudPatch {
            [HarmonyPrefix]
            public static bool IsGameOverDueToDeath(out bool __result) {
                __result = false;
                return false;
            }
        }
    }
}