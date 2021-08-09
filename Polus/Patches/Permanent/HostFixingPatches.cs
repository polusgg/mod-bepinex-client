using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using InnerNet;
using Polus.Mods.Patching;
using UnityEngine;

namespace Polus.Patches.Permanent {
    public class HostFixingPatches {
        private static int _bypassCall;
        private static int BypassCall => Mathf.Clamp(_bypassCall--, 0, int.MaxValue);
        public static void PrepareAmHost(int times = 1) => _bypassCall += times;
        public static void CleanupAmHost() => _bypassCall = 0;

        [HarmonyPatch(typeof(InnerNetClient), nameof(InnerNetClient.AmHost), MethodType.Getter)]
        [PermanentPatch]
        public static class AmHostDisable {
            internal static bool AmHostReal => AmongUsClient.Instance.HostId == AmongUsClient.Instance.ClientId;
            [HarmonyPrefix]
            public static bool AmHost(AmongUsClient __instance, ref bool __result) {
                __result = BypassCall > 0 && AmHostReal;
                return false;
            }
        }

        [HarmonyPatch]
        [PermanentPatch]
        public static class GameStartUpdate {
            [HarmonyTargetMethods]
            public static IEnumerable<MethodBase> Methods() {
                yield return AccessTools.Method(typeof(GameStartManager), nameof(GameStartManager.Update));
                yield return AccessTools.Method(typeof(GameStartManager), nameof(GameStartManager.MakePublic));
                yield return AccessTools.Method(typeof(GameStartManager), nameof(GameStartManager.ResetStartState));
                yield return AccessTools.Method(typeof(GameStartManager), nameof(GameStartManager.HandleDisconnect), new []{ typeof(PlayerControl), typeof(DisconnectReasons) });
            }
            [HarmonyPrefix]
            public static void Update() => PrepareAmHost(3);
            [HarmonyPostfix]
            public static void Clean() => CleanupAmHost();
        }

        [HarmonyPatch(typeof(GameStartManager), nameof(GameStartManager.Start))]
        [PermanentPatch]
        public static class GameStartStart {
            [HarmonyPostfix]
            public static void Start(GameStartManager __instance) {
                if (!AmHostDisable.AmHostReal) {
                    // "POG".Log(10);
                    return;
                }
                __instance.StartButton.gameObject.SetActive(true);
                __instance.MakePublicButton.GetComponent<ControllerHeldButtonBehaviour>().enabled = true;
                ActionMapGlyphDisplay componentInChildren = __instance.MakePublicButton.GetComponentInChildren<ActionMapGlyphDisplay>(true);
                if (componentInChildren) componentInChildren.gameObject.SetActive(false);
            }
        }
    }
}