using HarmonyLib;
using Hazel.Udp;
using PolusggSlim.Utils;
using UnhollowerBaseLib;

namespace PolusggSlim.Auth.Patches
{
    public static class ConnectionPatch
    {
        [HarmonyPatch(typeof(UnityUdpClientConnection), nameof(UnityUdpClientConnection.WriteBytesToConnection))]
        public static class UnityUdpClientConnection_WriteBytesToConnection
        {
            public static void Prefix(
                [HarmonyArgument(0)] ref Il2CppStructArray<byte> bytes,
                [HarmonyArgument(1)] ref int length) => SignByteArray(ref bytes, ref length);
        }

        [HarmonyPatch(typeof(UnityUdpClientConnection), nameof(UnityUdpClientConnection.WriteBytesToConnectionSync))]
        public static class UnityUdpClientConnection_WriteBytesToConnectionSync
        {
            public static void Prefix(
                [HarmonyArgument(0)] ref Il2CppStructArray<byte> bytes,
                [HarmonyArgument(1)] ref int length) => SignByteArray(ref bytes, ref length);
        }

        private static void SignByteArray(ref Il2CppStructArray<byte> bytes, ref int length)
        {
            // if (ServerManager.Instance.CurrentRegion.PingServer ==
            //     PluginSingleton<PolusggMod>.Instance.Configuration.Server.IpAddress)
            // {
            if (AmongUsClient.Instance.GameMode == GameModes.OnlineGame && bytes[0] != SigningHelper.AUTH_BYTE && bytes[0] != 10)
            {
                var record = bytes[0] != 0;
                PluginSingleton<PolusggMod>.Instance.SigningHelper.SignByteArray(ref bytes, ref length);
                
                if (record)
                    PluginSingleton<PolusggMod>.Instance.PacketLogger.RecordPacket(bytes);
            }
            // }
        }
    }
}