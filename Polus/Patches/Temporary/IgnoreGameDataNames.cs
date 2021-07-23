using HarmonyLib;

namespace Polus.Patches.Temporary {
    [HarmonyPatch(typeof(GameData.PlayerInfo), nameof(GameData.PlayerInfo.PlayerName), MethodType.Getter)]
    public class IgnoreGameDataNames {
        public static bool PlayerName(GameData.PlayerInfo __instance, ref string __result) {
            __result = __instance.Object ? __instance.Object.nameText.text : __instance._playerName;
            return false;
        }
    }
}