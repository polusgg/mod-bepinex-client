using HarmonyLib;
using InnerNet;
using UnityEngine;

namespace PolusggSlim.Patches.Misc
{
    public static class RemoveAuthentication
    {
        [HarmonyPatch(typeof(StatsManager), nameof(StatsManager.AmBanned), MethodType.Getter)]
        public static class StatsManager_getAmBanned
        {
            public static bool Prefix(ref bool __result)
            {
                return __result = false;
            }
        }

        [HarmonyPatch(typeof(AuthManager._CoConnect_d__4))]
        public static class AuthManager_CoConnect
        {
            public static bool Prefix(ref bool __result)
            {
                return __result = false;
            }
        }

        [HarmonyPatch(typeof(AuthManager._CoWaitForNonce_d__5))]
        public static class AuthManager_CoWaitForNonce
        {
            public static bool Prefix(ref bool __result)
            {
                return __result = false;
            }
        }
        
        [HarmonyPatch(typeof(MainMenuManager), nameof(MainMenuManager.Start))]
        public static class MainMenuManager_Start {
            public static void Postfix() {
                var play = GameObject.Find("PlayOnlineButton");
                play.GetComponent<SpriteRenderer>().color = Color.white;
                play.GetComponent<PassiveButton>().enabled = true;
                var rolloverHandler = play.GetComponent<ButtonRolloverHandler>();
                rolloverHandler.OutColor = Color.white;
                rolloverHandler.OverColor = Color.green;
            }
        }
        
        [HarmonyPatch(typeof(EOSManager), nameof(EOSManager.InitializePlatformInterface))]
        public static class EOSManager_InitializePlatformInterfacePatch
        {
            public static bool Prefix(EOSManager __instance)
            {
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
        public static class SaveManagerGetBirthDateYearPatch
        {
            public static bool Prefix(out int __result)
            {
                __result = 1990;
                return false;
            }
        }
    }
}