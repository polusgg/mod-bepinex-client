using HarmonyLib;

namespace Polus.Patches.Temporary
{
    [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.Die))]
    public class ExitVentWhenKilledPatch
    {
        [HarmonyPostfix]
        public static void Postfix(PlayerControl __instance) {
            if (__instance.inVent) __instance.MyPhysics.ExitAllVents();
        }
    }
}