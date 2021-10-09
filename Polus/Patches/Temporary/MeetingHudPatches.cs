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

        [HarmonyPatch(typeof(MeetingHud.__c), nameof(MeetingHud.__c._SortButtons_b__69_0))]
        public static class MeetingButtonSorting {
            [HarmonyPrefix]
            public static bool SetForegroundForDead(PlayerVoteArea p, out int __result) {
                PvaManager pvam = p.GetComponent<PvaManager>();
                __result = pvam.dead switch {
                    false when pvam.disabled => 512,
                    true when pvam.disabled => 256,
                    true => 128,
                    false => 0
                };
                return false;
            }
        }

        [HarmonyPatch(typeof(MeetingHud), nameof(MeetingHud.UpdateButtons))]
        public class ForegroundSetter {
            private static bool IsDead(MeetingHud mhud, PlayerControl control) => mhud.playerStates.First(state => state.TargetPlayerId == control.PlayerId).AmDead; 
            [HarmonyPostfix]
            public static void UpdateButtons(MeetingHud __instance) {
                bool shouldBeDead = IsDead(__instance, PlayerControl.LocalPlayer);
                if (shouldBeDead == __instance.amDead) return;
                __instance.amDead = shouldBeDead;
                __instance.SkipVoteButton.gameObject.SetActive(!shouldBeDead);
                __instance.Glass.sprite = shouldBeDead ? __instance.CrackedGlass : GrabUncrackedGlassPatch.UncrackedGlass;
                __instance.Glass.color = shouldBeDead ? Color.white : GrabUncrackedGlassPatch.UncrackedColor;
            }
        }

        [HarmonyPatch(typeof(MeetingHud), nameof(MeetingHud.Update))]
        public static class UpdateSortingPatch {
            [HarmonyPrefix]
            public static void Update(MeetingHud __instance) {
                __instance.SortButtons();
            }
        }
    }
}