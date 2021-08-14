using HarmonyLib;
using UnityEngine;

namespace Polus.Patches.Permanent {
    [HarmonyPatch(typeof(HatParent), nameof(HatParent.LateUpdate))]
    public class BetterHatLateUpdatePatch {
        public static bool IsValid(HatParent parent, HatBehaviour bhv) {
            return parent.FrontLayer.sprite != bhv.ClimbImage && parent.FrontLayer.sprite != bhv.FloorImage;
        } 
        public static bool LateUpdate(HatParent __instance) {
            if (__instance.Parent != null && __instance.Hat != null && IsValid(__instance, __instance.Hat)) {
                HatBehaviour behaviour = __instance.Hat;
                bool flipped = __instance.Parent.flipX;
                if ((behaviour.InFront || behaviour.BackImage) && behaviour.LeftMainImage) {
                    __instance.FrontLayer.sprite = flipped ? behaviour.LeftMainImage != null ? behaviour.LeftMainImage : behaviour.MainImage : behaviour.MainImage != null ? behaviour.MainImage : behaviour.LeftMainImage;
                }
                Sprite leftBack = behaviour.LeftBackImage != null ? behaviour.LeftBackImage : behaviour.BackImage != null ? behaviour.BackImage : behaviour.LeftMainImage != null ? behaviour.LeftMainImage : behaviour.MainImage;
                Sprite back = behaviour.BackImage != null ? behaviour.BackImage : behaviour.LeftBackImage != null ? behaviour.LeftBackImage : behaviour.MainImage != null ? behaviour.MainImage : behaviour.LeftMainImage;
                __instance.BackLayer.sprite = flipped ? leftBack : back;
            }

            return false;
        }
    }
}