using System;
using System.Linq;
using HarmonyLib;
using PolusGG.Mods.Patching;
using UnhollowerBaseLib;

namespace PolusGG.Patches.Permanent {
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
            PggConstants.Region.cachedServers = new[] {
                new ServerInfo(PggConstants.Region.Name, PggConstants.Region.DefaultIp, PggConstants.Region.Port)
            };
            ServerManager.DefaultRegions =
                ServerManager.DefaultRegions.Prepend(PggConstants.Region.Duplicate()).ToArray();
            // __instance.CurrentRegion = PggConstants.Region.Duplicate();
            // __instance.CurrentServer = (from s in __instance.AvailableServers
            // orderby s.ConnectionFailures, s.Players
            // select s).First();
            // Debug.Log(string.Format("Selected server: {0}", __instance.CurrentServer));
            __instance.state = (ServerManager.UpdateState) 2;
            // __instance.SaveServers();
            // IRegionInfo currentRegion = PggConstants.Region.Duplicate();
            // ServerManager.DefaultRegions = ServerManager.DefaultRegions.Append(currentRegion).ToArray();
            // __instance.CurrentRegion = currentRegion;
            // __instance.CurrentServer = currentRegion.Servers[0];
            // Debug.Log(string.Format("Selected server: {0}", __instance.CurrentServer));
            // __instance.Field_6 = ServerManager.UpdateState.Success;
            return true;
        }
    }

    [HarmonyPatch(typeof(AnnouncementPopUp._Init_d__12), nameof(AnnouncementPopUp._Init_d__12.MoveNext))]
    public class AnnouncementInitPatch {
        [PermanentPatch]
        [HarmonyPrefix]
        public static void Postfix(AnnouncementPopUp._Init_d__12 __instance) {
            if (ServerManager.Instance.OnlineNetAddress.Equals(PggConstants.Region.DefaultIp)) {
                PogusPlugin.ModManager.PatchMods();
                PogusPlugin.ModManager.LoadMods();
            }
        }
    }
}