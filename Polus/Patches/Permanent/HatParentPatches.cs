using HarmonyLib;
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
                // return true;
                if (__instance.Hat != null) {
                    HatBehaviour behaviour = __instance.Hat;
                    Sprite front;
                    Sprite back;

                    if (__instance.Parent != null && __instance.Parent.flipX) {
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

        [HarmonyPatch(typeof(HatParent), nameof(HatParent.SetIdleAnim))]
        public static class HatParentSetIdle {
            [HarmonyPrefix]
            public static bool SetIdleAnim(HatParent __instance) {
                if (!__instance.Hat)
                    return false;

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
                return false;
            }
        }
    }
}