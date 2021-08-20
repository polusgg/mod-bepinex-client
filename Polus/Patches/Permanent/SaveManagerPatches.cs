using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using InnerNet;
using Polus.Mods.Patching;

namespace Polus.Patches.Permanent {
    public class SaveManagerPatches {
        [HarmonyPatch(typeof(SaveManager), nameof(SaveManager.ChatModeType), MethodType.Getter)]
        public static class SaveManager_get_ChatModeType_Patch
        {
            [HarmonyPrefix]
            public static bool Prefix(out QuickChatModes __result)
            {
                __result = QuickChatModes.FreeChatOrQuickChat;
                return false;
            }
        }
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

            [HarmonyPatch(typeof(SaveManager), nameof(SaveManager.LastHat), MethodType.Setter)]
            public class LastHatSetPatch {
                [HarmonyPrefix]
                public static void SetLastHat() => ActuallyHat = true;
            }
            [HarmonyPatch(typeof(SaveManager), nameof(SaveManager.lastPet), MethodType.Setter)]
            public class LastPetSetPatch {
                [HarmonyPrefix]
                public static void SetLastHat() => ActuallyHat = true;
            }
            [HarmonyPatch(typeof(SaveManager), nameof(SaveManager.LastSkin), MethodType.Setter)]
            public class LastSkinSetPatch {
                [HarmonyPrefix]
                public static void GetLastSkin() => ActuallySkin = true;
            }
            [HarmonyPatch(typeof(SaveManager), nameof(SaveManager.LastHat), MethodType.Getter)]
            public class LastHatGetPatch {
                [HarmonyPrefix]
                public static bool GetLastHat(out uint __result) => (__result = ActuallyHat ? uint.MaxValue : 9999999) == uint.MaxValue;
            }
            [HarmonyPatch(typeof(SaveManager), nameof(SaveManager.lastPet), MethodType.Getter)]
            public class LastPetGetPatch {
                [HarmonyPrefix]
                public static bool GetLastPet(out uint __result) => (__result = ActuallyPet ? uint.MaxValue : 9999999) == uint.MaxValue;
            }
            [HarmonyPatch(typeof(SaveManager), nameof(SaveManager.LastSkin), MethodType.Getter)]
            public class LastSkinGetPatch {
                [HarmonyPrefix]
                public static bool GetLastSkin(out uint __result) => (__result = ActuallySkin ? uint.MaxValue : 9999999) == uint.MaxValue;
            }
        }
    }
}