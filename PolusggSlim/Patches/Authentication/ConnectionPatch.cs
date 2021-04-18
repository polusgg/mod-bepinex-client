using HarmonyLib;
using Hazel.Udp;
using PolusggSlim.Utils;
using UnhollowerBaseLib;

namespace PolusggSlim.Patches.Authentication
{
    public static class ConnectionPatch
    {
        [HarmonyPatch(typeof(UnityUdpClientConnection), nameof(UnityUdpClientConnection.SendHello))]
        public static class UnityUdpClientConnection_WriteBytesToConnection
        {
            public static void Prefix([HarmonyArgument(0)] ref Il2CppStructArray<byte> bytes, [HarmonyArgument(1)] ref int length)
            {
                PluginSingleton<PolusggMod>.Instance.AuthHelper.SignByteArray(ref bytes);
                length = bytes.Length;
            }
        }
    }
}