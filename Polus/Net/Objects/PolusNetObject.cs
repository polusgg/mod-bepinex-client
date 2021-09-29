using System.Collections.Concurrent;
using Hazel;

namespace Polus.Net.Objects {
    public class PolusNetObject {
        public delegate void OnDataDel(MessageReader reader);
        public delegate void OnRpcDel(MessageReader reader, byte callId);

        public record Rpc(MessageReader Reader, byte CallId) {
            public MessageReader Reader { get; } = Reader;
            public byte CallId { get; } = CallId;
        }

        private readonly ConcurrentQueue<MessageReader> _data = new();
        private readonly ConcurrentQueue<Rpc> _rpc = new();
        public uint NetId;
        public PnoBehaviour PnoBehaviour;

        public MessageReader GetData() {
            _data.TryDequeue(out MessageReader reader);
            return reader;
        }

        public Rpc GetRpcData() {
            _rpc.TryDequeue(out Rpc rpcData);
            return rpcData;
        }

        public void HandleRpc(MessageReader reader, byte callId) {
            _rpc.Enqueue(new Rpc(reader, callId));
        }

        public void Data(MessageReader reader) {
            _data.Enqueue(reader);
        }

        public bool HasData() {
            return !_data.IsEmpty;
        }

        public bool HasRpc() {
            return !_rpc.IsEmpty;
        }
    }
}