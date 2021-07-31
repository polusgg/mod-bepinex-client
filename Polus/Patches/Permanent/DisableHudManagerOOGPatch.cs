using HarmonyLib;
using Polus.Mods.Patching;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Polus.Patches.Permanent {
    // prevent hudmanager from being in a non online scene
    [HarmonyPatch(typeof(HudManager), nameof(HudManager.Update))]
    public class DisableHudManagerOOGPatch {
        [PermanentPatch]
        [HarmonyPrefix]
        public static void Update(HudManager __instance) {
            if (!AmongUsClient.Instance.InOnlineScene) Object.Destroy(__instance.gameObject);
        }
    }
}