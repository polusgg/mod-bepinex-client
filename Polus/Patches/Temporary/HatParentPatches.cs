using HarmonyLib;
using Polus.Behaviours;
using Polus.Enums;
using Polus.Extensions;
using PowerTools;
using UnityEngine;

namespace Polus.Patches.Temporary {
    public static class HatParentPatches {
        public static void SetSprites(HatParent parent) {
            if (parent.Hat.MainImage || parent.Hat.LeftMainImage) {
                if (parent.Hat.MainImage && !parent.Hat.LeftMainImage) parent.FrontLayer.flipX = parent.Parent.flipX;
                else if (!parent.Hat.MainImage && parent.Hat.LeftMainImage) parent.FrontLayer.flipX = !parent.Parent.flipX;
                else parent.FrontLayer.sprite = parent.Parent.flipX ? parent.Hat.LeftMainImage : parent.Hat.MainImage;
            }

            if (parent.Hat.BackImage || parent.Hat.LeftBackImage) {
                if (parent.Hat.BackImage && !parent.Hat.LeftBackImage) parent.BackLayer.flipX = parent.Parent.flipX;
                else if (!parent.Hat.BackImage && parent.Hat.LeftBackImage) parent.BackLayer.flipX = !parent.Parent.flipX;
                else parent.BackLayer.sprite = parent.Parent.flipX ? parent.Hat.LeftBackImage : parent.Hat.BackImage;
            }
        }

        [HarmonyPatch(typeof(HatParent), nameof(HatParent.LateUpdate))]
        public static class HatLateUpdatePatch {
            [HarmonyPrefix]
            public static bool LateUpdate(HatParent __instance) {
                if (__instance.Hat == null) return false;
                HatBehaviour behaviour = __instance.Hat;
                SecondaryHatSpriteBehaviour sec = __instance.GetSecondary();
                if (CosmeticManager.Instance.GetIdByHat(behaviour) < CosmeticManager.CosmeticStartId) return true;
                if (__instance.Parent && __instance.Hat && sec.state == HatState.Idle) {
                    __instance.FrontLayer.sprite = behaviour.MainImage ?? behaviour.LeftMainImage;
                    __instance.BackLayer.sprite = behaviour.BackImage ?? behaviour.LeftBackImage;

                    sec.thirdLayer.flipX = __instance.Parent.flipX;
                    SetSprites(__instance);
                }

                return false;
            }
        }

        [HarmonyPatch(typeof(HatParent), nameof(HatParent.SetFloorAnim))]
        public static class HatParentSetFloor {
            [HarmonyPrefix]
            public static void SetFloorAnim(HatParent __instance) {
                SecondaryHatSpriteBehaviour sec = __instance.GetSecondary();
                sec.state = HatState.Floor;
                sec.thirdLayer.enabled = false;
            }
        }

        [HarmonyPatch(typeof(HatParent), nameof(HatParent.SetClimbAnim))]
        public static class HatParentSetClimb {
            [HarmonyPrefix]
            public static void SetClimbAnim(HatParent __instance) {
                SecondaryHatSpriteBehaviour sec = __instance.GetSecondary();
                sec.state = HatState.Climb;
                sec.thirdLayer.enabled = false;
            }
        }

        [HarmonyPatch(typeof(HatParent), nameof(HatParent.SetHat), typeof(int))]
        public static class HatParentSetHat {
            [HarmonyPrefix]
            public static void SetHat(HatParent __instance, [HarmonyArgument(0)] int color) {
                SecondaryHatSpriteBehaviour sec = __instance.GetSecondary();
                sec.SetColor(color);
            }
        }

        [HarmonyPatch(typeof(HatParent), nameof(HatParent.SetColor))]
        public static class HatParentSetColor {
            [HarmonyPrefix]
            public static void SetColor(HatParent __instance, [HarmonyArgument(0)] int color) {
                SecondaryHatSpriteBehaviour sec = __instance.GetSecondary();
                color.Log(comment: "New funky color mode:");
                sec.SetColor(color);
                PlayerTab tab = Object.FindObjectOfType<PlayerTab>();
                color.Log(comment: $"New cute color {tab is null} mode:");
                tab?.HatImage.GetSecondary().SetColor(color);
            }
        }

        [HarmonyPatch(typeof(HatParent), nameof(HatParent.SetIdleAnim))]
        public static class HatParentSetIdle {
            [HarmonyPrefix]
            public static bool SetIdleAnim(HatParent __instance) {
                if (!__instance.Hat)
                    return false;

                SecondaryHatSpriteBehaviour sec = __instance.GetSecondary();

                sec.state = HatState.Idle;

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
                SetSprites(__instance);

                return false;
            }
        }
    }
}