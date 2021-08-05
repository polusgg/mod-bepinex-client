using PolusggSlim.Utils;

namespace PolusggSlim
{
    public static class PermanentPatches
    {
        // [HarmonyPatch(typeof(ServerManager), nameof(ServerManager.Awake))]
        // [HarmonyPostfix]
        public static void ServerManager_Awake_Postfix(ServerManager __instance)
        {
            var serverConfig = PluginSingleton<PolusggMod>.Instance.Configuration.Server;
            var newRegion = new StaticRegionInfo(
                serverConfig.RegionName,
                StringNames.NoTranslation,
                serverConfig.IpAddress,
                new[]
                {
                    new ServerInfo(serverConfig.ServerName, serverConfig.IpAddress, serverConfig.Port)
                }
            ).Cast<IRegionInfo>();

            __instance.AddOrUpdateRegion(newRegion);
            
            PggLog.Message("Loaded Polus.gg Servers");
        }
    }
}