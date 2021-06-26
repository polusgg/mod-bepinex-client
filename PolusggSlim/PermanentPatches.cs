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
                postfix: new HarmonyMethod(ServerManager_Awake.PostfixMethod)
            );

            // harmony.Patch(
            //     typeof(ServerManager).GetMethod(nameof(ServerManager.SetRegion)),
            //     new HarmonyMethod(ServerManager_SetRegion.PrefixMethod)
            // );
        }

        public static class ServerManager_Awake
        {
            public static MethodInfo PostfixMethod => typeof(ServerManager_Awake).GetMethod(nameof(Postfix));

            public static void Postfix(ServerManager __instance)
            {
                var serverConfig = PluginSingleton<PolusggMod>.Instance.Configuration.Server;
                // var regions = new[]
                // {
                var newRegion = new StaticRegionInfo(
                    serverConfig.RegionName,
                    StringNames.NoTranslation,
                    serverConfig.IpAddress,
                    new[]
                    {
                        new ServerInfo(serverConfig.ServerName, serverConfig.IpAddress, serverConfig.Port)
                    }
                ).Cast<IRegionInfo>();
                // };
                //
                // ServerManager.DefaultRegions = ServerManager.DefaultRegions.Concat(regions).ToArray();
                // __instance.AvailableRegions = regions;
                // __instance.CurrentRegion = regions[0];
                // __instance.CurrentServer = regions[0].Servers[0];
                // __instance.state = ServerManager.UpdateState.Success;

                __instance.AddOrUpdateRegion(newRegion);
                
                PggLog.Message("Loaded Polus.gg Servers");
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