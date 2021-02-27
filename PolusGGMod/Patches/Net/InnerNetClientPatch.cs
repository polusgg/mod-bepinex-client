using HarmonyLib;
using Hazel;
using Il2CppSystem;
using Il2CppSystem.Collections.Generic;
using InnerNet;
using PolusApi.Net;
using UnityEngine;
using Object = UnityEngine.Object;

namespace PolusGGMod.Patches.Net {
	public class PolusNetClient {
		[HarmonyPatch(typeof(InnerNetClient), nameof(InnerNetClient.Method_16))]
		public class InnerNetClientHandleGameDataInnerPatch {
			public static ClientData FindClientById(int id) {
				AmongUsClient instance = AmongUsClient.Instance;
				if (id < 0) {
					return null;
				}

				List<ClientData> obj = instance.allClients;
				ClientData result;
				lock (obj) {
					for (int i = 0; i < instance.allClients.Count; i++) {
						ClientData clientData = instance.allClients[i];
						if (clientData.Id == id) {
							return clientData;
						}
					}

					result = null;
				}

				return result;
			}


			[HarmonyPrefix]
						
			public static bool HandleGameDataInner([HarmonyArgument(0)] MessageReader reader,
				[HarmonyArgument(1)] int cnt) {
				AmongUsClient instance = AmongUsClient.Instance;
				switch (reader.Tag) {
					case 1: {
						uint netId = reader.ReadPackedUInt32();
						if (netId >> 31 > 0) {
							if (IObjectManager.Instance.HasObject(netId, out var polusNetObject)) {
								polusNetObject.Deserialize(reader, false);
								return false;
							}

							if (!IObjectManager.Instance.IsDestroyed(netId)) {
								instance.DeferMessage(cnt, reader, "Stored data for " + netId);
								return false;
							}
						}

						if (instance.allObjectsFast.TryGetValue(netId, out var innerNetObject)) {
							innerNetObject.Deserialize(reader, false);
							return false;
						}

						if (!instance.DestroyedObjects.Contains(netId)) {
							instance.DeferMessage(cnt, reader, "Stored data for " + netId);
							return false;
						}

						return false;
					}
					case 2: {
						uint netId = reader.ReadPackedUInt32();
						if (netId >> 31 > 0) {
							if (IObjectManager.Instance.HasObject(netId, out var polusNetObject)) {
								polusNetObject.HandleRpc(reader.ReadByte(), reader);
								return false;
							}

							if (netId != 4294967295U && !IObjectManager.Instance.IsDestroyed(netId)) {
								instance.DeferMessage(cnt, reader, "Stored Polus RPC for " + netId);
								return false;
							}
						}

						if (instance.allObjectsFast.TryGetValue(netId, out var innerNetObject2)) {
							byte call = reader.ReadByte();
							if (call > 0x80)
								IObjectManager.Instance.HandleRpcInner(innerNetObject2, call, reader);
							else innerNetObject2.HandleRpc(call, reader);
							return false;
						}

						if (netId != 4294967295U && !instance.DestroyedObjects.Contains(netId)) {
							instance.DeferMessage(cnt, reader, "Stored RPC for " + netId);
							return false;
						}

						return false;
					}
					case 4: {
						uint num3 = reader.ReadPackedUInt32();

						if (num3 >> 31 > 0) {
							IObjectManager.Instance.HandleSpawn(cnt, num3, reader);
							return false;
						}

						//todo spawning polus spawnable objects

						if (num3 >= (ulong) instance.SpawnableObjects.Length) {
							Debug.LogError("Couldn't find spawnable prefab: " + num3);
							return false;
						}

						int num4 = reader.ReadPackedInt32();
						ClientData clientData = FindClientById(num4);
						if (num4 > 0 && clientData == null) {
							instance.DeferMessage(cnt, reader, "Delay spawn for unowned " + num3);
							return false;
						}

						InnerNetObject innerNetObject3 =
							Object.Instantiate(instance.SpawnableObjects[(int) num3]);
						innerNetObject3.SpawnFlags = (SpawnFlags) reader.ReadByte();
						if ((innerNetObject3.SpawnFlags & SpawnFlags.IsClientCharacter) != SpawnFlags.None) {
							if (!clientData.Character) {
								clientData.InScene = true;
								clientData.Character = (innerNetObject3 as PlayerControl);
							}
							else if (innerNetObject3) {
								Debug.LogWarning(
									$"Double spawn character: {clientData.Id} already has {clientData.Character.NetId}");
								Object.Destroy(innerNetObject3.gameObject);
								return false;
							}
						}

						int num5 = reader.ReadPackedInt32();
						InnerNetObject[] componentsInChildren =
							innerNetObject3.GetComponentsInChildren<InnerNetObject>();
						if (num5 != componentsInChildren.Length) {
							Debug.LogError("Children didn't match for spawnable " + num3);
							Object.Destroy(innerNetObject3.gameObject);
							return false;
						}

						for (int i = 0; i < num5; i++) {
							InnerNetObject innerNetObject4 = componentsInChildren[i];
							innerNetObject4.NetId = reader.ReadPackedUInt32();
							innerNetObject4.OwnerId = num4;
							if (instance.DestroyedObjects.Contains(innerNetObject4.NetId)) {
								innerNetObject3.NetId = uint.MaxValue;
								Object.Destroy(innerNetObject3.gameObject);
								return false;
							}

							if (!instance.AddNetObject(innerNetObject4)) {
								innerNetObject3.NetId = uint.MaxValue;
								Object.Destroy(innerNetObject3.gameObject);
								return false;
							}

							MessageReader messageReader = reader.ReadMessage();
							if (messageReader.Length > 0) {
								innerNetObject4.Deserialize(messageReader, true);
							}
						}

						return false;
					}
					case 5: {
						uint num6 = reader.ReadPackedUInt32();
						instance.DestroyedObjects.Add(num6);
						if (num6 >> 31 > 0) {
							PolusNetObject polusNetObject = IObjectManager.Instance.FindObjectByNetId<PolusNetObject>(num6);//todo pno despawns
							if (polusNetObject) {
								IObjectManager.Instance.RemoveNetObject(polusNetObject);
							}
						}
						InnerNetObject innerNetObject5 = instance.FindObjectByNetId<InnerNetObject>(num6);
						if (innerNetObject5 && !innerNetObject5.AmOwner) {
							instance.RemoveNetObject(innerNetObject5);
							Object.Destroy(innerNetObject5.gameObject);
							return false;
						}

						return false;
					}
					case 6: {
						int num7 = reader.ReadPackedInt32();
												
						ClientData client = FindClientById(num7);
						string targetScene = reader.ReadString();
						if (client != null && !string.IsNullOrWhiteSpace(targetScene)) {
							List<Action> dispatcher = instance.Dispatcher;
							lock (dispatcher) {
								instance.Dispatcher.Add((new System.Action(delegate {
									instance.OnPlayerChangedScene(client, targetScene);
								})));
								return false;
							}
						}

						Debug.Log($"Couldn't find client {num7} to change scene to {targetScene}");
						return false;
					}
					case 7: {
						ClientData clientData2 = FindClientById(reader.ReadPackedInt32());
						if (clientData2 != null) {
							clientData2.IsReady = true;
							return false;
						}

						return false;
					}
				}

				Debug.Log(string.Format("Pgg bad tag {0} at {1}+{2}={3}:  ", new object[] {
					reader.Tag,
					reader.Offset,
					reader.Position,
					reader.Length
				}) + string.Join<byte>(" ", reader.Buffer));
				return false;
			}
		}
	}
}