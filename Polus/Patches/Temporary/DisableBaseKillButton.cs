using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;

namespace Polus.Patches.Temporary {
    [HarmonyPatch]
    public class DisableBaseKillButton {
        [HarmonyTargetMethods]
        public static IEnumerable<MethodBase> TargetMethods() {
            yield return AccessTools.Method(typeof(KillButtonManager), nameof(KillButtonManager.PerformKill));
            yield return AccessTools.Method(typeof(KillButtonManager), nameof(KillButtonManager.SetTarget));
            yield return AccessTools.Method(typeof(KillButtonManager), nameof(KillButtonManager.SetCoolDown));
        }

        [HarmonyPrefix]
        public static bool Nop(KillButtonManager __instance) {
            __instance.gameObject.SetActive(false);
            return false;
        }
    }
}
