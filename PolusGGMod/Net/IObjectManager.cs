using System;
using Hazel;
using InnerNet;
using UnityEngine;

namespace PolusGG.Net {
    public interface IObjectManager {
        /// <summary>
        /// Sets the internal spawnable object at the index to the PolusNetObject specified
        /// </summary>
        /// <param name="index">The index/spawn type of the object.</param>
        /// <param name="netObject"></param>
        public void Register(uint index, PnoBehaviour netObject);
        public PolusNetObject LocateNetObject(PnoBehaviour netBehaviour);
        public event Action<InnerNetObject, MessageReader, byte> InnerRpcReceived;
        public void HandleInnerRpc(InnerNetObject netObject, MessageReader reader, byte callId);
        public void HandleSpawn(uint spawnType, MessageReader reader);
        public void RemoveNetObject(PolusNetObject obj);
        public bool HasObject(uint netId, out PolusNetObject obj);
        public bool IsDestroyed(uint netId);
        public T FindObjectByNetId<T>(uint netId) where T : PolusNetObject;
        public void EndedGame();
        Transform GetNetObject(uint netId);
    }
}