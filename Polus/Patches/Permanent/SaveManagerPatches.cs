using HarmonyLib;
using InnerNet;

namespace Polus.Patches.Permanent {
    public class SaveManagerPatches {
        [HarmonyPatch(typeof(SaveManager), nameof(SaveManager.ChatModeType), MethodType.Getter)]
        public static class SaveManager_get_ChatModeType_Patch
        {
            [HarmonyPrefix]
            public static bool Prefix(out QuickChatModes __result)
            {
                __result = QuickChatModes.FreeChatOrQuickChat;
                return false;
            }
        }
        // [HarmonyPatch(typeof(SaveManager), nameof(SaveManager.GameSearchOptions), MethodType.Getter)]
        // public static class SearchSettingsPatch
        // {
        //     [HarmonyPostfix]
        //     public static bool GameSearchOptions(ref GameOptionsData result) {
        //         result.Keywords = GameKeywords.All;
        //         return false;
        //     }
        // }
    }
}