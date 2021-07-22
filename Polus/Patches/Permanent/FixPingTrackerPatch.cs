using HarmonyLib;
using Polus.Mods.Patching;

namespace Polus.Patches.Permanent {
    [HarmonyPatch(typeof(PingTracker), nameof(PingTracker.Update))]
    public static class FixPingTrackerPatch {
        [PermanentPatch]
        [HarmonyPostfix]
        public static void Update(PingTracker __instance) {
            // __instance.text.alignment = TextAlignmentOptions.Bottom;
        }
    }
}