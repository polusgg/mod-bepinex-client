using HarmonyLib;

namespace Polus.Patches.Temporary {
    [HarmonyPatch(typeof(Console), nameof(Console.CanUse))]
    public class AllowTaskInteractionPatch {
        public static bool TaskInteractionAllowed = true;
        [HarmonyPrefix]
        public static bool Prefix(Console __instance, [HarmonyArgument(0)] GameData.PlayerInfo pc,
            [HarmonyArgument(1)] ref bool canUse, [HarmonyArgument(2)] ref bool couldUse, ref float __result) {
            if (TaskInteractionAllowed) return true;
            couldUse = (canUse = false);
            __result = float.MaxValue;
            return false;
        }
    }
}