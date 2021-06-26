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
                    if (playerById == null)
                        playerVoteArea.SetDisabled();
                    else {
                        bool flag = playerById.Disconnected || playerById.IsDead;
                        if (flag != playerVoteArea.AmDead)
                            playerVoteArea.SetDead(__instance.reporterId == playerById.PlayerId, flag);
                    }
                }

                return false;
            }
        }
    }
}