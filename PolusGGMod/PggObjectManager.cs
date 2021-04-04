using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Hazel;
using InnerNet;
using PolusGG.Extensions;
using PolusGG.Net;
using UnityEngine;
using Object = UnityEngine.Object;

namespace PolusGG {
	public class PggObjectManager : IObjectManager {
		private Dictionary<uint, PolusNetObject> _allObjectsFast = new();
		private List<PolusNetObject> _allObjects = new();
		private HashSet<uint> _destroyedObjects = new();
		private Dictionary<uint, PnoBehaviour> _spawnObjects = new();
		private uint _netIdCnt = 0x80000000;

		[DllImport("user32.dll")]
		private static extern void MessageBox(IntPtr hwnd, string text, string caption, uint type = 4);
		//yeah ????? lol it's so fucking easy

		public void Register(uint index, PnoBehaviour netObject) {
			PogusPlugin.Logger.LogInfo($"Registered {netObject.GetType().Name} at index {index}");
			_spawnObjects[index] = netObject;
		}// you're laughin at me shut the fuck up

		public PolusNetObject LocateNetObject(PnoBehaviour netBehaviour) {
			return _allObjects.Find(x => x.PnoBehaviour.Pointer == netBehaviour.Pointer);
		}

		public PolusNetObject FindNetObject(uint netId) {
			return _allObjectsFast[netId];
		}

		public event EventHandler<RpcEventArgs> InnerRpcReceived;

		public void HandleInnerRpc(InnerNetObject netObject, RpcEventArgs args) {
			InnerRpcReceived?.Invoke(netObject, args);
		}

		public void HandleSpawn(int cnt, uint spawnType, MessageReader reader) {
			if (!_spawnObjects.ContainsKey(spawnType)) {
				Debug.LogError("Couldn't find polus spawnable prefab: " + spawnType);
				return;
			}

			/*int num4 = */reader.ReadPackedInt32();// todo is that readpackedint32 the owner id and does it matter at all
			// ClientData clientData = PolusNetClient.FindClientById(num4);
			// if (num4 > 0 && clientData == null) {
			// 	AmongUsClient.Instance.DeferMessage(cnt, reader, "Delay spawn for unowned " + netId);
			// 	return;
			

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

				netId.Log(4, "l");
				if (!AddNetObject(polusNetObject)) {
					polusNetObject.NetId = uint.MaxValue;
					Object.Destroy(polusNetObject.PnoBehaviour.gameObject);
					return;
				}

				MessageReader messageReader = reader.ReadMessage();
				if (messageReader.Length > 0) {
					// childNetObject.GetType().FullName.Log(2);
					polusNetObject.Spawn(messageReader);
					// "did it really".Log();
				}
			}

			pnoBehaviour.gameObject.active = true;
		}

		private bool AddNetObject(PolusNetObject polusNetObject) {
			// uint num = polusNetObject.NetId + 1u;
			// if (num > _netIdCnt)
			// {
			// 	_netIdCnt = num;
			// }
			_allObjectsFast.Log(2);
			polusNetObject.Log(2);
			polusNetObject.NetId.Log(2);
			
			_allObjects.Add(polusNetObject);
			_allObjectsFast.Add(polusNetObject.NetId, polusNetObject);
			// if (!_allObjectsFast.ContainsKey(polusNetObject.NetId))
			// {
				return true;
			// }
			// return false;
		}

		public void RemoveNetObject(PolusNetObject obj) {
			Object.Destroy(obj.PnoBehaviour);
		}

		public bool HasObject(uint netId, out PolusNetObject obj) {
			return _allObjectsFast.TryGetValue(netId, out obj);
		}

		public bool IsDestroyed(uint netId) {
			return _destroyedObjects.Contains(netId);
		}

		public T FindObjectByNetId<T>(uint netId) where T : PolusNetObject {
			if (_allObjectsFast.TryGetValue(netId, out var polusNetObject)) {
				return (T) polusNetObject;
			}

			return default;
		}

		public void EndedGame() {
			PogusPlugin.Logger.LogInfo("Lmao");
			_allObjects = new();
			_allObjectsFast = new();
			_destroyedObjects = new();
			_netIdCnt = new();
		}

		public void UnregisterAll() {
			_spawnObjects = new Dictionary<uint, PnoBehaviour>();
		}
	}
}