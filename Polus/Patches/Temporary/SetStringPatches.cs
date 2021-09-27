using System;
using HarmonyLib;
using InnerNet;
using Polus.Extensions;
using TMPro;
using UnhollowerBaseLib;
using UnityEngine;
using LogLevel = BepInEx.Logging.LogLevel;

namespace Polus.Patches.Temporary {
    [HarmonyPatch(typeof(HudManager), nameof(HudManager.Start))]
    public class SetStringResetPatch {
        [HarmonyPostfix]
        public static void Postfix() {
            PingTrackerTextPatch.PingText = null;
            RoomTrackerTextPatch.RoomText = null;
            HudUpdatePatch.TaskText = null;
            EmergencyTextPatch.EmergencyText = null;
        }
    }
    [HarmonyPatch(typeof(PingTracker), nameof(PingTracker.Update))]
    public static class PingTrackerTextPatch {
        public static string PingText;
#if DEBUG
        private static int _counter = 5;
        private static bool _displayed;
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
            {
                if (Input.GetKeyDown(KeyCode.F3))
                    _displayed = !_displayed;

                if (_counter-- <= 0 && _displayed) {
                    string textText = "\n";
                    foreach (GameData.PlayerInfo player in GameData.Instance.AllPlayers) {
                        Append(ref textText,
                            $"<color=#{Palette.PlayerColors[player.ColorId].ToHexColor()}>{player.PlayerName}", false);
                        Append(ref textText,
                            TranslationController.Instance.GetString(Palette.ColorNames[player.ColorId],
                                new Il2CppReferenceArray<Il2CppSystem.Object>(0)), false);
                        Append(ref textText, $"{player.PlayerId}", false);
                        Append(ref textText, $"{player.IsDead} {player.Disconnected}");
                    }
                    __instance.text.text += textText;
                }
            }
#endif
            // if (AmongUsClient.Instance.GameState == InnerNetClient.GameStates.Joined)
            // {
                __instance.text.text += "\nRegion: " + ServerManager.Instance.CurrentRegion.Name;
            // }

            if (PingText is not null) __instance.text.text = PingText.Replace("%s", AmongUsClient.Instance.Ping.ToString());
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
        [HarmonyPrefix]
        public static bool Update(RoomTracker __instance) {
            if (LobbyBehaviour.Instance) return true;
            if (RoomText is not null)
            {
                CustomSlide(__instance, RoomText);
                __instance.text.text = RoomText;
                return false;
            }

            __instance.text.text = __instance.LastRoom ? DestroyableSingleton<TranslationController>.Instance.GetString(__instance.LastRoom.RoomId) : "";

            // if (!Enum.IsDefined(typeof(SystemTypes), __instance.text.text))
            // {
            //     if (__instance.LastRoom)
            //     {
            //         int hitCount = __instance.LastRoom.roomArea.OverlapCollider(__instance.filter, __instance.buffer);
            //         if (RoomTracker.CheckHitsForPlayer(__instance.buffer, hitCount))
            //         {
            //             __instance.slideInRoutine = __instance.StartCoroutine(__instance.CoSlideIn(__instance.LastRoom.RoomId));
            //         }
            //     }
            // }

            return true;
        }

        private static void CustomSlide(RoomTracker __instance, string text)
        {
            Vector3 tempPos = __instance.text.transform.localPosition;
            Color tempColor = Color.white;
            __instance.text.text = text;
            tempPos.y = __instance.SourceY;
            tempColor.a = 1f;
            __instance.text.transform.localPosition = tempPos;
            __instance.text.color = tempColor;
        }
    }

    [HarmonyPatch(typeof(HudManager), nameof(HudManager.Update))]
    public class HudUpdatePatch
    {
        public static string TaskText = null;

        [HarmonyPostfix]
        public static void Postfix(HudManager __instance)
        {
            if (TaskText == null) return;
            if (__instance.taskDirtyTimer != 0f) return;
            __instance.TaskText.text = TaskText + "\n" + __instance.TaskText.text;
        }
    }

    [HarmonyPatch(typeof(EmergencyMinigame), nameof(EmergencyMinigame.Update))]
    public class EmergencyTextPatch
    {
        public static string EmergencyText = null;
        [HarmonyPostfix]
        public static void Postfix(EmergencyMinigame __instance)
        {
            if (EmergencyText is not null)
            {
                var number = "";
                number = PlayerControl.LocalPlayer.RemainingEmergencies.ToString();
                if (ShipStatus.Instance.Timer < 15f || ShipStatus.Instance.EmergencyCooldown > 0f)
                {
                    int num = Mathf.CeilToInt(15f - ShipStatus.Instance.Timer);
                    num = Mathf.Max(Mathf.CeilToInt(ShipStatus.Instance.EmergencyCooldown), num);
                    num.Log(1, "", LogLevel.Fatal);
                    number = DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.SecondsAbbv, new []
                    {
                        (Il2CppSystem.Object)num.ToString()
                    });
                }
                
                foreach (var pt in PlayerControl.LocalPlayer.myTasks)
                {
                    if (PlayerTask.TaskIsEmergency(pt))
                    {
                        number = "";
                        break;
                    }
                }

                number.Log(1, "", LogLevel.Fatal);
                __instance.StatusText.text = String.Format(EmergencyText, number, PlayerControl.LocalPlayer.Data.PlayerName);
                __instance.NumberText.text = "";
            }
        }
    }
}
