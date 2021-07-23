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
            return false;
        }
    }

    [HarmonyPatch(typeof(HudManager), nameof(HudManager.Start))]
    public class HideKillButtonOnStart {
        [HarmonyPostfix]
        public static void Postfix(HudManager __instance) {
            __instance.KillButton.gameObject.SetActive(false);
        }
    }
}