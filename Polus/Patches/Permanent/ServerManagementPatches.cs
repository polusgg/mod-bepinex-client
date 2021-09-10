using System;
using System.Collections;
using System.Collections.Generic;
using HarmonyLib;
using Polus.Extensions;
using Polus.Mods.Patching;
using Polus.ServerList;
using UnhollowerBaseLib;

namespace Polus.Patches.Permanent {

    [HarmonyPatch(typeof(ServerManager), nameof(ServerManager.Awake))]
    public class ServerManagerAwakePatch {
        private static bool _hasStarted;

        [PermanentPatch]
        [HarmonyPrefix]
        public static bool Prefix(ServerManager __instance) {
            if (_hasStarted) return true;
            _hasStarted = true;

            PogusPlugin.ModManager.StartMods();
            Il2CppStructArray<uint> stats = new(8);
            Array.Copy(StatsManager.Instance.WinReasons, stats, 7);
            StatsManager.Instance.WinReasons = stats;
            // ServerManager.DefaultRegions =
            // ServerManager.DefaultRegions.Prepend(PggConstants.Region).ToArray();

            ServerManager.DefaultRegions = ServerManager.Instance.AvailableRegions = Array.Empty<IRegionInfo>();
            var servers = ServerListLoader.Load().GetAwaiter().GetResult();

            foreach (var server in servers)
            {
                ServerManager.Instance.AddOrUpdateRegion(new StaticRegionInfo(server.Name, StringNames.NoTranslation, server.Ip, new[]
                {
                    new ServerInfo(server.Name, server.Ip, 22023)
                }).Cast<IRegionInfo>());
            }

            if (PogusPlugin.ModManager.AllPatched) return false;
            PogusPlugin.ModManager.LoadMods();
            PogusPlugin.ModManager.PatchMods();
            return false;
        }

        private static IEnumerator SetPggRegion() {
            yield return ServerManager.Instance.WaitForServers();

            // if (ServerManager.Instance.AvailableRegions.All(x => x.PingServer != PggConstants.Region.PingServer)) {
            // }
            ServerManager.Instance.SetRegion(PggConstants.Region);
        }
    }

    [HarmonyPatch(typeof(AnnouncementPopUp._Init_d__12), nameof(AnnouncementPopUp._Init_d__12.MoveNext))]
    public class AnnouncementInitPatch {
        [PermanentPatch]
        [HarmonyPrefix]
        public static void Postfix(AnnouncementPopUp._Init_d__12 __instance) {
            // if (ServerManager.Instance.OnlineNetAddress.Equals(PggConstants.Region.PingServer) &&
            // !PogusPlugin.ModManager.AllPatched) {
            // }
        }
    }
}
