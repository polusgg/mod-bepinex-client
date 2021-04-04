using System;
using Hazel;
using InnerNet;

namespace PolusApi.Net {
    public interface IObjectManager {
        public static IObjectManager Instance;

        /// <summary>
        /// Sets the internal spawnable object at the index to the PolusNetObject specified
        /// </summary>
        /// <param name="index">The index/spawn type of the object.</param>
        /// <param name="netObject"></param>
        public void Register(uint index, PnoBehaviour netObject);

        public PolusNetObject LocateNetObject(PnoBehaviour netBehaviour);
        public event EventHandler<RpcEventArgs> InnerRpcReceived;
        public void HandleInnerRpc(InnerNetObject netObject, RpcEventArgs args);
        public void HandleSpawn(int cnt, uint spawnType, MessageReader reader);
        public void RemoveNetObject(PolusNetObject obj);
        public bool HasObject(uint netId, out PolusNetObject obj);
        public bool IsDestroyed(uint netId);
        public T FindObjectByNetId<T>(uint netId) where T : PolusNetObject;
        public void EndedGame();
    }
}