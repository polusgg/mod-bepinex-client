using BepInEx.Logging;
using HarmonyLib;
using Polus.Behaviours;
using Polus.Extensions;
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
                if (CosmeticManager.Instance.GetIdByHat(behaviour) < CosmeticManager.CosmeticStartId) return true;
                if (__instance.Parent && __instance.Hat != null && IsValid(__instance, __instance.Hat)) {
                    SecondaryHatSpriteBehaviour sec = SecondaryHatSpriteBehaviour.GetHelper(__instance);
                    __instance.FrontLayer.sprite = behaviour.MainImage ?? behaviour.LeftMainImage;
                    __instance.BackLayer.sprite = behaviour.BackImage ?? behaviour.LeftBackImage;

                    sec.thirdLayer.flipX = __instance.Parent.flipX;
                    __instance.FrontLayer.flipX = !behaviour.MainImage ? !__instance.Parent.flipX : __instance.Parent.flipX;
                    __instance.BackLayer.flipX = !behaviour.BackImage ? !__instance.Parent.flipX : __instance.Parent.flipX;
                }

                return false;
            }
        }

        [HarmonyPatch(typeof(HatParent), nameof(HatParent.SetFloorAnim))]
        public static class HatParentSetClimb {
            [HarmonyPrefix]
            public static void SetFloorAnim(HatParent __instance) {
                SecondaryHatSpriteBehaviour sec = SecondaryHatSpriteBehaviour.GetHelper(__instance);
                sec.thirdLayer.enabled = false;
            }
        }

        [HarmonyPatch(typeof(HatParent), nameof(HatParent.SetHat), typeof(int))]
        public static class HatParentSetHat {
            [HarmonyPostfix]
            public static void SetHat(HatParent __instance, [HarmonyArgument(0)] int color) {
                SecondaryHatSpriteBehaviour sec = SecondaryHatSpriteBehaviour.GetHelper(__instance);
                sec.SetColor(color);
            }
        }

        [HarmonyPatch(typeof(HatParent), nameof(HatParent.SetIdleAnim))]
        public static class HatParentSetIdle {
            [HarmonyPrefix]
            public static bool SetIdleAnim(HatParent __instance) {
                if (!__instance.Hat)
                    return false;

                SecondaryHatSpriteBehaviour sec = SecondaryHatSpriteBehaviour.GetHelper(__instance);

                if (CosmeticManager.Instance.GetIdByHat(__instance.Hat) < CosmeticManager.CosmeticStartId) {
                    sec.thirdLayer.enabled = false;
                    return true;
                }

                if (__instance.Hat.AltShader) {
                    if (__instance.Hat.InFront) {
                        __instance.FrontLayer.sharedMaterial = DestroyableSingleton<HatManager>.Instance.DefaultHatShader;
                        __instance.BackLayer.sharedMaterial = DestroyableSingleton<HatManager>.Instance.DefaultHatShader;
                        sec.thirdLayer.sharedMaterial = __instance.Hat.AltShader;
                    } else {
                        sec.thirdLayer.sharedMaterial = DestroyableSingleton<HatManager>.Instance.DefaultHatShader;
                        __instance.FrontLayer.sharedMaterial = __instance.Hat.AltShader;
                        __instance.BackLayer.sharedMaterial = __instance.Hat.AltShader;
                    }
                } else {
                    __instance.FrontLayer.sharedMaterial = DestroyableSingleton<HatManager>.Instance.DefaultHatShader;
                    __instance.BackLayer.sharedMaterial = DestroyableSingleton<HatManager>.Instance.DefaultHatShader;
                    sec.thirdLayer.sharedMaterial = DestroyableSingleton<HatManager>.Instance.DefaultHatShader;
                }

                SpriteAnimNodeSync component = __instance.GetComponent<SpriteAnimNodeSync>();
                if (component != null) component.NodeId = __instance.Hat.NoBounce ? 1 : 0;
                __instance.BackLayer.enabled = true;
                __instance.FrontLayer.enabled = true;
                sec.thirdLayer.enabled = true;

                sec.thirdLayer.sprite = __instance.Hat.LeftClimbImage;
                __instance.FrontLayer.sprite = __instance.Hat.MainImage ?? __instance.Hat.LeftMainImage;
                __instance.BackLayer.sprite = __instance.Hat.BackImage ?? __instance.Hat.LeftBackImage;

                if (!__instance.Parent) return false; 
                sec.thirdLayer.flipX = __instance.Parent.flipX;
                __instance.FrontLayer.flipX = !__instance.Hat.MainImage ? !__instance.Parent.flipX : __instance.Parent.flipX;
                __instance.BackLayer.flipX = !__instance.Hat.BackImage ? !__instance.Parent.flipX : __instance.Parent.flipX;

                return false;
            }
        }
    }
}