using HarmonyLib;
using TMPro;
using UnityEngine;

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
                    string textText = "\n";
                    foreach (GameData.PlayerInfo player in GameData.Instance.AllPlayers) {
                        Append(ref textText,
                            $"<color=#{Palette.PlayerColors[player.ColorId].ToHexColor()}>{player.PlayerName}", false);
                        Append(ref textText,
                            TranslationController.Instance.GetString(Palette.ColorNames[player.ColorId],
                                new Il2CppReferenceArray<Object>(0)), false);
                        Append(ref textText, $"{player.PlayerId}", false);
                        Append(ref textText, $"{player.IsDead} {player.Disconnected}");
                    }
                    __instance.text.text += textText;
                }
            #endif

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
            if (RoomText is not null)
            {
                CustomSlide(__instance, RoomText);
                __instance.text.text = RoomText;
                return false;
            }

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
            __instance.TaskText.text += TaskText;
        }
    }

    // public class 
}
