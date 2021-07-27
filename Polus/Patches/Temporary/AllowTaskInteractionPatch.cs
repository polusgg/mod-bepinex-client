using HarmonyLib;

namespace Polus.Patches.Temporary {
    [HarmonyPatch(typeof(HudManager), nameof(HudManager.Start))]
    public class AllowTaskInteractionResetPatch {
        [HarmonyPostfix]
        public static void Postfix() {
            AllowTaskInteractionPatch.TaskInteractionAllowed = true;
        }
    }
    
    [HarmonyPatch(typeof(Console), nameof(Console.CanUse))]
    public class AllowTaskInteractionPatch {
        public static bool TaskInteractionAllowed = true;
        [HarmonyPrefix]
        public static bool Prefix(Console __instance, [HarmonyArgument(0)] GameData.PlayerInfo pc,
            [HarmonyArgument(1)] ref bool canUse, [HarmonyArgument(2)] ref bool couldUse, ref float __result) {
            // EXPLANATION: AllowImpostor is set to true when Console is not a task console (pretty obvious, duh)
            // That's why we're ignoring Consoles that have AllowImpostor on.
            if (TaskInteractionAllowed || __instance.AllowImpostor) return true;
            couldUse = canUse = false;
            __result = float.MaxValue;
            return false;
        }
    }
}