using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Hazel;
using InnerNet;
using PolusApi.Net;
using PolusGGMod.Patches.Net;
using UnityEngine;
using Object = UnityEngine.Object;

namespace PolusGGMod {
	public class PggObjectManager : IObjectManager {
		private Dictionary<uint, PolusNetObject> _allObjectsFast = new();
		private List<PolusNetObject> _allObjects = new();
		private HashSet<uint> _destroyedObjects = new();
		private Dictionary<int, PolusNetObject> _spawnObjects = new();
		private uint _netIdCnt = 0x80000000;
		private readonly object _objectLock = new Object();

		private event EventHandler<RpcEventArgs> RpcReceived;

		// [DllImport("user32.dll")]
		// private static extern void MessageBox(IntPtr hwnd, string text, string caption, uint type = 4);

		public void Register(int index, PolusNetObject netObject) {
		} 

		public event EventHandler<RpcEventArgs> InnerRpcReceived {
			add {
				lock (_objectLock) {
					RpcReceived += value;
				}
			}
			remove {
				lock (_objectLock) {
					RpcReceived -= value;
				}
			}
		}

		public void HandleInnerRpc(InnerNetObject netObject, RpcEventArgs args) {
			System.Console.WriteLine("bingus");
			RpcReceived?.Invoke(netObject, args);
		}

		public void HandleSpawn(int cnt, uint netId, MessageReader reader) {
			if (netId >= (ulong) _spawnObjects.Count) {
				Debug.LogError("Couldn't find spawnable prefab: " + netId);
				return;
			}

			int num4 = reader.ReadPackedInt32();
			ClientData clientData = PolusNetClient.FindClientById(num4);
			if (num4 > 0 && clientData == null) {
				AmongUsClient.Instance.DeferMessage(cnt, reader, "Delay spawn for unowned " + netId);
				return;
			}

			PolusNetObject polusNetObject =
				Object.Instantiate(_spawnObjects[(int) netId]);
			polusNetObject.SpawnFlags = (SpawnFlags) reader.ReadByte();
			if (polusNetObject.SpawnFlags != SpawnFlags.None) {
				AmongUsClient.Instance.HandleDisconnect(DisconnectReasons.Custom,
					"Spawn flags are unused pepega lmao cringe sad pepelaugh\n\nlmao\nbad");
			}

			int num5 = reader.ReadPackedInt32();
			PolusNetObject[] componentsInChildren =
				polusNetObject.GetComponentsInChildren<PolusNetObject>();
			if (num5 != componentsInChildren.Length) {
				Debug.LogError("Children didn't match for spawnable " + num5);
				Object.Destroy(polusNetObject.gameObject);
				return;
			}

			for (int i = 0; i < num5; i++) {
				PolusNetObject polusNetObject4 = componentsInChildren[i];
				polusNetObject4.NetId = reader.ReadPackedUInt32();
				polusNetObject4.OwnerId = num4;
				if (_destroyedObjects.Contains(polusNetObject4.NetId)) {
					polusNetObject.NetId = uint.MaxValue;
					Object.Destroy(polusNetObject.gameObject);
					return;
				}

				if (!this.AddNetObject(polusNetObject4)) {
					polusNetObject.NetId = uint.MaxValue;
					Object.Destroy(polusNetObject.gameObject);
					return;
				}

				MessageReader messageReader = reader.ReadMessage();
				if (messageReader.Length > 0) {
					polusNetObject4.Deserialize(messageReader, true);
				}
			}
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
			_spawnObjects = new Dictionary<int, PolusNetObject>();
		}
	}
}