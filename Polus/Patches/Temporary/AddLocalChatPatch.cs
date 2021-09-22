using HarmonyLib;

namespace Polus.Patches.Temporary {
    [HarmonyPatch(typeof(ChatController), nameof(ChatController.AddChat))]
    public class AddLocalChatPatch {
        [HarmonyPrefix]
        public static bool AddChat(ChatController __instance)
        {
            return false;
        }
    }
}