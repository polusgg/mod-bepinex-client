using System.Diagnostics;
using HarmonyLib;
using Polus.Extensions;

namespace Polus.Patches.Temporary {
    [HarmonyPatch]
    public static class SearsPatches {
        public static bool ActuallyHat;
        public static bool ActuallyPet;
        public static bool ActuallySkin;

        public static void Reset() {
            ActuallyHat = false;
            ActuallyPet = false;
            ActuallySkin = false;
        }

        [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.HandleRpc))]
        public class SearsHandleRpc {
            [HarmonyPrefix]
            public static void HandleRpc(PlayerControl __instance, [HarmonyArgument(0)] RpcCalls rpc) {
                if (!__instance.AmOwner) return;
                switch (rpc) {
                    case RpcCalls.SetHat:
                        ActuallyHat = true;
                        break;
                    case RpcCalls.SetPet:
                        ActuallyPet = true;
                        break;
                    case RpcCalls.SetSkin:
                        ActuallySkin = true;
                        break;
                }
            }
        }

        [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.SetHat))]
        public class SetHatPatch {
            [HarmonyPostfix]
            public static void SetHat(PlayerControl __instance, [HarmonyArgument(0)] uint id) {
                if (PlayerControl.LocalPlayer is not null && PlayerControl.LocalPlayer.Pointer == __instance.Pointer && id != 9999999) SaveManager.LastHat = id;
            }
        }

        [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.SetPet))]
        public class SetPetPatch {
            [HarmonyPostfix]
            public static void SetPet(PlayerControl __instance, [HarmonyArgument(0)] uint id) {
                if (PlayerControl.LocalPlayer is not null && PlayerControl.LocalPlayer.Pointer == __instance.Pointer && id != 9999999) SaveManager.LastPet = id;
            }
        }

        [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.SetSkin))]
        public class SetSkinPatch {
            [HarmonyPostfix]
            public static void SetSkin(PlayerControl __instance, [HarmonyArgument(0)] uint id) {
                if (PlayerControl.LocalPlayer is not null && PlayerControl.LocalPlayer.Pointer == __instance.Pointer && id != 9999999) SaveManager.LastSkin = id;
            }
        }

        [HarmonyPatch(typeof(SaveManager), nameof(SaveManager.LastHat), MethodType.Setter)]
        public class LastHatSetPatch {
            [HarmonyPrefix]
            public static void SetLastHat() => ActuallyHat = true;
        }

        [HarmonyPatch(typeof(SaveManager), nameof(SaveManager.LastPet), MethodType.Setter)]
        public class LastPetSetPatch {
            [HarmonyPrefix]
            public static void SetLastPet() => ActuallyPet = true;
        }

        [HarmonyPatch(typeof(SaveManager), nameof(SaveManager.LastSkin), MethodType.Setter)]
        public class LastSkinSetPatch {
            [HarmonyPrefix]
            public static void GetLastSkin() => ActuallySkin = true;
        }

        [HarmonyPatch(typeof(SaveManager), nameof(SaveManager.LastHat), MethodType.Getter)]
        public class LastHatGetPatch {
            [HarmonyPrefix]
            public static bool GetLastHat(out uint __result) => (__result = (uint) (ActuallyHat ? 10000 : 9999999)) == 10000;
        }

        [HarmonyPatch(typeof(SaveManager), nameof(SaveManager.LastPet), MethodType.Getter)]
        public class LastPetGetPatch {
            [HarmonyPrefix]
            public static bool GetLastPet(out uint __result) => (__result = ActuallyPet ? uint.MaxValue - 1 : 9999999) == uint.MaxValue - 1;
        }

        [HarmonyPatch(typeof(SaveManager), nameof(SaveManager.LastSkin), MethodType.Getter)]
        public class LastSkinGetPatch {
            [HarmonyPrefix]
            public static bool GetLastSkin(out uint __result) => (__result = ActuallySkin ? uint.MaxValue - 1 : 9999999) == uint.MaxValue - 1;
        }
    }
}