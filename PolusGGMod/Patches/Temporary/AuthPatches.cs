using System;
using System.Security.Cryptography;
using System.Text;
using HarmonyLib;
using Hazel;
using Hazel.Udp;
using InnerNet;
using PolusGG.Api;
using UnhollowerBaseLib;

namespace PolusGG.Patches.Temporary {
    public class AuthPatches {
        private const byte AuthByte = 0x80;
        private const byte UuidSize = 16;
        private const byte HashSize = 20;

        private static HMAC _hmac = HMAC.Create();
        
        [HarmonyPatch(typeof(InnerNetClient), nameof(UnityUdpClientConnection.WriteBytesToConnection))]
        public class RuinPacketSending {
            public static void Prefix([HarmonyArgument(0)] ref Il2CppStructArray<byte> data, [HarmonyArgument(1)] ref int length) {
                _hmac.Key = Encoding.UTF8.GetBytes(PolusAuth.Token);
                byte[] hash = _hmac.ComputeHash(data, 0, length);
                byte[] output = new byte[1 + UuidSize + HashSize + length];
                output[0] = AuthByte;
                PolusAuth.Uuid.CopyTo(output, 1);
                hash.CopyTo(output, 1 + UuidSize);
                data.CopyTo(output, 1 + UuidSize + HashSize);
                data = output;
                length = output.Length;
            }
        }
    }
}