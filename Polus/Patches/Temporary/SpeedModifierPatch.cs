using HarmonyLib;
using Polus.Behaviours;
using Polus.Extensions;

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
                __instance.body.velocity = __instance.gameObject.EnsureComponent<SpeedModifierManager>().SpeedModifer * DestroyableSingleton<HudManager>.Instance.joystick.Delta * (flag ? __instance.TrueGhostSpeed : __instance.TrueSpeed);
            }
            return false;
        }
    }
}