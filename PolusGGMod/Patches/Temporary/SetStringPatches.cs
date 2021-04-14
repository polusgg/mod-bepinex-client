using HarmonyLib;

namespace PolusGG.Patches {
    [HarmonyPatch(typeof(PingTracker), nameof(PingTracker.Update))]
    public class PingTrackerTextPatch {
        public static string PingText = null;
        [HarmonyPostfix]
        public static void Update(PingTracker __instance) {
            if (PingText is not null) {
                __instance.text.Text = PingText;
            }
        }
    }

    [HarmonyPatch(typeof(RoomTracker), nameof(RoomTracker.FixedUpdate))]
    public class RoomTrackerTextPatch {
        public static string RoomText = null;
        [HarmonyPostfix]
        public static void Update(RoomTracker __instance) {
            if (RoomText is not null) {
                __instance.text.Text = RoomText;
            }
        }
    }
}