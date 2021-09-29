using HarmonyLib;
using Polus.Mods.Patching;
using UnityEngine;

namespace Polus.Patches.Permanent {
    public class FixScrollerPatches {
        [HarmonyPatch(typeof(Scroller), nameof(Scroller.FixedUpdate))]
        public static class DisableFixedUpdatePatch {
            [PermanentPatch]
            [HarmonyPrefix]
            public static bool FixedUpdate() => false;
        }

        [HarmonyPatch(typeof(Scroller), nameof(Scroller.Update))]
        public static class MoveFixedUpdateToUpdatePatch {
            [PermanentPatch]
            [HarmonyPrefix]
            public static void Update(Scroller __instance) {
                if (!__instance.Inner) {
                    return;
                }

                Vector2 mouseScrollDelta = Input.mouseScrollDelta;
                if (mouseScrollDelta.y == 0f) return;
                __instance.velocity = Vector2.zero;
                mouseScrollDelta.y = -mouseScrollDelta.y;
                __instance.ScrollRelative(mouseScrollDelta);
            }
        }
    }
}