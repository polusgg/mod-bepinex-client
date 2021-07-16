using HarmonyLib;
using PolusGG.Mods.Patching;
using UnityEngine;

namespace PolusGG.Patches.Permanent {
    [HarmonyPatch(typeof(MainMenuManager), nameof(MainMenuManager.Start))]
    public class DisableMainMenuButtonsPatch {
        [PermanentPatch]
        [HarmonyPostfix]
        public static void Start() {
            EOSManager.Instance.FindPlayOnlineButton().transform.position = new Vector3(0, -0.95f, 0);
            GameObject.Find("PlayLocalButton").active = false;
            GameObject.Find("HowToPlayButton").transform.position = new Vector3(0, -1.725f, 0);
            GameObject.Find("FreePlayButton").active = false;
        }
    }

    [HarmonyPatch(typeof(RegionTextMonitor), nameof(RegionTextMonitor.Start))]
    public class DisableRegionMenuPatch {
        [PermanentPatch]
        [HarmonyPrefix]
        public static void Start(RegionTextMonitor __instance) {
            __instance.transform.parent.gameObject.active = false;
        }
    }
}