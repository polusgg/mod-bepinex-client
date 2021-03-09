using System;
using Hazel;

namespace PolusApi.Net {
    public interface IObjectManager {
        public static IObjectManager Instance;

        /// <summary>
        /// Sets the internal spawnable object at the index to the PolusNetObject specified
        /// </summary>
        /// <param name="index">The index/spawn type of the object.</param>
        /// <param name="netObject"></param>
        public void Register(int index, PolusNetObject netObject);
        public event EventHandler<RpcEventArgs> InnerRpcReceived;
        public void HandleInnerRpc(InnerNetObject netObject, RpcEventArgs args);
        public void HandleSpawn(int cnt, uint netId, MessageReader reader);
        public void RemoveNetObject(PolusNetObject obj);
        public bool HasObject(uint netId, out PolusNetObject obj);
        public bool IsDestroyed(uint netId);
        public T FindObjectByNetId<T>(uint netId) where T : PolusNetObject;
    }
}