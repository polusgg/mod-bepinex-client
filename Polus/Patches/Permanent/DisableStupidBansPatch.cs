using HarmonyLib;
using InnerNet;
using Polus.Mods.Patching;
using UnityEngine;

namespace Polus.Patches.Permanent {
    [HarmonyPatch(typeof(StatsManager), nameof(StatsManager.AmBanned), MethodType.Getter)]
    public class DisableStupidBansPatch {
        [PermanentPatch]
        [HarmonyPrefix]
        private static bool AmBannedGetter(out bool __result) {
            __result = false;
            return false;
        }
    }

    [HarmonyPatch(typeof(AuthManager._CoWaitForNonce_d__5), nameof(AuthManager._CoWaitForNonce_d__5.MoveNext))]
    public class DisableStupidNoncesPatch {
        // [PermanentPatch]
        [HarmonyPrefix]
        private static bool StupidNonce(AuthManager._CoWaitForNonce_d__5 __instance, out bool __result) {
            // __instance.__4__this.LastNonceReceived = new Nullable<uint>();
            __result = false;
            return false;
        }
    }

    [HarmonyPatch(typeof(AuthManager._CoConnect_d__4), nameof(AuthManager._CoConnect_d__4.MoveNext))]
    public class DisableStupidConnectsPatch {
        // [PermanentPatch]
        [HarmonyPrefix]
        private static bool StupidConnects(out bool __result) {
            __result = false;
            return false;
        }
    }

    [HarmonyPatch(typeof(AccountManager), nameof(AccountManager.CanPlayOnline))]
    public class AlrightBaeNowYouCanPlayOnline {
        // [PermanentPatch]
        [HarmonyPrefix]
        private static bool HeyBabyNowYouCanPlayOnline(out bool __result) {
            __result = true;
            return false;
        }
    }

    [HarmonyPatch(typeof(EOSManager), nameof(EOSManager.ProductUserId), MethodType.Getter)]
    public class ProductionUserIdentifier {
        // [PermanentPatch]
        [HarmonyPrefix]
        public static bool ProducerAuthorizationBuzzword(out string __result) {
            __result = "sick beats from a cool dude (this can be anything i just choose to put stupid shit here)";
            return false;
        }
    }

    [HarmonyPatch(typeof(AccountManager), nameof(AccountManager.Awake))]
    public static class DisableAccountManager {
        [PermanentPatch]
        [HarmonyPrefix]
        public static void Awake(AccountManager __instance) {
            __instance.gameObject.SetActive(false);
        }
    }

    [HarmonyPatch(typeof(SaveManager), nameof(SaveManager.AccountLoginStatus), MethodType.Getter)]
    public static class DisableRunLogin {
        [PermanentPatch]
        [HarmonyPrefix]
        public static bool AccountLoginStatus(out EOSManager.AccountLoginStatus __result) {
            __result = EOSManager.AccountLoginStatus.Offline;
            EOSManager.Instance.hasRunLoginFlow = true;
            return false;
        }
    }

    [HarmonyPatch(typeof(EOSManager), nameof(EOSManager.InitializePlatformInterface))]
    public static class EosManagerInitializePlatformInterfacePatch {
        [PermanentPatch]
        [HarmonyPrefix]
        public static bool Prefix(EOSManager __instance) {
            SaveManager.LoadPlayerPrefs();
            SaveManager.hasLoggedIn = true;
            SaveManager.isGuest = false;
            SaveManager.ChatModeType = QuickChatModes.FreeChatOrQuickChat;
            SaveManager.SavePlayerPrefs();
            __instance.ageOfConsent = 0;
            __instance.loginFlowFinished = true;
            __instance.platformInitialized = true;
            __instance.gameObject.SetActive(false);
            return false;
        }
    }

    [HarmonyPatch(typeof(EOSManager), nameof(EOSManager.IsAllowedOnline))]
    public static class EosManagerIsAllowedOnline {
        [PermanentPatch]
        [HarmonyPrefix]
        public static void IsAllowedOnline([HarmonyArgument(0)] bool isOnline) {
            isOnline = true;
        }
    }

    [HarmonyPatch(typeof(SaveManager), nameof(SaveManager.BirthDateYear), MethodType.Getter)]
    public static class SaveManagerGetBirthDateYearPatch {
        // [PermanentPatch]
        [HarmonyPrefix]
        public static bool Prefix(out int __result) {
            __result = 1990;
            return false;
        }
    }
}