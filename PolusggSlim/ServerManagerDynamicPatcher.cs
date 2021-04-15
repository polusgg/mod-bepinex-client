using System.Linq;
using HarmonyLib;
using PolusggSlim.Utils;

namespace PolusggSlim
{
    public static class ServerManagerDynamicPatcher
    {
        [HarmonyPatch(typeof(ServerManager), nameof(ServerManager.Awake))]
        public static class ServerManager_Awake
        {
            public static void Prefix(ServerManager __instance)
            {
                var serverConfig = PluginSingleton<PolusggMod>.Instance.Config.Server;
                ServerManager.DefaultRegions = new[]
                {
                    new StaticRegionInfo(
                        serverConfig.RegionName, 
                        StringNames.NoTranslation, 
                        serverConfig.IpAddress, 
                        new[]
                        {
                            new ServerInfo(serverConfig.ServerName, serverConfig.IpAddress, serverConfig.Port)
                        }
                        ).Cast<IRegionInfo>()
                };
            }
        }
        
        [HarmonyPatch(typeof(ServerManager), nameof(ServerManager.SetRegion))]
        public static class ServerManager_SetRegion
        {
            public static void Prefix([HarmonyArgument(0)] IRegionInfo regionInfo)
            {
                var plugin = PluginSingleton<PolusggMod>.Instance;
                var serverConfig = plugin.Config.Server;

                if (regionInfo.PingServer == serverConfig.IpAddress)
                    plugin.LocalLoad();
                else
                    plugin.LocalUnload();
            }
        }
    }
}