using System.Linq;
using System.Reflection;
using HarmonyLib;
using PolusggSlim.Utils;

namespace PolusggSlim
{
    public static class PermanentPatches
    {
        public static void PatchAll(Harmony harmony)
        {
            harmony.Patch(
                typeof(ServerManager).GetMethod(nameof(ServerManager.Awake)),
                new HarmonyMethod(ServerManager_Awake.PrefixMethod)
            );

            harmony.Patch(
                typeof(ServerManager).GetMethod(nameof(ServerManager.SetRegion)),
                new HarmonyMethod(ServerManager_SetRegion.PrefixMethod)
            );
        }

        public static class ServerManager_Awake
        {
            public static MethodInfo PrefixMethod => typeof(ServerManager_Awake).GetMethod(nameof(Prefix));
            public static void Prefix(ServerManager __instance)
            {
                var serverConfig = PluginSingleton<PolusggMod>.Instance.Configuration.Server;
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
        
        public static class ServerManager_SetRegion
        {
            public static MethodInfo PrefixMethod => typeof(ServerManager_SetRegion).GetMethod(nameof(Prefix));
            public static void Prefix([HarmonyArgument(0)] IRegionInfo regionInfo)
            {
                var plugin = PluginSingleton<PolusggMod>.Instance;
                var serverConfig = plugin.Configuration.Server;

                if (regionInfo.PingServer == serverConfig.IpAddress)
                    plugin.LocalLoad();
                else
                    plugin.LocalUnload();
            }
        }
    }
}