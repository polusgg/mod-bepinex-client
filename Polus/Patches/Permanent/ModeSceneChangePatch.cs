using HarmonyLib;
using InnerNet;

namespace Polus.Patches.Permanent {
    [HarmonyPatch(typeof(InnerNetClient), nameof(InnerNetClient.SendSceneChange))]
    public class ModeSceneChangePatch {
        [HarmonyPrefix]
        public static void SendSceneChange(InnerNetClient __instance) {
            __instance.mode = MatchMakerModes.Client;
        }
    }
}