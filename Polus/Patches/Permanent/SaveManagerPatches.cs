using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using InnerNet;
using Polus.Mods.Patching;

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
        [HarmonyPatch]
        public static class SearchSettingsPatch
        {
            [HarmonyTargetMethods]
            public static IEnumerable<MethodBase> Targets() {
                yield return AccessTools.PropertyGetter(typeof(SaveManager), nameof(SaveManager.LastHat));
                yield return AccessTools.PropertyGetter(typeof(SaveManager), nameof(SaveManager.LastPet));
                yield return AccessTools.PropertyGetter(typeof(SaveManager), nameof(SaveManager.LastSkin));
            }
            [PermanentPatch]
            [HarmonyPrefix]
            public static bool LastValues(ref uint __result) {
                __result = uint.MaxValue;
                return false;
            }
        }
    }
}