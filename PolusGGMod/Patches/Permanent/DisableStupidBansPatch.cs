﻿using HarmonyLib;
using InnerNet;
using PolusGG.Mods.Patching;
using UnityEngine;

namespace PolusGG.Patches.Permanent {
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
        [PermanentPatch]
        [HarmonyPrefix]
        private static bool StupidNonce(AuthManager._CoWaitForNonce_d__5 __instance, out bool __result) {
            __result = false;
            return false;
        }
    }

    [HarmonyPatch(typeof(AuthManager._CoConnect_d__4), nameof(AuthManager._CoConnect_d__4.MoveNext))]
    public class DisableStupidConnectsPatch {
        [PermanentPatch]
        [HarmonyPrefix]
        private static bool StupidConnects(out bool __result) {
            __result = false;
            return false;
        }
    }

    [HarmonyPatch(typeof(MainMenuManager), nameof(MainMenuManager.Start))]
    public static class MainMenuEnableOnline {
        [PermanentPatch]
        [HarmonyPostfix]
        public static void Postfix() {
            GameObject play = GameObject.Find("PlayOnlineButton");
            play.GetComponent<SpriteRenderer>().color = Color.white;
            play.GetComponent<PassiveButton>().enabled = true;
            ButtonRolloverHandler rollo = play.GetComponent<ButtonRolloverHandler>();
            rollo.OutColor = Color.white;
            rollo.OverColor = Color.green;
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

    [HarmonyPatch(typeof(SaveManager), nameof(SaveManager.BirthDateYear), MethodType.Getter)]
    public static class SaveManagerGetBirthDateYearPatch {
        [PermanentPatch]
        [HarmonyPrefix]
        public static bool Prefix(out int __result) {
            __result = 1990;
            return false;
        }
    }
}