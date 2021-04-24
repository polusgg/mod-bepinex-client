﻿//#if DEBUG

using HarmonyLib;
using PolusGG.Mods.Patching;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace PolusGG.Patches.Permanent {
    [HarmonyPatch(typeof(SimpleSoundPlayer), nameof(SimpleSoundPlayer.OnEnable))]
    public class SanaeSkipsTheLogoPatch {
        [PermanentPatch]
        [HarmonyPrefix]
        public static bool Prefix(SimpleSoundPlayer __instance) {
            if (SceneManager.GetActiveScene().name == "SplashIntro") {
                Object.Destroy(__instance.gameObject);
                SceneManager.LoadScene("MainMenu");
                return false;
            }

            return true;
        }
    }
}
//#endif