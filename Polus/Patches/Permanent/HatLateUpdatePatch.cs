using HarmonyLib;
using UnityEngine;

namespace Polus.Patches.Permanent {
    [HarmonyPatch(typeof(HatParent), nameof(HatParent.LateUpdate))]
    public class HatLateUpdatePatch {
        public static bool IsValid(HatParent parent, HatBehaviour bhv) {
            return parent.FrontLayer.sprite != bhv.ClimbImage && parent.FrontLayer.sprite != bhv.FloorImage;
        }

        [HarmonyPrefix]
        public static bool LateUpdate(HatParent __instance) {
            // return true;
            if (__instance.Parent != null && __instance.Hat != null && IsValid(__instance, __instance.Hat)) {
                HatBehaviour behaviour = __instance.Hat;
                Sprite front;
                Sprite back;

                if (__instance.Parent.flipX) {
                    if (behaviour.LeftMainImage != null) {
                        front = behaviour.LeftMainImage;
                        __instance.FrontLayer.flipX = false;
                    } else {
                        front = behaviour.MainImage;
                        __instance.FrontLayer.flipX = true;
                    }

                    if (behaviour.LeftBackImage != null) {
                        back = behaviour.LeftBackImage;
                        __instance.BackLayer.flipX = false;
                    } else {
                        back = behaviour.BackImage;
                        __instance.BackLayer.flipX = true;
                    }
                } else {
                    if (behaviour.MainImage != null) {
                        front = behaviour.MainImage;
                        __instance.FrontLayer.flipX = false;
                    } else {
                        front = behaviour.LeftMainImage;
                        __instance.FrontLayer.flipX = true;
                    }

                    if (behaviour.BackImage != null) {
                        back = behaviour.BackImage;
                        __instance.BackLayer.flipX = false;
                    } else {
                        back = behaviour.LeftBackImage;
                        __instance.BackLayer.flipX = true;
                    }
                }

                __instance.FrontLayer.sprite = front;
                __instance.BackLayer.sprite = back;
            }

            return false;
        }
    }
}