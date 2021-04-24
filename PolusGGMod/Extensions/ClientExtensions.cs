﻿using Hazel;
using PolusGG.Behaviours.Inner;
using UnityEngine;

namespace PolusGG.Extensions {
    public static class ClientExtensions {
        public static void SendRpcImmediately(this AmongUsClient client, uint netId, byte callId,
            SendOption option = SendOption.Reliable) {
            MessageWriter messageWriter = MessageWriter.Get(option);
            messageWriter.StartMessage(5);
            messageWriter.Write(client.GameId);
            messageWriter.StartMessage(2);
            messageWriter.WritePacked(netId);
            messageWriter.Write(callId);
            messageWriter.EndMessage();
            messageWriter.EndMessage();
            client.connection.Send(messageWriter);
            messageWriter.Recycle();
        }

        public static Vector2 ReadVector2(this MessageReader reader) {
            float v = reader.ReadUInt16() / 65535f;
            float v2 = reader.ReadUInt16() / 65535f;
            return new Vector2(PolusNetworkTransform._xRange.Lerp(v), PolusNetworkTransform._yRange.Lerp(v2));
        }
    }
}