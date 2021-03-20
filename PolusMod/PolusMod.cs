using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using BepInEx.Logging;
using HarmonyLib;
using Hazel;
using PolusApi;
using PolusApi.Net;
using PolusApi.Resources;
using PolusMod.Pno;
using UnhollowerBaseLib;
using UnhollowerRuntimeLib;
using UnityEngine;
using Delegate = Il2CppSystem.Delegate;

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
            objectManager.Register(0x83, RegisterPnos.CreateDeadBodyPrefab());
            objectManager.Register(0x80, RegisterPnos.CreateImage());
            ResolutionManager.ResolutionChanged.delegates = ResolutionManager.ResolutionChanged.delegates.AddItem(DelegateSupport.ConvertDelegate<Delegate>(new Action<float>((aspect) => {
                MessageWriter writer = MessageWriter.Get(SendOption.Reliable);
                writer.StartMessage((byte) PolusRootPackets.Resize);
                writer.Write((float) Screen.width);
                writer.Write((float) Screen.height);
                writer.EndMessage();
                AmongUsClient.Instance.SendOrDisconnect(writer);
            }))).ToArray();
            Cache = cache;
        }

        public override void Unload() { }

        private void OnInnerRpcReceived(object sender, RpcEventArgs e) {
            InnerNetObject netObject = (InnerNetObject) sender;

            switch ((PolusRpcCalls) e.callId) {
                case PolusRpcCalls.SetString:
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
                case PolusRpcCalls.PlaySound:
                    AudioClip ac = Cache.CachedFiles[e.reader.ReadPackedUInt32()].Get<AudioClip>();
                    bool sfx = e.reader.ReadBoolean();
                    bool loop = e.reader.ReadBoolean();
                    byte volume = e.reader.ReadByte();
                    Vector2 vec2 = PolusNetworkTransform.ReadVector2(e.reader);
                    
                    SoundManager.Instance.PlayDynamicSound(ac.name, ac, loop, new Action<AudioSource, float>(
                        (source, _) => {
                            source.volume = (volume - Vector2.Distance(PlayerControl.LocalPlayer.GetTruePosition(), vec2) -
                                             (
                                                 PhysicsHelpers.AnythingBetween(
                                                     vec2,
                                                     PlayerControl.LocalPlayer.GetTruePosition(),
                                                     LayerMask.NameToLayer("Ship") | LayerMask.NameToLayer("Players"),
                                                     false
                                                     ) ? 15f : 0f)) / 100f;
                        }), sfx);
                    break;
            }
        }

        public override void HandleRoot(MessageReader reader) {
            switch ((PolusRootPackets) reader.Tag) {
                case PolusRootPackets.FetchResource:
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
                        if (resourceTask.Exception?.InnerException is CacheRequestException exception) {
                            writer.WritePacked((uint) exception.Code);
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
                case PolusRootPackets.Intro:
                    
                    break;
                case PolusRootPackets.EndGame:
                    
                    break;
            }
        }

        private MessageWriter StartSendResourceResponse(uint resource, ResponseType type) {
            MessageWriter writer = MessageWriter.Get(SendOption.Reliable);
            writer.StartMessage((byte) PolusRootPackets.FetchResource);
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