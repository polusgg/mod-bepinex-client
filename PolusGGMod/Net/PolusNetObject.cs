using Hazel;

namespace PolusGG.Net {
    public class PolusNetObject {
        public delegate void OnDataDel(MessageReader reader);

        public delegate void OnRpcDel(MessageReader reader, byte callId);

        private MessageReader data;
        private bool hasSpawn;
        public uint NetId;
        public OnDataDel OnData;
        public OnRpcDel OnRpc;
        public PnoBehaviour PnoBehaviour;

        public MessageReader GetSpawnData() {
            hasSpawn = false;
            return data;
        }

        public void HandleRpc(MessageReader reader, byte callId) {
            OnRpc?.Invoke(reader, callId);
        }

        public void Spawn(MessageReader reader) {
            // reader.Length.Log(3, "3 times i will cry in real life");
            hasSpawn = true;
            data = reader;
        }

        public void Data(MessageReader reader) {
            OnData?.Invoke(reader);
        }

        public bool HasSpawnData() {
            return hasSpawn;
        }
    }
}