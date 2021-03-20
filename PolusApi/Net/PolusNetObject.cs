using System;
using System.IO;
using Hazel;
using PolusGGMod;

namespace PolusApi.Net {
    public class PolusNetObject {
        public uint NetId;
        public PnoBehaviour PnoBehaviour;
        public OnDataDel OnData;
        public OnRpcDel OnRpc;
        private byte[] data;
        private bool hasSpawn = false;
        public delegate void OnDataDel(MessageReader reader);
        public delegate void OnRpcDel(MessageReader reader, byte callId);

        public MessageReader GetSpawnData() {
            MessageReader reader = MessageReader.GetSized(data.Length);
            reader.Buffer = data;
            hasSpawn = false;
            return reader;
        }
        public void HandleRpc(MessageReader reader, byte callId) {
            OnRpc?.Invoke(reader, callId);
        }

        public void Spawn(MessageReader reader) {
            reader.Position.Log(3, "3 times i will cry in real life");
            hasSpawn = true;
            data = reader.ReadBytes(reader.BytesRemaining);
        }
        public void Data(MessageReader reader) {
            OnData?.Invoke(reader);
        }

        public bool HasSpawnData() => hasSpawn;
    }
}