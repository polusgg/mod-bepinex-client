using Hazel;

namespace PolusMod {
    public static class ClientExtensions {
        public static void SendRpcImmediately(this AmongUsClient client, uint netId, byte callId, SendOption option = SendOption.Reliable) {
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
    }
}