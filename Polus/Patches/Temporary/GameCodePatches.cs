using HarmonyLib;
using InnerNet;

namespace Polus.Patches.Temporary {
    public class GameCodePatches {
        [HarmonyPatch(typeof(GameCode), nameof(GameCode.GameNameToInt))]
        public class GameNameToIntPatch {
            [HarmonyPrefix]
            public static bool GameNameToInt([HarmonyArgument(0)] string gameId, out int __result) {
                if (gameId.Length == 6) {
                    __result = GameCode.GameNameToIntV2(gameId);
                } else if (gameId.Length != 4) {
                    __result = -1;
                } else {
                    gameId = gameId.ToUpperInvariant();
                    __result = gameId[0] | (gameId[1] << 8) | (gameId[2] << 16) | (gameId[3] << 24);
                }

                return false;
            }
        }

        [HarmonyPatch(typeof(GameCode), nameof(GameCode.IntToGameName))]
        public class IntToGameNamePatch {
            [HarmonyPrefix]
            public static bool IntToGameName([HarmonyArgument(0)] int gameId, out string __result) {
                if (gameId < -1)
                    __result = GameCode.IntToGameNameV2(gameId);
                else if (gameId == 32)
                    __result = null;
                else
                    __result = new string(new[] {
                        (char) (gameId & 255),
                        (char) ((gameId >> 8) & 255),
                        (char) ((gameId >> 16) & 255),
                        (char) ((gameId >> 24) & 255)
                    });

                return false;
            }
        }
    }
}