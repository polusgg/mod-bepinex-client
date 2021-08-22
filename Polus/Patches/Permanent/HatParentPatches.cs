using HarmonyLib;
using Polus.Behaviours;
using PowerTools;
using UnityEngine;

namespace Polus.Patches.Permanent {
    public static class HatParentPatches {
        [HarmonyPatch(typeof(HatParent), nameof(HatParent.LateUpdate))]
        public static class HatLateUpdatePatch {
            public static bool IsValid(HatParent parent, HatBehaviour bhv) {
                return parent.FrontLayer.sprite != bhv.ClimbImage && parent.FrontLayer.sprite != bhv.FloorImage;
            }

            [HarmonyPrefix]
            public static bool LateUpdate(HatParent __instance) {
                if (__instance.Hat == null) return false;
                HatBehaviour behaviour = __instance.Hat;
                if (CosmeticManager.Instance.GetIdByHat(behaviour) < 10000000) return true;
                if (__instance.Parent && __instance.Hat != null && IsValid(__instance, __instance.Hat)) {
                    __instance.FrontLayer.sprite = behaviour.MainImage ?? behaviour.LeftMainImage;
                    __instance.BackLayer.sprite = behaviour.BackImage ?? behaviour.LeftBackImage;

                    __instance.FrontLayer.flipX = !behaviour.MainImage ? !__instance.Parent.flipX : __instance.Parent.flipX;
                    __instance.BackLayer.flipX = !behaviour.BackImage ? !__instance.Parent.flipX : __instance.Parent.flipX;
                }

                return false;
            }
        }

        [HarmonyPatch(typeof(HatParent), nameof(HatParent.SetIdleAnim))]
        public static class HatParentSetIdle {
            [HarmonyPrefix]
            public static bool SetIdleAnim(HatParent __instance) {
                if (!__instance.Hat)
                    return false;
                if (CosmeticManager.Instance.GetIdByHat(__instance.Hat) < 10000000) return true;

                if (__instance.Hat.AltShader) {
                    __instance.FrontLayer.sharedMaterial = __instance.Hat.AltShader;
                    __instance.BackLayer.sharedMaterial = __instance.Hat.AltShader;
                } else {
                    __instance.FrontLayer.sharedMaterial = DestroyableSingleton<HatManager>.Instance.DefaultHatShader;
                    __instance.BackLayer.sharedMaterial = DestroyableSingleton<HatManager>.Instance.DefaultHatShader;
                }

                SpriteAnimNodeSync component = __instance.GetComponent<SpriteAnimNodeSync>();
                if (component != null) component.NodeId = __instance.Hat.NoBounce ? 1 : 0;
                __instance.BackLayer.enabled = true;
                __instance.FrontLayer.enabled = true;

                __instance.FrontLayer.sprite = __instance.Hat.MainImage ?? __instance.Hat.LeftMainImage;
                __instance.BackLayer.sprite = __instance.Hat.BackImage ?? __instance.Hat.LeftBackImage;

                if (!__instance.Parent) return false; 
                __instance.FrontLayer.flipX = !__instance.Hat.MainImage ? !__instance.Parent.flipX : __instance.Parent.flipX;
                __instance.BackLayer.flipX = !__instance.Hat.BackImage ? !__instance.Parent.flipX : __instance.Parent.flipX;

                return false;
            }
        }
    }
}