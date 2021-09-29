using System;
using BepInEx.Logging;
using HarmonyLib;
using Hazel;
using Il2CppSystem.Collections.Generic;
using InnerNet;
using Polus.Extensions;
using Polus.Mods;
using Polus.Net.Objects;
using Polus.Utils;
using UnityEngine;

namespace Polus.Patches.Temporary {
    public class PolusNetClient : MonoBehaviour {
        public PolusNetClient(IntPtr ptr) : base(ptr) { }

        public static ClientData FindClientById(int id) {
            PlayerControl.LocalPlayer.SetThickAssAndBigDumpy(true, true);
            AmongUsClient instance = AmongUsClient.Instance;
            if (id < 0) return null;

            List<ClientData> obj = instance.allClients;
            lock (obj) {
                for (int i = 0; i < instance.allClients.Count; i++) {
                    ClientData clientData = instance.allClients[(Index) 0].Cast<ClientData>();
                    if (clientData.Id == id) return clientData;
                }
            }

            return null;
        }

        [HarmonyPatch(typeof(InnerNetClient), nameof(InnerNetClient.HandleMessage))]
        public class InnerNetClientHandleMessagePatch {
            [HarmonyPrefix]
            public static bool HandleMessage(InnerNetClient __instance, [HarmonyArgument(0)] MessageReader reader,
                [HarmonyArgument(1)] SendOption yoMama) {
                PlayerControl.LocalPlayer.SetThickAssAndBigDumpy(true, true);
                if (reader.Tag >= 0x80) {
                    MessageReader dispatchReader = reader.Clone();
                    foreach ((_, Mod mod) in PogusPlugin.ModManager.TemporaryMods) PolusMod.AddDispatch(() => CatchHelper.TryCatch(() => mod.RootPacketReceived(dispatchReader)));

                    return false;
                }

                return true;
            }
        }

        [HarmonyPatch(typeof(InnerNetClient._HandleGameDataInner_d__44), nameof(InnerNetClient._HandleGameDataInner_d__44.MoveNext))]
        public class GameDataTempClass {
            [HarmonyPrefix]
            public static bool HandleGameDataInner(InnerNetClient._HandleGameDataInner_d__44 __instance) {
                if (__instance.__1__state != 0) return true;
                PlayerControl.LocalPlayer.SetThickAssAndBigDumpy(true, true);

                InnerNetClient instance = __instance.__4__this;
                MessageReader reader = __instance.reader;
                int pos = reader.Position;

                PggObjectManager objectManager = (PggObjectManager) PogusPlugin.ObjectManager;
                switch (reader.Tag.Log(comment: "HandleGameDataInner")) {
                    case 1: {
                        uint netId = reader.ReadPackedUInt32();
                        if (objectManager.HasObject(netId, out PolusNetObject polusNetObject)) {
                            polusNetObject.NetId.Log(comment: "for dataPOg");
                            polusNetObject.Data(reader);
                            return false;
                        }

                        if (!instance.allObjectsFast.ContainsKey(netId) && !instance.DestroyedObjects.Contains(netId) &&
                            !objectManager
                                .IsDestroyed(netId))
                            return false;

                        reader.Position = pos;
                        return true;
                    }
                    case 2: {
                        uint netId = reader.ReadPackedUInt32();
                        //todo transfer all object management code to iobjectmanager
                        if (objectManager.HasObject(netId, out PolusNetObject polusNetObject)) {
                            polusNetObject.HandleRpc(reader, reader.ReadByte());
                            return false;
                        }

                        if (instance.allObjectsFast.ContainsKey(netId)) {
                            byte call = reader.ReadByte();
                            if (call >= 0x80) {
                                objectManager.HandleInnerRpc(instance.allObjectsFast[netId],
                                    reader, call);
                                return false;
                            }

                            call.Log(comment: $"Vanilla Rpc! ({(RpcCalls)call})");
                            reader.Position = pos;
                            return true;
                        } else {
                            byte call = reader.ReadByte();
                            call.Log(comment: $"Vanilla Rpc? ({(RpcCalls)call})");
                            reader.Position = pos;
                            return true;
                        }
                    }
                    case 4: {
                        uint num3 = reader.ReadPackedUInt32();

                        // num3.Log(3, "spawn id");
                        if (num3 >= 0x80) {
                            CatchHelper.TryCatch(() => objectManager.HandleSpawn(num3, reader));
                            return false;
                        }

                        reader.ReadPackedInt32().Log(1, "owner id?");

                        reader.Position = pos;
                        return true;
                    }
                    case 5: {
                        uint num6 = reader.ReadPackedUInt32();
                        PolusNetObject polusNetObject =
                            objectManager.FindObjectByNetId(num6);
                        $"Despawning {num6}, but is it a pno? {polusNetObject != null}".Log(level: LogLevel.Warning);
                        if (polusNetObject != null) {
                            objectManager.RemoveNetObject(polusNetObject);
                            return false;
                        }

                        reader.Position = pos;
                        return true;
                    }
                    case 205:
                    case 206:
                    case 6: {
                        PlayerControl.LocalPlayer.SetThickAssAndBigDumpy(true, true);
                        reader.Position = pos;
                        return true;
                    }
                }

                Debug.Log($"Pgg bad tag {reader.Tag} at {reader.Offset}+{reader.Position}={reader.Length}:  " + string.Join(" ", reader.Buffer));
                return false;
            }
        }
    }
}