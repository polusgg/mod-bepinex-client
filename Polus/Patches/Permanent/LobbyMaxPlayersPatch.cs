using HarmonyLib;

namespace Polus.Patches.Permanent {
    [HarmonyPatch(typeof(GameStartManager), nameof(GameStartManager.Update))]
    public class LobbyMaxPlayersPatch {
        private static int maxPlayers = -1;
        [HarmonyPrefix]
        public static void Update(GameStartManager __instance) {
            if (PlayerControl.GameOptions.MaxPlayers == maxPlayers) return;
            maxPlayers = PlayerControl.GameOptions.MaxPlayers;
            __instance.LastPlayerCount = -1;
        } 
    }
}