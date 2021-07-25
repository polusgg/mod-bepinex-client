using HarmonyLib;
using Polus.Mods.Patching;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Polus.Patches.Permanent {
    // prevent hudmanager 
    [HarmonyPatch(typeof(HudManager), nameof(HudManager.Update))]
    public class DisableHudManagerOOGPatch {
        [PermanentPatch]
        [HarmonyPrefix]
        public static void Update(HudManager __instance) {
            if (SceneManager.GetActiveScene().name != "OnlineGame") Object.Destroy(__instance.gameObject);
        }
    }
}