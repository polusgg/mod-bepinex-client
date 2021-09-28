using System;
using System.IO;
using BepInEx.Logging;
using HarmonyLib;
using InnerNet;
using Polus.Extensions;
using Polus.Mods.Patching;
using UnityEngine;

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

        [HarmonyPatch(typeof(SaveManager), nameof(SaveManager.LoadSecureData))]
        public static class LoadSecureDataPatch {
            [PermanentPatch]
            [HarmonyPrefix]
            public static void LoadSecureData() {
                if (!SaveManager.purchaseFile.Loaded) {
                    try {
                        LoadData(SaveManager.purchaseFile, delegate(BinaryReader reader) {
                            while (reader.BaseStream.Position < reader.BaseStream.Length) {
                                string text = reader.ReadString();
                                if (text.Split(new char[] {
                                    '-'
                                }).Length == 3) {
                                    SaveManager.dobInfo = text;
                                } else {
                                    SaveManager.purchases.Add(text);
                                }
                            }
                        });
                    } catch (NullReferenceException) { } catch (Exception ex) {
                        $"Deleted corrupt secure file outer: {ex}".Log();
                        SaveManager.purchaseFile.Delete();
                    }
                }
            }
            public static void LoadData(SecureDataFile sdf, [HarmonyArgument(0)] Action<BinaryReader> performRead) {
                // return true;
                sdf.filePath = Path.Combine(Application.persistentDataPath, "secureNew");
                sdf.Loaded = true;
                $"Loading secure: {sdf.filePath}".Log();
                if (File.Exists(sdf.filePath)) {
                    byte[] array;
                    try {
                        array = File.ReadAllBytes(sdf.filePath);
                        for (int i = 0; i < array.Length; i++) array[i] ^= (byte) (i % 212);
                    } catch {
                        "Couldn't read secure file".Log(level: LogLevel.Error);
                        sdf.Delete();
                        return;
                    }

                    try {
                        MemoryStream memoryStream = new(array);
                        BinaryReader binaryReader = new(memoryStream);
                        binaryReader.ReadString(); //sysid
                        performRead(binaryReader);
                        "wawoowoowooeee".Log();
                        binaryReader.Dispose();
                        memoryStream.Dispose();
                    } catch {
                        "Deleted corrupt secure file inner".Log(level: LogLevel.Error);
                        sdf.Delete();
                    }
                }
            }
        }
    }
}