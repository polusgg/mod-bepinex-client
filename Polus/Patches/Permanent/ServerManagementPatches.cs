using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using HarmonyLib;
using Il2CppDumper;
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
            "Hello1".Log();
            if (_hasStarted) return true;
            _hasStarted = true;

            PogusPlugin.ModManager.StartMods();
            Il2CppStructArray<uint> stats = new(8);
            Array.Copy(StatsManager.Instance.WinReasons, stats, 7);
            StatsManager.Instance.WinReasons = stats;

            ServerManager.DefaultRegions = ServerManager.Instance.AvailableRegions = Array.Empty<IRegionInfo>();
            var servers = ServerListLoader.Load().GetAwaiter().GetResult();

            var newServers = servers.Select(server =>
                new StaticRegionInfo(server.Name, StringNames.NoTranslation, server.Ip, new[] {
                    new ServerInfo(server.Name, server.Ip, 22023)
                }).Cast<IRegionInfo>());
            
            #if DEBUG
            if (!PogusPlugin.Revision.HasValue) {
                newServers = newServers
                    .AddItem(new StaticRegionInfo("Localhost", StringNames.NoTranslation, "127.0.0.1", new[] {
                        new ServerInfo("Localhost", "127.0.0.1", 22023)
                    }).Cast<IRegionInfo>());
                if (File.Exists("newregion.txt")) {
                    newServers = newServers.Append(PggConstants.Region).ToArray();
                }
            }
            #endif

            ServerManager.DefaultRegions = ServerManager.Instance.AvailableRegions = newServers.ToArray();

            if (ServerManager.Instance.AvailableRegions.Length > 0) {
                byte change = (byte) Math.Clamp(PggSaveManager.CurrentRegion, 0, ServerManager.Instance.AvailableRegions.Length - 1);
                if (PggSaveManager.CurrentRegion != change) PggSaveManager.CurrentRegion = change;
                ServerManager.Instance.CurrentRegion = ServerManager.Instance.AvailableRegions[PggSaveManager.CurrentRegion];
                ServerManager.Instance.CurrentServer = ServerManager.Instance.AvailableRegions[PggSaveManager.CurrentRegion].Servers[0];

                $"Current Region: {ServerManager.Instance.CurrentRegion.Name} {ServerManager.Instance.CurrentServer.Ip}".Log();
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