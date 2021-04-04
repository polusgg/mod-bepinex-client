using System;
using System.Reflection;
using HarmonyLib;

namespace PolusGGMod.Patches {
    [HarmonyPatch(typeof(StatsManager), nameof(StatsManager.AmBanned), MethodType.Getter)]
    public class DisableStupidBansPatch {
        [PermanentPatch]
        [HarmonyPrefix]
        private static bool AmBannedGetter(out bool __result) {
            __result = false;
            return false;
        }
    }
}