namespace PolusggSlim.Utils
{
    internal static class PggLog
    {
        internal static void Info(object data)
        {
            PluginSingleton<PolusggMod>.Instance.Log.LogInfo(data);
        }

        internal static void Message(object data)
        {
            PluginSingleton<PolusggMod>.Instance.Log.LogMessage(data);
        }

        internal static void Warning(object data)
        {
            PluginSingleton<PolusggMod>.Instance.Log.LogWarning(data);
        }

        internal static void Error(object data)
        {
            PluginSingleton<PolusggMod>.Instance.Log.LogError(data);
        }

        internal static void Fatal(object data)
        {
            PluginSingleton<PolusggMod>.Instance.Log.LogFatal(data);
        }
    }
}