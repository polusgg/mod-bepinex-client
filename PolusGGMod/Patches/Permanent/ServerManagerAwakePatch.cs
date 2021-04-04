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
            Il2CppStructArray<uint> stats = new(8);
            Array.Copy(StatsManager.Instance.WinReasons, stats, 7);
            StatsManager.Instance.WinReasons = stats;
            ServerManager.DefaultRegions = ServerManager.DefaultRegions.Append(PggConstants.Region).ToArray();
            __instance.CurrentRegion = PggConstants.Region;
            if (__instance.AvailableServers.All(s => s.Players == 0)) {
                __instance.CurrentRegion.Servers =
                    new Il2CppReferenceArray<ServerInfo>(__instance.AvailableServers.OrderBy(a => Guid.NewGuid())
                        .ToArray());
            }
            __instance.CurrentServer = (from s in __instance.AvailableServers
                orderby s.ConnectionFailures, s.Players
                select s).First();
            Debug.Log(string.Format("Selected server: {0}", __instance.CurrentServer));
            __instance.state = (ServerManager.Nested_0) 2;
            __instance.SaveServers();
            // IRegionInfo currentRegion = PggConstants.Region.Duplicate();
            // ServerManager.DefaultRegions = ServerManager.DefaultRegions.Append(currentRegion).ToArray();
            // __instance.CurrentRegion = currentRegion;
            // __instance.CurrentServer = currentRegion.Servers[0];
            // Debug.Log(string.Format("Selected server: {0}", __instance.CurrentServer));
            // __instance.Field_6 = ServerManager.UpdateState.Success;
            return false;
        }
    }
}