using System;
using System.IO;
using System.Linq;
using HarmonyLib;
using Hazel;
using UnhollowerBaseLib;
using UnityEngine;

namespace PolusGGMod.Patches {
    [HarmonyPatch(typeof(ServerManager), nameof(ServerManager.Awake))]
    public class ServerManagerAwakePatch {
        private static bool _hasStarted;
        [PermanentPatch]
        [HarmonyPrefix]
        public static bool Awake(ServerManager __instance) {
            if (_hasStarted) return false;
            _hasStarted = true;

            PogusPlugin.ModManager.StartMods();
            IRegionInfo currentRegion = PggConstants.Region.Duplicate();
            ServerManager.DefaultRegions = ServerManager.DefaultRegions.Append(currentRegion).ToArray();
            __instance.CurrentRegion = currentRegion;
            __instance.CurrentServer = currentRegion.Servers[0];
            Debug.Log(string.Format("Selected server: {0}", __instance.CurrentServer));
            __instance.Field_6 = ServerManager.UpdateState.Success;
            return false;
        }
    }
}