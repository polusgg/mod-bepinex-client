using HarmonyLib;
using Polus.Behaviours;
using Polus.Extensions;
using UnityEngine;

namespace Polus.Patches.Temporary {
    [HarmonyPatch(typeof(PlayerPhysics), nameof(PlayerPhysics.FixedUpdate))]
    public class SpeedModifierPatch {
        [HarmonyPrefix]
        public static bool Prefix(PlayerPhysics __instance) {
            GameData.PlayerInfo data = __instance.myPlayer.Data;
            bool flag = data != null && data.IsDead;
            __instance.HandleAnimation(flag);
            if (__instance.AmOwner && __instance.myPlayer.CanMove && GameData.Instance)
            {
                __instance.body.velocity = __instance.gameObject.EnsureComponent<IndividualModifierManager>().SpeedModifer * DestroyableSingleton<HudManager>.Instance.joystick.Delta * (flag ? __instance.TrueGhostSpeed : __instance.TrueSpeed);
            }
            return false;
        }
    }

    [HarmonyPatch(typeof(CustomNetworkTransform), nameof(CustomNetworkTransform.FixedUpdate))]
    public class SpeedModifierPatchButCNT {
        [HarmonyPrefix]
        public static bool Prefix(CustomNetworkTransform __instance) {
            if (__instance.AmOwner)
            {
                if (__instance.HasMoved())
                {
                    __instance.SetDirtyBit(3U);
                    return false;
                }
            }
            else
            {
                if (__instance.interpolateMovement != 0f)
                {
                    Vector2 vector = (__instance.targetSyncPosition - __instance.body.position) * __instance.gameObject.EnsureComponent<IndividualModifierManager>().SpeedModifer;
                    if (vector.sqrMagnitude >= 0.0001f)
                    {
                        float num = __instance.interpolateMovement / __instance.sendInterval;
                        vector.x *= num;
                        vector.y *= num;
                        if (PlayerControl.LocalPlayer)
                        {
                            vector = Vector2.ClampMagnitude(vector, PlayerControl.LocalPlayer.MyPhysics.TrueSpeed * __instance.gameObject.EnsureComponent<IndividualModifierManager>().SpeedModifer);
                        }
                        __instance.body.velocity = vector;
                    }
                    else
                    {
                        __instance.body.velocity = Vector2.zero;
                    }
                }
                __instance.targetSyncPosition += __instance.targetSyncVelocity * Time.fixedDeltaTime * 0.1f * __instance.gameObject.EnsureComponent<IndividualModifierManager>().SpeedModifer;
            }
            return false;
        }
    }

    [HarmonyPatch(typeof(ShipStatus), nameof(ShipStatus.CalculateLightRadius))]
    public class CalculateLightPatch {
        [HarmonyPostfix]
        public static void Postfix(ShipStatus __instance, ref float __result) {
            __result *= PlayerControl.LocalPlayer.gameObject.EnsureComponent<IndividualModifierManager>().VisionModifier;
        }
    }
}