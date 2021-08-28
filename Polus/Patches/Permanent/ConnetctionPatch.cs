using HarmonyLib;
using InnerNet;

namespace Polus.Patches.Permanent {
    [HarmonyPatch(typeof(InnerNetClient), nameof(InnerNetClient.SendOrDisconnect))]
    public class ConnetctionPatch {
        [HarmonyPrefix]
        public static bool SendOrDisconnect(InnerNetClient __instance) => __instance.AmConnected;
    }
}