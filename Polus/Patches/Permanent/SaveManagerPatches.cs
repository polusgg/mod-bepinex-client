using System;
using BepInEx.Logging;
using HarmonyLib;
using Il2CppSystem.IO;
using InnerNet;
using Polus.Extensions;
using Polus.Mods.Patching;
using UnityEngine;
using BinaryReader = Il2CppSystem.IO.BinaryReader;
using File = System.IO.File;

namespace Polus.Patches.Permanent {
    public class SaveManagerPatches {
        [HarmonyPatch(typeof(SaveManager), nameof(SaveManager.ChatModeType), MethodType.Getter)]
        public static class SaveManager_get_ChatModeType_Patch {
            [PermanentPatch]
            [HarmonyPrefix]
            public static bool Prefix(out QuickChatModes __result) {
                __result = QuickChatModes.FreeChatOrQuickChat;
                return false;
            }
        }

        [HarmonyPatch(typeof(SecureDataFile), nameof(SecureDataFile.LoadData))]
        public static class LoadSecureNewBypassPatch {
            [PermanentPatch]
            [HarmonyPrefix]
            public static bool LoadSecureData(SecureDataFile __instance, [HarmonyArgument(0)] Action<BinaryReader> performRead) {
                __instance.filePath = System.IO.Path.Combine(Application.persistentDataPath, "secureNew");
                __instance.Loaded = true;
                ("Loading secure: " + __instance.filePath).Log(30);
                if (File.Exists(__instance.filePath)) {
                    byte[] array;
                    try {
                        array = File.ReadAllBytes(__instance.filePath);
                        for (int i = 0; i < array.Length; i++) array[i] ^= (byte) (i % 212);
                    } catch {
                        "Couldn't read secure file".Log(level: LogLevel.Error);
                        __instance.Delete();
                        return false;
                    }

                    try {
                        MemoryStream memoryStream = new(array);
                        BinaryReader binaryReader = new(memoryStream);
                        binaryReader.ReadString(); //sysid
                        performRead(binaryReader);
                        binaryReader.Dispose();
                    } catch {
                        ("Deleted corrupt secure file inner").Log(level: LogLevel.Error);
                        __instance.Delete();
                    }
                }

                return false;
            }
        }
    }
}