using HarmonyLib;

namespace PolusGG.Patches.Permanent {
    public class FixMeetings {
        [HarmonyPatch(typeof(MeetingHud), nameof(MeetingHud.UpdateButtons))]
        public class HandleDisconnectsNStuffRegardless {
            [HarmonyPrefix]
            public static bool HAsafoafknaf(MeetingHud __instance) {
                if (PlayerControl.LocalPlayer.Data.IsDead && !__instance.amDead) __instance.SetForegroundForDead();

                foreach (PlayerVoteArea playerVoteArea in __instance.playerStates) {
                    GameData.PlayerInfo playerById = GameData.Instance.GetPlayerById(playerVoteArea.TargetPlayerId);
                    if (playerById == null || playerById.Disconnected)
                        playerVoteArea.SetDisabled();
                    else if (playerById.IsDead != playerVoteArea.AmDead) playerVoteArea.SetDead(__instance.reporterId == playerById.PlayerId, playerVoteArea.AmDead = playerById.IsDead);
                }

                return false;
            }
        }
    }
}