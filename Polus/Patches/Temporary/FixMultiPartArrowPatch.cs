using HarmonyLib;
using UnityEngine;

namespace Polus.Patches.Temporary {
    [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.SetTasks))]
    public class FixMultiPartArrowPatch {
        [HarmonyPrefix]
        public static void Prefix(PlayerControl __instance) {
            if (__instance != PlayerControl.LocalPlayer) return;
            foreach (var pt in __instance.myTasks)
            {
                if (pt.TryCast<NormalPlayerTask>() == null) continue;
                if (pt.Cast<NormalPlayerTask>().Arrow != null) GameObject.Destroy(pt.Cast<NormalPlayerTask>().Arrow.gameObject);
            }
        }
    }
}