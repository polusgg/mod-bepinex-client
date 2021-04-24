﻿using HarmonyLib;
using TMPro;
using UnhollowerBaseLib;
using UnityEngine;
using Object = Il2CppSystem.Object;

namespace PolusGG.Patches.Temporary {
    [HarmonyPatch(typeof(PingTracker), nameof(PingTracker.Update))]
    public static class PingTrackerTextPatch {
        public static string PingText;
        #if DEBUG
        public static int _counter = 5;
        #endif

        [HarmonyPostfix]
        public static void Update(PingTracker __instance) {
            __instance.text.alignment = TextAlignmentOptions.TopLeft;
            if (__instance.useGUILayout) {
                AspectPosition pos = __instance.GetComponent<AspectPosition>();
                pos.DistanceFromEdge = new Vector3(3.5f, 0.3f, 0f);
                pos.AdjustPosition();
                __instance.useGUILayout = false;
            }
            #if DEBUG
            if (GameData.Instance)
                if (_counter-- <= 0) {
                    PingText = "";
                    foreach (GameData.PlayerInfo player in GameData.Instance.AllPlayers) {
                        Append(ref PingText,
                            $"<color=#{Palette.PlayerColors[player.ColorId].ToHexColor()}>{player.PlayerName}", false);
                        Append(ref PingText,
                            TranslationController.Instance.GetString(Palette.ColorNames[player.ColorId],
                                new Il2CppReferenceArray<Object>(0)), false);
                        Append(ref PingText, $"id:{player.PlayerId}", false);
                        Append(ref PingText, $"idx:{GameData.Instance.AllPlayers.IndexOf(player)}</color>");
                    }
                }
            #endif

            if (PingText is not null) __instance.text.text = PingText;
        }

        public static string ToHexColor(this Color32 color) {
            return $"{color.r:X2}{color.g:X2}{color.b:X2}{color.a:X2}";
        }

        public static void Append(ref string tmp, string str, bool newLine = true) {
            tmp += str;
            tmp += newLine ? '\n' : ' ';
        }
    }

    [HarmonyPatch(typeof(RoomTracker), nameof(RoomTracker.FixedUpdate))]
    public class RoomTrackerTextPatch {
        public static string RoomText = null;

        [HarmonyPostfix]
        public static void Update(RoomTracker __instance) {
            if (RoomText is not null) __instance.text.text = RoomText;
        }
    }

    // public class 
}