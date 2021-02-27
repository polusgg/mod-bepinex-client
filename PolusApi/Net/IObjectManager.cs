using Hazel;

namespace PolusApi.Net {
    public interface IObjectManager {
        public static IObjectManager Instance;

        public void Register(int index, PolusNetObject netObject);
        public void HandleRpcInner(InnerNetObject netObject, byte call, MessageReader reader);
        public void HandleSpawn(int cnt, uint netId, MessageReader reader);
        public void RemoveNetObject(PolusNetObject obj);
        public bool HasObject(uint netId, out PolusNetObject obj);
        public bool IsDestroyed(uint netId);
        public T FindObjectByNetId<T>(uint netId) where T : PolusNetObject;
    }
}