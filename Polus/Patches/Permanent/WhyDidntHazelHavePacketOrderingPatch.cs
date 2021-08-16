using System.Collections.Generic;
using BepInEx.Logging;
using HarmonyLib;
using Hazel;
using Hazel.Udp;
using Polus.Extensions;
using Polus.Utils;

namespace Polus.Patches.Permanent {
    [HarmonyPatch(typeof(UdpConnection), nameof(UdpConnection.ReliableMessageReceive))]
    public class WhyDidntHazelHavePacketOrderingPatch {
        private static ushort _nextSequenceReceived;
        private static Dictionary<ushort, MessageReader> _packetQueue = new();

        public static void Reset() {
            _packetQueue = new Dictionary<ushort, MessageReader>();
            _nextSequenceReceived = 0;
        }

        [HarmonyPrefix]
        public static bool ReliableMessageReceive(UdpConnection __instance, [HarmonyArgument(0)] MessageReader reader, [HarmonyArgument(1)] int bytesReceived) {
            reader.Position++;
            
            ushort nonce = reader.ReadUInt16();
            
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
}