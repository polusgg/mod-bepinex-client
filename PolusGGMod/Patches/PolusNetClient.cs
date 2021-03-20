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
        public static ClientData FindClientById(int id) {
            AmongUsClient instance = AmongUsClient.Instance;
            if (id < 0) {
                return null;
            }

            List<ClientData> obj = instance.allClients;
            lock (obj) {
                for (int i = 0; i < instance.allClients.Count; i++) {
                    ClientData clientData = instance.allClients[i];
                    if (clientData.Id == id) {
                        return clientData;
                    }
                }
            }

            return null;
        }

        [HarmonyPatch(typeof(AmongUsClient), nameof(AmongUsClient.OnDisconnect))]
        public class InnerNetClientLolDestroyAllPatch {
            [PermanentPatch]
            [HarmonyPrefix]
            public static bool OnDisconnect() {
                // if (PogusPlugin.ModManager.AllPatched) {
                    IObjectManager.Instance.EndedGame();
                // }
                return true;
            }
        }

        [HarmonyPatch(typeof(InnerNetClient), nameof(InnerNetClient.HandleMessage))]
        public class InnerNetClientHandleMessagePatch {
            [PermanentPatch]
            [HarmonyPrefix]
            public static bool HandleMessage([HarmonyArgument(0)] MessageReader reader,
                [HarmonyArgument(1)] SendOption yoMama) {
                // PogusPlugin.Logger.LogInfo($"Root packet {reader.Tag:X2}");
                if (reader.Tag >= 0x80) {
                    foreach (var (_, mod) in PogusPlugin.ModManager.TemporaryMods) {
                        mod.HandleRoot(reader);
                    }

                    return false;
                }

                return true;
            }
        }

        [HarmonyPatch(typeof(InnerNetClient), nameof(InnerNetClient.Method_16))]
        public class InnerNetClientHandleGameDataInnerPatch {
            [PermanentPatch]
            [HarmonyPrefix]
            public static bool HandleGameDataInner(InnerNetClient __instance, [HarmonyArgument(0)] MessageReader reader,
                [HarmonyArgument(1)] int cnt) {
                InnerNetClient instance = __instance;
                InnerNetObject netObject;
                int pos = reader.Position;

                switch (reader.Tag) {
                    case 1: {
                        uint netId = reader.ReadPackedUInt32();
                        //todo transfer all object management code to iobjectmanager
                        if (IObjectManager.Instance.HasObject(netId, out var polusNetObject)) {
                            polusNetObject.NetId.Log(1, "for dataPOg");
                            polusNetObject.Data(reader);
                            return false;
                        }

                        if (!instance.DestroyedObjects.Contains(netId) && !IObjectManager.Instance.IsDestroyed(netId)) {
                            instance.DeferMessage(cnt, reader, "Stored data for " + netId);
                            return false;
                        }
                        
                        reader.Position = pos.Log(1, "lol send it baby");
                        return true;
                    }
                    case 2: {
                        uint netId = reader.ReadPackedUInt32();
                        //todo transfer all object management code to iobjectmanager
                        if (IObjectManager.Instance.HasObject(netId, out var polusNetObject)) {
                            polusNetObject.HandleRpc(reader, reader.ReadByte());
                            return false;
                        }

                        if (instance.allObjectsFast.ContainsKey(netId)) {
                            byte call = reader.ReadByte();
                            // PogusPlugin.Logger.LogInfo($"WOO RPC FOR {call}");
                            if (call >= 0x80)
                                IObjectManager.Instance.HandleInnerRpc(instance.allObjectsFast[netId], new RpcEventArgs(call, reader));
                            else instance.allObjectsFast[netId].HandleRpc(call, reader);
                            return false;
                        }

                        if (netId != 4294967295U && !instance.DestroyedObjects.Contains(netId) && !IObjectManager.Instance.IsDestroyed(netId)) {
                            instance.DeferMessage(cnt, reader, "Stored RPC for " + netId);
                            return false;
                        }

                        return false;
                    }
                    case 4: {
                        uint num3 = reader.ReadPackedUInt32();

                        if (num3 >= 0x80) {
                            IObjectManager.Instance.HandleSpawn(cnt, num3, reader);
                            return false;
                        }
                        
                        reader.Position = pos;
                        return true;

                        // if (num3 >= (ulong) instance.SpawnableObjects.Length) {
                        //     Debug.LogError($"Couldn't find spawnable prefab: {num3}");
                        //     return false;
                        // }
                        //
                        // int num4 = reader.ReadPackedInt32();
                        // ClientData clientData = FindClientById(num4);
                        // if (num4 > 0 && clientData == null) {
                        //     instance.DeferMessage(cnt, reader, "Delay spawn for unowned " + num3);
                        //     return false;
                        // }
                        //
                        // netObject = Object.Instantiate<InnerNetObject>(instance.SpawnableObjects[(int) num3]);
                        // PogusPlugin.Logger.LogInfo($"Spawned lomoa {netObject.NetId} {netObject.name}");
                        // netObject.SpawnFlags = (SpawnFlags) reader.ReadByte();
                        // if ((netObject.SpawnFlags & SpawnFlags.IsClientCharacter) != SpawnFlags.None) {
                        //     if (!clientData.Character) {
                        //         clientData.InScene = true;
                        //         clientData.Character = (netObject as PlayerControl);
                        //     }
                        //     else if (netObject) {
                        //         Debug.LogWarning(string.Format("Double spawn character: {0} already has {1}",
                        //             clientData.Id, clientData.Character.NetId));
                        //         Object.Destroy(netObject.gameObject);
                        //         return false;
                        //     }
                        // }
                        //
                        // int num5 = reader.ReadPackedInt32();
                        // InnerNetObject[] componentsInChildren = netObject.GetComponentsInChildren<InnerNetObject>();
                        // if (num5 != componentsInChildren.Length) {
                        //     Debug.LogError("Children didn't match for spawnable " + num3);
                        //     Object.Destroy(netObject.gameObject);
                        //     return false;
                        // }
                        //
                        // for (int i = 0; i < num5; i++) {
                        //     InnerNetObject innerNetObject4 = componentsInChildren[i];
                        //     innerNetObject4.NetId = reader.ReadPackedUInt32();
                        //     innerNetObject4.OwnerId = num4;
                        //     if (instance.DestroyedObjects.Contains(innerNetObject4.NetId)) {
                        //         netObject.NetId = uint.MaxValue;
                        //         Object.Destroy(netObject.gameObject);
                        //         return false;
                        //     }
                        //
                        //     if (!instance.AddNetObject(innerNetObject4)) {
                        //         netObject.NetId = uint.MaxValue;
                        //         Object.Destroy(netObject.gameObject);
                        //         return false;
                        //     }
                        //
                        //     MessageReader messageReader = reader.ReadMessage();
                        //     if (messageReader.Length > 0) {
                        //         innerNetObject4.Deserialize(messageReader, true);
                        //     }
                        // }
                        //
                        // return false;
                    }
                    case 5: {
                        uint num6 = reader.ReadPackedUInt32();
                        instance.DestroyedObjects.Add(num6);
                        PolusNetObject polusNetObject =
                            IObjectManager.Instance.FindObjectByNetId<PolusNetObject>(num6); //todo pno despawns
                        if (polusNetObject != null) {
                            IObjectManager.Instance.RemoveNetObject(polusNetObject);
                            return false;
                        }

                        netObject = instance.FindObjectByNetId<InnerNetObject>(num6);
                        if (netObject && !netObject.AmOwner) {
                            instance.RemoveNetObject(netObject);
                            Object.Destroy(netObject.gameObject);
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