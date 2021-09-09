using System;
using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using InnerNet;
using Polus.Extensions;
using Polus.Mods.Patching;

namespace Polus.Patches.Permanent {
    public class SaveManagerPatches {
        [HarmonyPatch(typeof(SaveManager), nameof(SaveManager.ChatModeType), MethodType.Getter)]
        public static class SaveManager_get_ChatModeType_Patch {
            [HarmonyPrefix]
            public static bool Prefix(out QuickChatModes __result) {
                __result = QuickChatModes.FreeChatOrQuickChat;
                return false;
            }
        }
    }
}