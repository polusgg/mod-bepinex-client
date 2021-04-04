using HarmonyLib;

namespace PolusGG.Patches {
    [HarmonyPatch(typeof(ChatController), nameof(ChatController.SendChat))]
    public class LolDisableChatRateLimitPatch {
        [HarmonyPrefix]
        public static bool Update(ChatController __instance) {
            // if (!PlayerControl.LocalPlayer.RpcSendChat(__instance.TextArea.text))
            // {
            //     return false;
            // }
            // __instance.TextArea.Clear();

            return true;
        }
    }
}