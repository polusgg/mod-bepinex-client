using HarmonyLib;

namespace Polus.Patches.Temporary {
    [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.SetName))]
    public class FixDumbSetNamePatch {
        [HarmonyPrefix]
        public static bool Prefix(PlayerControl __instance, [HarmonyArgument(0)] string name, [HarmonyArgument(1)] bool dontCensor = false) {
            if (name != "") return true;
            if (GameData.Instance)
            {
                GameData.Instance.UpdateName(__instance.PlayerId, name, dontCensor);
            }
            __instance.gameObject.name = name;
            __instance.nameText.text = name;
            return false;
        }
    }
}