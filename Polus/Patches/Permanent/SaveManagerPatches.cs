using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using HarmonyLib;
using InnerNet;
using Polus.Extensions;
using Polus.Mods.Patching;
using UnityEngine;

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

        [HarmonyPatch(typeof(SecureDataFile), MethodType.Constructor)]
        public static class VanillaSecureFilePatch {
            [HarmonyPrefix]
            public static void Constructor([HarmonyArgument(0)] out string path) {
                path = Path.Combine(Application.persistentDataPath, "secureNew");
            }
        }
    }
}