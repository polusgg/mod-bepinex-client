using System;
using HarmonyLib;
using Hazel;
using Il2CppSystem.Collections.Generic;
using InnerNet;
using PolusGG.Extensions;
using PolusGG.Mods;
using PolusGG.Net;
using UnityEngine;

namespace PolusGG.Patches.Permanent {
    public class PolusNetClient : MonoBehaviour {
        public PolusNetClient(IntPtr ptr) : base(ptr) {}

        public static ClientData FindClientById(int id) {
            AmongUsClient instance = AmongUsClient.Instance;
            if (id < 0) {
                return null;
            }

            List<ClientData> obj = instance.allClients;
            lock (obj) {
                for (int i = 0; i < instance.allClients.Count; i++) {
                    ClientData clientData = instance.allClients[(Index) 0].Cast<ClientData>();
                    if (clientData.Id == id) {
                        return clientData;
                    }
                }
            }

            return null;
        }

        [HarmonyPatch(typeof(InnerNetClient), nameof(InnerNetClient.HandleMessage))]
        public class InnerNetClientHandleMessagePatch {
            [PermanentPatch]
            [HarmonyPrefix]
            public static bool HandleMessage(InnerNetClient __instance, [HarmonyArgument(0)] MessageReader reader,
                [HarmonyArgument(1)] SendOption yoMama) {
                if (reader.Tag >= 0x80) {
                    foreach ((_, Mod mod) in PogusPlugin.ModManager.TemporaryMods) {
                        PogusPlugin.Logger.LogInfo($"Handling packet {reader.Tag:X2} for {mod.Name}");
                        mod.RootPacketReceived(reader);
                    }

                    return false;
                }

                return true;
            }
        }

        [HarmonyPatch(typeof(InnerNetClient._HandleGameDataInner_d__42), nameof(InnerNetClient._HandleGameDataInner_d__42.MoveNext))]
        public class GameDataTempClass {
            [PermanentPatch]
            [HarmonyPrefix]
            public static bool HandleGameDataInner(InnerNetClient._HandleGameDataInner_d__42 __instance) {
                if (__instance.__state == 0) {}
                InnerNetClient instance = __instance.__this;
                MessageReader reader = __instance.reader;
                int msgCnt = __instance.msgNum;
                InnerNetObject netObject;
                int pos = reader.Position;
                
                switch (reader.Tag.Log(4, "Game Data")) {
                    case 1: {
                        uint netId = reader.ReadPackedUInt32();
                        //todo transfer all object management code to iobjectmanager
                        if (IObjectManager.Instance.HasObject(netId, out var polusNetObject)) {
                            polusNetObject.NetId.Log(1, "for dataPOg");
                            polusNetObject.Data(reader);
                            return false;
                        }
                
                        if (!instance.allObjectsFast.ContainsKey(netId) && !instance.DestroyedObjects.Contains(netId) &&
                            !IObjectManager.Instance.IsDestroyed(netId)) {
                            // DeferMessage(cnt, reader, "Stored data for " + netId);
                            return false;
                        }
                
                        reader.Position = pos;
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
                            PogusPlugin.Logger.LogInfo($"WOO RPC FOR {call}");
                            if (call >= 0x80)
                                IObjectManager.Instance.HandleInnerRpc(instance.allObjectsFast[netId],
                                    new RpcEventArgs(call, reader));
                            else instance.allObjectsFast[netId].HandleRpc(call, reader);
                            return false;
                        }
                
                        if (netId != 4294967295U && !instance.DestroyedObjects.Contains(netId) &&
                            !IObjectManager.Instance.IsDestroyed(netId)) {
                            // DeferMessage(cnt, reader, "Stored RPC for " + netId);
                            return false;
                        }
                
                        return false;
                    }
                    case 4: {
                        uint num3 = reader.ReadPackedUInt32();
                
                        num3.Log(6, "SpNwan iDdd");
                        if (num3 >= 0x80) {
                            IObjectManager.Instance.HandleSpawn(num3, reader);
                            return false;
                        }
                
                        reader.Position = pos;
                        return true;
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
                
                        reader.Position = pos;
                        return true;
                    }
                    case 7:
                    case 6: {
                        reader.Position = pos;
                        return true;
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