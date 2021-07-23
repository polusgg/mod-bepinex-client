using HarmonyLib;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Polus.Patches.Permanent {
    // prevent hudmanager 
    [HarmonyPatch(typeof(HudManager), nameof(HudManager.Update))]
    public class DisableHudManagerOOGPatch {
        [HarmonyPrefix]
        public static void Update(HudManager __instance) {
            if (SceneManager.GetActiveScene().name != "OnlineGame") Object.Destroy(__instance.gameObject);
        }
    }
}