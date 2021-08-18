using System.Linq;
using HarmonyLib;
using Hazel;
using Polus.Behaviours;
using UnityEngine;

namespace Polus.Patches.Temporary {
    public class MeetingHudPatches {
        [HarmonyPatch(typeof(ShipStatus), nameof(ShipStatus.IsGameOverDueToDeath))]
        public class EndHudPatch {
            [HarmonyPrefix]
            public static bool IsGameOverDueToDeath(out bool __result) {
                __result = false;
                return false;
            }
        }

        [HarmonyPatch(typeof(PlayerVoteArea), nameof(PlayerVoteArea.SetTargetPlayerId))]
        public static class AddPvamToButtonsPatch {
            [HarmonyPostfix]
            public static void SetTargetPlayerId(PlayerVoteArea __instance, [HarmonyArgument(0)] byte targetId) {
                __instance.gameObject.AddComponent<PvaManager>().Initialize(__instance, targetId);
            }
        }

        [HarmonyPatch(typeof(PlayerVoteArea), nameof(PlayerVoteArea.Deserialize))]
        public static class VoteAreaDeserializePatch {
            [HarmonyPrefix]
            public static bool Deserialize(PlayerVoteArea __instance, [HarmonyArgument(0)] MessageReader reader) {
                __instance.VotedFor = reader.ReadByte();
                __instance.GetComponent<PvaManager>().SetState(
                    reader.ReadBoolean(),
                    reader.ReadBoolean(),
                    reader.ReadBoolean()
                );
                return false;
            }
        }

        [HarmonyPatch(typeof(MeetingHud), nameof(MeetingHud.Awake))]
        public static class GrabUncrackedGlassPatch {
            public static Sprite UncrackedGlass;
            public static Color UncrackedColor;
            
            [HarmonyPostfix]
            public static void Awake(MeetingHud __instance) {
                UncrackedGlass = __instance.Glass.sprite;
                UncrackedColor = __instance.Glass.color;
            }
        }

        [HarmonyPatch(typeof(MeetingHud), nameof(MeetingHud.SetForegroundForDead))]
        public static class DisableWeirdSetForegroundThing {
            public static bool SetForegroundForDead() => false;
        }

        [HarmonyPatch(typeof(MeetingHud), nameof(MeetingHud.UpdateButtons))]
        public class ForegroundSetter {
            private static bool IsDead(MeetingHud mhud, PlayerControl control) => mhud.playerStates.First(state => state.TargetPlayerId == control.PlayerId).AmDead; 
            [HarmonyPostfix]
            public static void UpdateButtons(MeetingHud __instance) {
                __instance.SortButtons();
                bool shouldBeDead = IsDead(__instance, PlayerControl.LocalPlayer);
                if (shouldBeDead == __instance.amDead) return;
                __instance.amDead = shouldBeDead;
                __instance.SkipVoteButton.gameObject.SetActive(!shouldBeDead);
                __instance.Glass.sprite = shouldBeDead ? __instance.CrackedGlass : GrabUncrackedGlassPatch.UncrackedGlass;
                __instance.Glass.color = shouldBeDead ? Color.white : GrabUncrackedGlassPatch.UncrackedColor;
            }
        }
    }
}