using System;
using System.Collections.Generic;
using System.Linq;
using Hazel;
using PolusApi.Net;
using UnhollowerRuntimeLib;
using UnityEngine;
using Object = UnityEngine.Object;

namespace PolusGGMod {
	public class PggObjectManager : IObjectManager {
		private Dictionary<uint, PolusNetObject> _allObjectsFast = new();
		private List<PolusNetObject> _allObjects = new();
		private HashSet<uint> _destroyedObjects = new();
		private Dictionary<uint, PolusNetObject> _spawnObjects = new();
		private uint _netIdCnt = 0x80000000;

		// [DllImport("user32.dll")]
		// private static extern void MessageBox(IntPtr hwnd, string text, string caption, uint type = 4);

		public void Register(uint index, PolusNetObject netObject) {
			PogusPlugin.Logger.LogInfo($"Registered {netObject.name} at index {index}");
			_spawnObjects[index] = netObject;
		}

		public event EventHandler<RpcEventArgs> InnerRpcReceived;

		public void HandleInnerRpc(InnerNetObject netObject, RpcEventArgs args) {
			InnerRpcReceived.Invoke(netObject, args);
		}

		public void HandleSpawn(int cnt, uint spawnType, MessageReader reader) {
			if (!_spawnObjects.ContainsKey(spawnType)) {
				Debug.LogError("Couldn't find polus spawnable prefab: " + spawnType);
				return;
			}

			/*int num4 = */reader.ReadPackedInt32();
			// ClientData clientData = PolusNetClient.FindClientById(num4);
			// if (num4 > 0 && clientData == null) {
			// 	AmongUsClient.Instance.DeferMessage(cnt, reader, "Delay spawn for unowned " + netId);
			// 	return;
			// }

			PolusNetObject polusNetObject =
				Object.Instantiate(_spawnObjects[spawnType]);
			reader.ReadByte();
			int num5 = reader.ReadPackedInt32();
			PolusNetObject[] componentsInChildren =
				polusNetObject.GetComponentsInChildren<PolusNetObject>();
			if (num5 != componentsInChildren.Length) {
				Debug.LogError("Children didn't match for polus spawnable " + num5);
				Object.Destroy(polusNetObject.gameObject);
				return;
			}

			for (int i = 0; i < num5; i++) {
				PolusNetObject childNetObject = componentsInChildren[i];
				childNetObject.NetId = reader.ReadPackedUInt32();
				// polusNetObject4.OwnerId = num4;
				if (_destroyedObjects.Contains(childNetObject.NetId)) {
					polusNetObject.NetId = uint.MaxValue;
					Object.Destroy(polusNetObject.gameObject);
					return;
				}

				if (!AddNetObject(childNetObject)) {
					polusNetObject.NetId = uint.MaxValue;
					Object.Destroy(polusNetObject.gameObject);
					return;
				}

				MessageReader messageReader = reader.ReadMessage();
				if (messageReader.Length > 0) {
					childNetObject.GetType().FullName.Log(2);
					childNetObject.Deserialize(messageReader, true);
					"did it really".Log();
				}
			}

			polusNetObject.gameObject.active = true;
		}

		private bool AddNetObject(PolusNetObject polusNetObject) {
			uint num = polusNetObject.NetId + 1u;
			if (num > _netIdCnt)
			{
				_netIdCnt = num;
			}
			if (!_allObjectsFast.ContainsKey(polusNetObject.NetId))
			{
				_allObjects.Add(polusNetObject);
				_allObjectsFast.Add(polusNetObject.NetId, polusNetObject);
				return true;
			}
			return false;
		}

		public void RemoveNetObject(PolusNetObject obj) {
			obj.Despawn();
		}

		public bool HasObject(uint netId, out PolusNetObject obj) {
			return _allObjectsFast.TryGetValue(netId, out obj);
		}

		public bool IsDestroyed(uint netId) {
			return _destroyedObjects.Contains(netId);
		}

		public T FindObjectByNetId<T>(uint netId) where T : PolusNetObject {
			AmongUsClient instance = AmongUsClient.Instance;
			if (_allObjectsFast.TryGetValue(netId, out var polusNetObject)) {
				return (T) polusNetObject;
			}

			return default;
		}

		public void DestroyAll() {
			_allObjects.ForEach(x => Object.DestroyImmediate(x.gameObject));
			_allObjects = new();
			_allObjectsFast = new();
			_destroyedObjects = new();
			_netIdCnt = new();
		}

		public void UnregisterAll() {
			_spawnObjects = new Dictionary<uint, PolusNetObject>();
		}
	}
}