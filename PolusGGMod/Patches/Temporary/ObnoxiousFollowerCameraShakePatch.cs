using HarmonyLib;
using UnityEngine;

namespace PolusGG.Patches.Permanent {
    #if DEBUG
    [HarmonyPatch(typeof(FollowerCamera), nameof(FollowerCamera.Update))]
    public class ObnoxiousFollowerCameraShakePatch {
        [HarmonyPrefix]
        public static bool Update(FollowerCamera __instance) {
            if (__instance.Target && !__instance.Locked) {
                __instance.transform.position = Vector3.Lerp(__instance.transform.position,
                    (Vector2)__instance.Target.transform.position + __instance.Offset, 5f * Time.deltaTime);
            }

            return false;
        }
    }
    #endif
}