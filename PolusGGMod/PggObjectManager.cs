using System;
using System.Collections.Generic;
using Hazel;
using InnerNet;
using PolusGG.Extensions;
using PolusGG.Net;
using UnityEngine;
using Object = UnityEngine.Object;

namespace PolusGG {
    public class PggObjectManager : IObjectManager {
        public void Register(uint index, PnoBehaviour netObject) {
            PogusPlugin.Logger.LogInfo($"Registered {netObject.GetType().Name} at index {index}");
            AmongUsClient.Instance.NonAddressableSpawnableObjects
            // _spawnObjects[index] = netObject;
        }

        public PolusNetObject LocateNetObject(PnoBehaviour netBehaviour) {
            return _allObjects.Find(x => x.PnoBehaviour.Pointer == netBehaviour.Pointer);
        }

        public event Action<InnerNetObject, MessageReader, byte> InnerRpcReceived;

        public void HandleInnerRpc(InnerNetObject netObject, MessageReader reader, byte callId) {
            InnerRpcReceived?.Invoke(netObject, reader, callId);
        }

        public void HandleSpawn(uint spawnType, MessageReader reader) {
            if (!_spawnObjects.ContainsKey(spawnType)) {
                Debug.LogError("Couldn't find polus spawnable prefab: " + spawnType);
                return;
            }

            //owner id
            reader.ReadPackedInt32();

            PnoBehaviour pnoBehaviour =
                Object.Instantiate(_spawnObjects[spawnType]);
            reader.ReadByte();
            int num5 = reader.ReadPackedInt32();
            PnoBehaviour[] componentsInChildren =
                pnoBehaviour.GetComponentsInChildren<PnoBehaviour>();
            if (num5 != componentsInChildren.Length) {
                Debug.LogError("Children didn't match for polus spawnable " + num5);
                Object.Destroy(pnoBehaviour.gameObject);
                return;
            }

            for (int i = 0; i < num5; i++) {
                PnoBehaviour childNetObject = componentsInChildren[i];
                PolusNetObject polusNetObject = new() {
                    NetId = reader.ReadPackedUInt32(),
                    PnoBehaviour = childNetObject
                };

                uint netId = polusNetObject.NetId;
                // polusNetObject4.OwnerId = num4;
                if (_destroyedObjects.Contains(netId)) {
                    polusNetObject.NetId = uint.MaxValue;
                    Object.Destroy(polusNetObject.PnoBehaviour.gameObject);
                    return;
                }

                // netId.Log(4, "l");
                if (!AddNetObject(polusNetObject)) {
                    polusNetObject.NetId = uint.MaxValue;
                    Object.Destroy(polusNetObject.PnoBehaviour.gameObject);
                    return;
                }
                
                childNetObject.name.Log(1, "has been spawned");

                MessageReader messageReader = reader.ReadMessage();
                if (messageReader.Length > 0)
                    // messageReader.Length.Log(6, "spawn length");
                    polusNetObject.Data(messageReader);
                // "did it really".Log();
            }

            pnoBehaviour.gameObject.active = true;
        }

        public void RemoveNetObject(PolusNetObject obj) {
            Object.Destroy(obj.PnoBehaviour.gameObject);

            _destroyedObjects.Add(obj.NetId);
        }

        public bool HasObject(uint netId, out PolusNetObject obj) {
            return _allObjectsFast.TryGetValue(netId, out obj);
        }

        public bool IsDestroyed(uint netId) {
            return _destroyedObjects.Contains(netId);
        }

        public PolusNetObject FindObjectByNetId(uint netId) => _allObjectsFast.TryGetValue(netId, out PolusNetObject netObject) ? netObject : null;

        public void EndedGame() {
            _allObjects = new List<PolusNetObject>();
            _allObjectsFast = new Dictionary<uint, PolusNetObject>();
            _destroyedObjects = new HashSet<uint>();
        }

        public Transform GetNetObject(uint netId) {
            if (_allObjectsFast.TryGetValue(netId, out PolusNetObject polusNetObject))
                return polusNetObject.PnoBehaviour.transform;

            if (AmongUsClient.Instance.allObjectsFast.ContainsKey(netId)) {
                return AmongUsClient.Instance.allObjectsFast[netId].transform;
            }

            return null;
        }

        public PolusNetObject FindNetObject(uint netId) {
            return _allObjectsFast[netId];
        }

        private bool AddNetObject(PolusNetObject polusNetObject) {
            // uint num = polusNetObject.NetId + 1u;
            // if (num > _netIdCnt)
            // {
            // 	_netIdCnt = num;
            // }
            // _allObjectsFast.Log(2);
            // polusNetObject.Log(2);
            // polusNetObject.NetId.Log(2);

            _allObjects.Add(polusNetObject);
            _allObjectsFast.Add(polusNetObject.NetId, polusNetObject);
            // if (!_allObjectsFast.ContainsKey(polusNetObject.NetId))
            // {
            return true;
            // }
            // return false;
        }

        public void UnregisterAll() {
            _spawnObjects = new Dictionary<uint, PnoBehaviour>();
        }
    }
}