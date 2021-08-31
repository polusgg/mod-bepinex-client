using System.Linq;
using HarmonyLib;
using UnityEngine;

namespace Polus.Patches.Permanent.Qol {
    public class HatOffsetPatches {
        [HarmonyPatch(typeof(CustomPlayerMenu), nameof(CustomPlayerMenu.Start))]
        public static class CustomizationOffsetFixPatch {
            [HarmonyPrefix]
            public static void Start(CustomPlayerMenu __instance) => __instance.PreviewArea.GetComponentInChildren<HatParent>().transform.localPosition = new Vector3(-0.04f, 0.555f, 0f);
        }

        [HarmonyPatch(typeof(IntroCutscene._CoBegin_d__14), nameof(IntroCutscene._CoBegin_d__14.MoveNext))]
        public static class IntroOffsetFixPatch {
            [HarmonyPrefix]
            public static void CoBegin(IntroCutscene._CoBegin_d__14 __instance) {
                if (__instance.__1__state == 0) {
                    __instance.__4__this.PlayerPrefab.HatSlot.transform.localPosition = new Vector3(-0.04f, 0.575f, 0f);
                }
            }
        }

        [HarmonyPatch(typeof(EndGameManager._CoBegin_d__21), nameof(EndGameManager._CoBegin_d__21.MoveNext))]
        public static class OutroOffsetFixPatch {
            [HarmonyPrefix]
            public static void CoBegin(EndGameManager._CoBegin_d__21 __instance) {
                if (__instance.__1__state == 0) {
                    __instance.__4__this.PlayerPrefab.HatSlot.transform.localPosition = new Vector3(-0.04f, 0.575f, 0f);
                }
            }
        }

        [HarmonyPatch(typeof(ChatBubble), nameof(ChatBubble.SetCosmetics))]
        public static class ChatOffsetFixPatch {
            [HarmonyPrefix]
            public static void SetCosmetics(ChatBubble __instance) {
                __instance.Player.HatSlot.transform.localPosition = new Vector3(0.04f, 0.575f, 0f);
            }
        }

        [HarmonyPatch(typeof(PlayerVoteArea), nameof(PlayerVoteArea.SetCosmetics))]
        public static class MeetingOffsetFixPatch {
            [HarmonyPostfix]
            public static void SetCosmetics(PlayerVoteArea __instance) {
                __instance.PlayerIcon.HatSlot.transform.localPosition = new Vector3(-0.04f, 0.575f, 0f);
            } 
        }
    }
}