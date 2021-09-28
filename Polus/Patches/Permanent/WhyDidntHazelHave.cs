using System;
using System.Collections.Generic;
using System.Threading;
using BepInEx.Logging;
using HarmonyLib;
using Hazel;
using Hazel.Udp;
using Polus.Enums;
using Polus.Extensions;
using Polus.Mods.Patching;
using Polus.Utils;

namespace Polus.Patches.Permanent {
    public class WhyDidntHazelHave {
        [HarmonyPatch(typeof(UdpConnection), nameof(UdpConnection.ReliableMessageReceive))]
        public class PacketOrderingPatch {
            private static ushort _nextSequenceReceived;
            private static Dictionary<ushort, MessageReader> _packetQueue = new();

            public static void Reset() {
                _packetQueue = new Dictionary<ushort, MessageReader>();
                _nextSequenceReceived = 0;
            }

            [PermanentPatch]
            [HarmonyPrefix]
            public static bool ReliableMessageReceive(UdpConnection __instance, [HarmonyArgument(0)] MessageReader reader, [HarmonyArgument(1)] int bytesReceived) {
                reader.Position++;

                ushort nonce = reader.ReadUInt16();

                __instance.SendAck(nonce);

                lock (_packetQueue) {
                    if (!NetHelpers.SidGreaterThan(nonce, _nextSequenceReceived)) {
                        nonce.Log(comment: "Dropping a potential duplicate packet with id", level: LogLevel.Warning);
                        return false;
                    }

                    _packetQueue.Add(nonce, reader);
                    if (_nextSequenceReceived != nonce) {
                        $"Holding hazel reliable {nonce}, currently on {_nextSequenceReceived}".Log();
                        return false;
                    }

                    while (_packetQueue.ContainsKey(_nextSequenceReceived)) {
                        CatchHelper.TryCatch(() => __instance.InvokeDataReceived(SendOption.Reliable, _packetQueue[_nextSequenceReceived], 3, bytesReceived));
                        _packetQueue.Remove(_nextSequenceReceived);
                        _nextSequenceReceived++;
                    }
                }

                return false;
            }
        }

        public static class BetterPingsPatches {
            [HarmonyPatch(typeof(UdpConnection), nameof(UdpConnection.HandleReceive))]
            public static class HandleReceivePatch {
                [PermanentPatch]
                [HarmonyPrefix]
                public static bool HandleReceive(UdpConnection __instance, [HarmonyArgument(0)] MessageReader reader, [HarmonyArgument(1)] int bytesReceived) {
                    if (reader.Buffer[0] == 12) {
                        __instance.pingsSinceAck = 0;
                        __instance.Statistics.LogUnreliableReceive(bytesReceived - 1, bytesReceived);
                        reader.Recycle();
                        return false;
                    }

                    return true;
                }
            }

            [HarmonyPatch(typeof(UdpConnection), nameof(UdpConnection.SendPing))]
            public static class SendPingPatch {
                [PermanentPatch]
                [HarmonyPrefix]
                public static bool SendPing(UdpConnection __instance) {
                    __instance.WriteBytesToConnection(new byte[]{ 12 }, 1);
                    __instance.Statistics.LogUnreliableSend(0, 1);
                    return false;
                }
            }
        }

        // public static class PacketFragmentationPatches {
        //     [HarmonyPatch(typeof(UdpConnection), nameof(UdpConnection.HandleReceive))]
        //     public static class HandleReceivePatch {
        //         public static Dictionary<uint, byte[][]> Fragments = new(); 
        //         [PermanentPatch]
        //         [HarmonyPrefix]
        //         public static bool HandleReceive(UdpConnection __instance, [HarmonyArgument(0)] MessageReader reader, [HarmonyArgument(1)] int bytesReceived) {
        //             if (reader.Buffer[0] == (byte) (SendOptionPlus.Reliable | SendOptionPlus.Fragmented)) {
        //                 //todo merge packet buffers 
        //                 __instance.pingsSinceAck = 0;
        //                 __instance.Statistics.LogReliableReceive(bytesReceived - 1, bytesReceived);
        //                 reader.Recycle();
        //                 return false;
        //             }
        //
        //             return true;
        //         }
        //     }
        // }
    }
}