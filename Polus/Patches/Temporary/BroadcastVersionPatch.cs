using HarmonyLib;

namespace Polus.Patches.Temporary {
    [HarmonyPatch(typeof(Constants), nameof(Constants.GetBroadcastVersion))]
    public class BroadcastVersionPatch {
        [HarmonyPostfix]
        public static void GetBroadcastVersion(out int __result) {
            __result = Constants.GetVersion(2021, 6, 30, 1);
        }
    }
}