using System;
using System.Collections.Generic;
using System.Linq;
using BepInEx.Logging;
using Hazel;
using InnerNet;
using PolusApi;
using PolusApi.Net;
using PolusApi.Resources;
using PolusMod.Pno;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace PolusMod {
    [Mod(Id, "1.0.0", "Sanae6")]
    public class TestPggMod : Mod {
        public const string Id = "PolusMod Lmoa";
        public bool loaded;
        public ICache Cache;
        public static RoleData RoleData = new();

        public override void Load() {
            if (!loaded) {
                loaded = true;
            }
            
            Logger.LogInfo("Loaded " + Id);
        }

        public override void Start(IObjectManager objectManager, ICache cache) {
            objectManager.InnerRpcReceived += OnInnerRpcReceived;
            objectManager.Register(0x83, RegisterPnos.CreateDeadBodyPrefab());
            objectManager.Register(0x81, RegisterPnos.CreateButton());

            //todo fix resolution issues changed lol when (maybe make a monobehaviour for receiving that)
            // ResolutionManager.ResolutionChanged.delegates = ResolutionManager.ResolutionChanged.delegates.AddItem(DelegateSupport.ConvertDelegate<Delegate>(new Action<float>((aspect) => {
            //     MessageWriter writer = MessageWriter.Get(SendOption.Reliable);
            //     writer.StartMessage((byte) PolusRootPackets.Resize);
            //     writer.Write((float) Screen.width);
            //     writer.Write((float) Screen.height);
            //     writer.EndMessage();
            //     AmongUsClient.Instance.SendOrDisconnect(writer);
            // }))).ToArray();
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
                            source.volume =
                                (volume - Vector2.Distance(PlayerControl.LocalPlayer.GetTruePosition(), vec2) -
                                 (
                                     PhysicsHelpers.AnythingBetween(
                                         vec2,
                                         PlayerControl.LocalPlayer.GetTruePosition(),
                                         LayerMask.NameToLayer("Ship") | LayerMask.NameToLayer("Players"),
                                         false
                                     )
                                         ? 15f
                                         : 0f)) / 100f;
                        }), sfx);
                    break;
            }
        }

        public override void HandleRoot(MessageReader reader) {
            Logger.LogInfo($"LOL {reader.Tag}");
            switch ((PolusRootPackets) reader.Tag) {
                case PolusRootPackets.FetchResource:
                    uint resource = reader.ReadPackedUInt32();
                    string location = reader.ReadString();
                    byte[] hash = reader.ReadBytes(16);
                    uint resourceType = reader.ReadByte();
                    MessageWriter writer;
                    if (Cache.IsCachedAndValid(resource, hash)) {
                        Logger.LogInfo($"{resource} is already cached");
                        writer = StartSendResourceResponse(resource, ResponseType.DownloadEnded);
                        writer.Write(1);
                        EndSendResourceResponse(writer);
                        return;
                    }

                    try {
                        Logger.LogInfo($"Trying to download and cache {resource}");
                        Cache.AddToCache(resource, location, hash,
                            (ResourceType) resourceType);
                        writer = StartSendResourceResponse(resource, ResponseType.DownloadEnded);
                        writer.Write(0);
                        EndSendResourceResponse(writer);
                        Logger.LogInfo($"Cached {resource}!");
                    } catch (Exception e) {
                        Logger.LogInfo($"Failed to cache {resource}");
                        writer = StartSendResourceResponse(resource, ResponseType.DownloadFailed);
                        if (e is CacheRequestException exception) {
                            writer.WritePacked((uint) exception.Code);
                        } else {
                            writer.WritePacked(0x69420);
                        }

                        EndSendResourceResponse(writer);
                    }

                    break;
                case PolusRootPackets.Intro:
                    RoleData.IntroName = reader.ReadString();
                    RoleData.IntroDesc = reader.ReadString();
                    RoleData.IntroColor = new Color32(reader.ReadByte(), reader.ReadByte(), reader.ReadByte(), reader.ReadByte());
                    RoleData.IntroPlayers = Enumerable.Repeat(15, reader.Length - reader.Position).Select(_ => reader.ReadByte()).ToList();
                    //todo finish this and outro
                    break;
                case PolusRootPackets.EndGame:
                    RoleData.OutroName = reader.ReadString();
                    RoleData.OutroDesc = reader.ReadString();
                    RoleData.OutroColor = new Color32(reader.ReadByte(), reader.ReadByte(), reader.ReadByte(), reader.ReadByte());
                    RoleData.OutroPlayers = Enumerable.Repeat(15, reader.Length - reader.Position - 2).Select(_ => new WinningPlayerData(GameData.Instance.GetPlayerById(reader.ReadByte()))).ToList();
                    RoleData.ShowQuit = reader.ReadBoolean();
                    RoleData.ShowPlayAgain = reader.ReadBoolean();
                    
                    // test go directly to endgame
                    // SceneManager.LoadScene("EndGame");
                    break;
                default:
                    Logger.LogError($"Invalid packet with id {reader.Tag}");
                    break;
            }
        }

        private MessageWriter StartSendResourceResponse(uint resource, ResponseType type) {
            Logger.LogInfo($"Starting response with type {type}");
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

    public class RoleData {
        public bool IsImpostor = false;
        public string IntroName = "Poobscoob";
        public string IntroDesc = "you failed to set this on time";
        public Color IntroColor = Color.magenta;
        public List<byte> IntroPlayers = new();
        public string OutroName;
        public string OutroDesc;
        public Color OutroColor;
        public List<WinningPlayerData> OutroPlayers;
        public bool ShowPlayAgain;
        public bool ShowQuit;
    }

    public static class Lol {
        public static void SetThickAssAndBigDumpy(this PlayerControl playerControl, bool isThick, bool hasBigDumpy) {
            //stub as fuck
        }
    }
}