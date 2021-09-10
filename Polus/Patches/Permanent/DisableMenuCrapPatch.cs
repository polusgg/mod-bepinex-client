using HarmonyLib;
using Polus.Behaviours;
using Polus.Enums;
using Polus.Extensions;
using Polus.Mods.Patching;
using UnityEngine;

namespace Polus.Patches.Permanent {
    [HarmonyPatch(typeof(MainMenuManager), nameof(MainMenuManager.Start))]
    public class DisableMainMenuButtonsPatch {
        [PermanentPatch]
        [HarmonyPostfix]
        public static void Start() {
            GameObject.Find("PlayOnlineButton").EnsureComponent<PlayOnlineButtonManager>();
            GameObject.Find("PlayLocalButton").active = false;
            GameObject.Find("HowToPlayButton").transform.position = new Vector3(0, -1.725f, 0);
            GameObject.Find("FreePlayButton").active = false;
            AmongUsClient.Instance.MainMenuScene = GameScenes.MMOnline;
        }
    }

    // [HarmonyPatch(typeof(RegionTextMonitor), nameof(RegionTextMonitor.Start))]
    public class DisableRegionMenuPatch {
        [PermanentPatch]
        [HarmonyPrefix]
        public static void Start(RegionTextMonitor __instance) {
            __instance.transform.parent.gameObject.EnsureComponent<UnconditionalHide>();
        }
    }
}