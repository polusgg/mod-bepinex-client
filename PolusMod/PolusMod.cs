using System;
using System.Net;
using System.Threading.Tasks;
using BepInEx.Logging;
using Hazel;
using PolusApi;
using PolusApi.Net;
using PolusApi.Resources;
using UnhollowerRuntimeLib;
using UnityEngine;

namespace PolusMod {
    [Mod(Id, "1.0.0", "Sanae6")]
    public class TestPggMod : Mod {
        public const string Id = "PolusMod Lmoa";
        public bool loaded;
        public ICache Cache;

        public override void Load() {
            if (!loaded) {
                loaded = true;
            }

            Logger.LogInfo("Loaded " + Id);
        }

        public override void Start(IObjectManager objectManager, ICache cache) {
            objectManager.InnerRpcReceived += OnInnerRpcReceived;
            objectManager.Register(131, RegisterPnos.CreateDeadBodyPrefab());
            // GameObject lmao = new();
            // lmao.AddComponent<LmaoBehaviour>();
            Cache = cache;
        }

        public override void Unload() { }

        private void OnInnerRpcReceived(object sender, RpcEventArgs e) {
            InnerNetObject netObject = (InnerNetObject) sender;

            switch ((PolusRpcCalls) e.callId) {
                case PolusRpcCalls.SetCode:
                    GameStartManager.Instance.GameRoomName.Text = e.reader.ReadString();
                    break;
                case PolusRpcCalls.ChatVisibility:
                    var lol = e.reader.ReadByte() > 0;
                    // Logger.LogInfo($"Got it lmoao {HudManager.Instance.Chat.gameObject.active} {lol}");
                    if (HudManager.Instance.Chat.gameObject.active != lol)
                        HudManager.Instance.Chat.SetVisible(lol);
                    break;
                case PolusRpcCalls.CloseHud:
                    if (Minigame.Instance != null) Minigame.Instance.Close(true);
                    if (CustomPlayerMenu.Instance != null) CustomPlayerMenu.Instance.Close(true);
                    break;
            }
        }

        public override void HandleRoot(MessageReader reader) {
            switch (reader.Tag) {
                case 0x80:
                    uint resource = reader.ReadPackedUInt32();
                    string location = reader.ReadString();
                    byte[] hash = reader.ReadBytes(16);
                    uint resourceType = reader.ReadByte();
                    MessageWriter writer;
                    if (Cache.IsCachedAndValid(resource, hash)) {
                        writer = StartSendResourceResponse(resource, ResponseType.DownloadEnded);
                        writer.Write(1);
                        EndSendResourceResponse(writer);
                        return;
                    }

                    Task<CacheFile> resourceTask = Cache.AddToCache(resource, location, hash,
                        (ResourceType) resourceType);
                    resourceTask.Wait();
                    if (resourceTask.IsFaulted) {
                        writer = StartSendResourceResponse(resource, ResponseType.DownloadFailed);
                        if (resourceTask.Exception?.InnerException is CacheRequestException) {
                            writer.WritePacked((uint) ((CacheRequestException) resourceTask.Exception.InnerException)
                                .Code);
                        } else {
                            writer.WritePacked(0x69420);
                        }

                        EndSendResourceResponse(writer);
                    } else {
                        writer = StartSendResourceResponse(resource, ResponseType.DownloadEnded);
                        writer.Write((byte) 0);
                        EndSendResourceResponse(writer);
                    }

                    break;
                case 0x81:
                    break;
                case 0x90:
                    
                    break;
            }
        }

        private MessageWriter StartSendResourceResponse(uint resource, ResponseType type) {
            MessageWriter writer = MessageWriter.Get(SendOption.Reliable);
            writer.StartMessage(0x80);
            writer.WritePacked(resource);
            writer.Write((byte) type);
            return writer;
        }

        private void EndSendResourceResponse(MessageWriter writer) {
            writer.EndMessage();
            AmongUsClient.Instance.SendOrDisconnect(writer);
            writer.Recycle();
        }

        public override string Name => "PolusMod";
        public static ManualLogSource _loggee;

        public override ManualLogSource Logger {
            get => _loggee;
            set => _loggee = value;
        }

        public void SetPlayerAppearance(PlayerControl player, PlayerControl corpse) {
            if (player.name.Contains("Ludwig", StringComparison.InvariantCultureIgnoreCase)) {
                corpse.SetThickAssAndBigDumpy(true, true);
            }
        }
    }

    public static class Lol {
        public static void SetThickAssAndBigDumpy(this PlayerControl playerControl, bool isThick, bool hasBigDumpy) {
            //stub as fuck
        }
    }
}