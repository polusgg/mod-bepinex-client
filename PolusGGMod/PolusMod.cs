using System;
using System.Collections.Generic;
using System.Linq;
using BepInEx.Logging;
using Hazel;
using InnerNet;
using PolusGG.Behaviours;
using PolusGG.Behaviours.Inner;
using PolusGG.Enums;
using PolusGG.Extensions;
using PolusGG.Mods;
using PolusGG.Net;
using PolusGG.Patches.Temporary;
using PolusGG.Resources;
using TMPro;
using UnityEngine;
using Object = UnityEngine.Object;

namespace PolusGG {
    [Mod(Id, "1.0.0", "Sanae6")]
    public class PolusMod : Mod {
        private const string Id = "PolusMain";
        public static RoleData RoleData = new();
        public static ManualLogSource _loggee;
        private static readonly int OutlineColor = Shader.PropertyToID("_OutlineColor");
        private static readonly int Outline = Shader.PropertyToID("_Outline");
        private ICache Cache;
        private bool loaded;
        private bool optionsDirty;
        public static PolusMod Instance;

        public override string Name => "PolusMod";

        public override ManualLogSource Logger {
            get => _loggee;
            set => _loggee = value;
        }

        public override void Start() {
            Instance = this;
            if (loaded) return;

            loaded = true;

            PogusPlugin.ObjectManager.InnerRpcReceived += OnInnerRpcReceived;
            PogusPlugin.ObjectManager.Register(0x80, RegisterPnos.CreateImage());
            PogusPlugin.ObjectManager.Register(0x81, RegisterPnos.CreateButton());
            PogusPlugin.ObjectManager.Register(0x83, RegisterPnos.CreateDeadBodyPrefab());
            PogusPlugin.ObjectManager.Register(0x85, RegisterPnos.CreateSoundSource());
            PogusPlugin.ObjectManager.Register(0x87, RegisterPnos.CreatePoi());
            PogusPlugin.ObjectManager.Register(0x88, RegisterPnos.CreateCameraController());
            PogusPlugin.ObjectManager.Register(0x89, RegisterPnos.CreatePrefabHandle());

            ResolutionManagerPlus.Resolution();
            Cache = PogusPlugin.Cache;
        }

        public override void Stop() {
            // if (PolusDiscordManager.Instance) Object.Destroy(PolusDiscordManager.Instance.gameObject);
            // DiscordManager.Instance.Start();
        }

        private void OnInnerRpcReceived(InnerNetObject netObject, MessageReader reader, byte callId) {
            PlayerControl playerControl;
            switch ((PolusRpcCalls) callId) {
                case PolusRpcCalls.ChatVisibility: {
                    bool lol = reader.ReadByte() > 0;
                    if (HudManager.Instance.Chat.gameObject.active != lol)
                        HudManager.Instance.Chat.SetVisible(lol);
                    break;
                }
                case PolusRpcCalls.CloseHud: {
                    if (Minigame.Instance != null) Minigame.Instance.Close(true);
                    if (CustomPlayerMenu.Instance != null) CustomPlayerMenu.Instance.Close(true);
                    break;
                }
                case PolusRpcCalls.SetRole: {
                    playerControl = netObject.Cast<PlayerControl>();

                    if (reader.ReadBoolean()) {
                        if (playerControl == PlayerControl.LocalPlayer) {
                            DestroyableSingleton<HudManager>.Instance.KillButton.gameObject.SetActive(true);
                            playerControl.SetKillTimer(PlayerControl.GameOptions.KillCooldown);
                        }

                        if (PlayerControl.LocalPlayer.Data.IsImpostor)
                            playerControl.Data.Object.nameText.color = Palette.ImpostorRed;
                    } else {
                        if (playerControl == PlayerControl.LocalPlayer) {
                            playerControl.SetKillTimer(21f);
                            HudManager.Instance.KillButton.gameObject.SetActive(false);
                            foreach (GameData.PlayerInfo player in GameData.Instance.AllPlayers)
                                player.Object.nameText.color = Palette.White;
                        }

                        playerControl.Data.Object.nameText.color = Palette.White;
                    }

                    foreach (GameData.PlayerInfo player in GameData.Instance.AllPlayers) {
                        $"{player.Object.name} - {player.IsImpostor}".Log();
                        player.Object.nameText.color = player.IsImpostor && PlayerControl.LocalPlayer.Data.IsImpostor
                            ? Palette.ImpostorRed
                            : Palette.White;
                    }

                    break;
                }
                case PolusRpcCalls.Revive: {
                    netObject.Cast<PlayerControl>().Revive();
                    break;
                }
                case PolusRpcCalls.SetHat: {
                    Cache.CachedFiles[reader.ReadPackedUInt32()].Get<Sprite>();
                    Cache.CachedFiles[reader.ReadPackedUInt32()].Get<Sprite>();
                    Cache.CachedFiles[reader.ReadPackedUInt32()].Get<Sprite>();
                    break;
                }
                case PolusRpcCalls.SetOpacity: {
                    "SET PASSITY".Log(10);
                    PlayerControl control = netObject.Cast<PlayerControl>();
                    Color32 color = control.myRend.color;
                    color.a = reader.ReadByte();
                    control.myRend.color = color;
                    control.HatRenderer.color = color;
                    control.MyPhysics.Skin.layer.color = color;
                    control.CurrentPet.rend.color = color;
                    break;
                }
                case PolusRpcCalls.SetOutline: {
                    PlayerControl control = netObject.Cast<PlayerControl>();
                    control.myRend.material.SetFloat(Outline, reader.ReadByte());
                    control.myRend.material.SetColor(OutlineColor, new Color32(reader.ReadByte(), reader.ReadByte(), reader.ReadByte(), reader.ReadByte()));
                    break;
                }
                case PolusRpcCalls.DespawnAllVents: {
                    IEnumerable<int> enumerable =
                        Enumerable.Range(0, reader.ReadByte()).Select(_ => (int) reader.ReadByte());
                    IEnumerable<Vent> vents = Object.FindObjectsOfType<Vent>()
                        .Where(v => enumerable.Contains(v.Id));
                    foreach (Vent vent in vents) Object.Destroy(vent);

                    break;
                }
                case PolusRpcCalls.BeginAnimationPlayer:
                    netObject.gameObject.EnsureComponent<PlayerAnimPlayer>().HandleMessage(reader);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public override void RootPacketReceived(MessageReader reader) {
            // Logger.LogInfo($"LOL {reader.Tag}");
            switch ((PolusRootPackets) reader.Tag) {
                case PolusRootPackets.FetchResource: {
                    uint resource = reader.ReadPackedUInt32();
                    string location = reader.ReadString();
                    byte[] hash = reader.ReadBytes(32);
                    uint resourceType = reader.ReadByte();
                    MessageWriter writer;
                    if (Cache.IsCachedAndValid(resource, hash)) {
                        Logger.LogInfo($"{resource} is already cached");
                        writer = StartSendResourceResponse(resource, ResponseType.DownloadEnded);
                        writer.Write(1);
                        EndSend(writer);
                        return;
                    }

                    try {
                        Logger.LogInfo($"Trying to download and cache {resource} ({location})");
                        Cache.AddToCache(resource, location, hash,
                            (ResourceType) resourceType);
                        writer = StartSendResourceResponse(resource, ResponseType.DownloadEnded);
                        writer.Write(0);
                        EndSend(writer);
                        Logger.LogInfo($"Cached {resource}!");
                    } catch (Exception e) {
                        Logger.LogError($"Failed to cache {resource} ({location})");
                        Logger.LogError(e);
                        writer = StartSendResourceResponse(resource, ResponseType.DownloadFailed);
                        if (e is CacheRequestException exception)
                            writer.WritePacked((uint) exception.Code);
                        else
                            writer.WritePacked(0x69420);

                        EndSend(writer);
                    }

                    break;
                }
                case PolusRootPackets.Intro: {
                    RoleData.IntroName = reader.ReadString();
                    RoleData.IntroDesc = reader.ReadString();
                    RoleData.IntroColor = new Color32(reader.ReadByte(), reader.ReadByte(), reader.ReadByte(),
                        reader.ReadByte());
                    RoleData.IntroPlayers = Enumerable.Repeat(15, reader.Length - reader.Position)
                        .Select(_ => reader.ReadByte()).ToList();
                    //todo finish this and outro
                    break;
                }
                case PolusRootPackets.EndGame: {
                    RoleData.OutroName = reader.ReadString();
                    RoleData.OutroDesc = reader.ReadString();
                    RoleData.OutroColor = new Color32(reader.ReadByte(), reader.ReadByte(), reader.ReadByte(),
                        reader.ReadByte());
                    RoleData.OutroPlayers = Enumerable.Repeat(15, reader.Length - reader.Position - 2)
                        .Select(_ => new WinningPlayerData(GameData.Instance.GetPlayerById(reader.ReadByte())))
                        .ToList();
                    if (RoleData.OutroPlayers.Count == 1) {
                        RoleData.OutroPlayers[0].IsYou = true;
                    }
                    RoleData.ShowQuit = reader.ReadBoolean();
                    RoleData.ShowPlayAgain = reader.ReadBoolean();

                    // test go directly to endgame
                    // SceneManager.LoadScene("EndGame");
                    break;
                }
                case PolusRootPackets.SetString: {
                    string text = reader.ReadString();
                    StringLocations stringLocation = (StringLocations) reader.ReadByte();
                    switch (stringLocation) {
                        case StringLocations.GameCode: {
                            GameStartManager.Instance.GameRoomName.text = text;
                            break;
                        }
                        case StringLocations.GamePlayerCount: {
                            GameStartManager.Instance.PlayerCounter.text = text;
                            break;
                        }
                        case StringLocations.PingTracker: {
                            PingTrackerTextPatch.PingText = text == "__unset" ? null : text;
                            break;
                        }
                        case StringLocations.RoomTracker: {
                            RoomTrackerTextPatch.RoomText = text == "__unset" ? null : text;
                            break;
                        }
                        case StringLocations.TaskCompletion: {
                            HudManager.Instance.TaskCompleteOverlay.GetComponent<TextMeshPro>().text = text;
                            break;
                        }
                        case StringLocations.TaskText: {
                            if (PlayerControl.LocalPlayer) {
                                ImportantTextTask importantTextTask =
                                    new GameObject("_Player").AddComponent<ImportantTextTask>();
                                importantTextTask.Text = text;
                                importantTextTask.transform.SetParent(PlayerControl.LocalPlayer.transform, false);
                            }

                            break;
                        }
                    }

                    break;
                }
                case PolusRootPackets.DeclareHat: {
                    //new hat
                    HatBehaviour hat = ScriptableObject.CreateInstance<HatBehaviour>();
                    uint id = reader.ReadPackedUInt32();
                    hat.MainImage = Cache.CachedFiles[reader.ReadPackedUInt32()].Get<Sprite>();
                    uint back = reader.ReadPackedUInt32();
                    hat.ClimbImage = Cache.CachedFiles[reader.ReadPackedUInt32()].Get<Sprite>();
                    hat.FloorImage = Cache.CachedFiles[reader.ReadPackedUInt32()].Get<Sprite>();
                    hat.AltShader = Cache.CachedFiles[reader.ReadPackedUInt32()].Get<Material>();
                    hat.ChipOffset = reader.ReadVector2();
                    hat.NoBounce = !reader.ReadBoolean();
                    // ReSharper disable once AssignmentInConditionalExpression (resharper can whine and cry at my superior intellect
                    if (hat.InFront = !reader.ReadBoolean()) Cache.CachedFiles[back].Get<Sprite>();

                    if (reader.ReadBoolean()) {
                        hat.NotInStore = true;
                        hat.Free = false;
                    }

                    HatManager.Instance.AllHats[(Index) (int) id] = hat;
                    break;
                }
                case PolusRootPackets.DisplaySystemAnnouncement: {
                    MaintenanceBehaviour.Instance.ShowToast(reader.ReadString());
                    break;
                }
                case PolusRootPackets.SetGameOption: {
                    ushort sequenceId = reader.ReadUInt16();
                    GameOptionsPatches.OnEnablePatch.ReceivedGameOptionPacket(new GameOptionsPatches.GameOptionPacket {
                        Type = OptionPacketType.SetOption,
                        Reader = reader.ReadBytes(reader.BytesRemaining),
                        SequenceId = sequenceId
                    });

                    break;
                }
                case PolusRootPackets.DeleteGameOption: {
                    ushort sequenceId = reader.ReadUInt16();
                    GameOptionsPatches.OnEnablePatch.ReceivedGameOptionPacket(new GameOptionsPatches.GameOptionPacket {
                        Type = OptionPacketType.DeleteOption,
                        Reader = reader.ReadBytes(reader.BytesRemaining),
                        SequenceId = sequenceId
                    });
                    break;
                }
                case PolusRootPackets.LoadHat: {
                    HatManager.Instance.AllHats.Insert((int) reader.ReadPackedUInt32(),
                        Cache.CachedFiles[reader.ReadPackedUInt32()].Get<HatBehaviour>());
                    break;
                }
                default: {
                    Logger.LogError($"Invalid packet with id {reader.Tag}");
                    break;
                }
            }
        }

        public void DirtyOptions() {
            optionsDirty = true;
        }

        public override void FixedUpdate() {
            if (MeetingHud.Instance) {
                PlayerControl.LocalPlayer.SetThickAssAndBigDumpy(true, true);
            }
        }

        public override void Update() {
            if (optionsDirty) {
                GameSettingMenu menu = Object.FindObjectOfType<GameSettingMenu>();
                if (menu) menu.OnEnable();
                if (LobbyBehaviour.Instance)
                    GameOptionsPatches.UpdateHudString();
                optionsDirty = false;
            }
        }

        public override void LobbyJoined() {
            Logger.LogInfo("Joined Lobby!");
            new GameObject().AddComponent<MaintenanceBehaviour>();
        }

        public override void LobbyLeft() {
            PingTrackerTextPatch.PingText = null;
            RoomTrackerTextPatch.RoomText = null;
            GameOptionsPatches.OnEnablePatch.Reset();
        }

        public override void PlayerSpawned(PlayerControl player) {
            
        }

        public override void PlayerDestroyed(PlayerControl player) {
            
        }

        public override void GameEnded() {
            
        }

        private MessageWriter StartSendResourceResponse(uint resource, ResponseType type) {
            Logger.LogInfo($"Starting response with type {type}");
            MessageWriter writer = MessageWriter.Get(SendOption.Reliable);
            writer.StartMessage((byte) PolusRootPackets.FetchResource);
            writer.WritePacked(resource);
            writer.Write((byte) type);
            return writer;
        }

        public static void EndSend(MessageWriter writer) {
            writer.EndMessage();
            AmongUsClient.Instance.SendOrDisconnect(writer);
            writer.Recycle();
        }

        public void SetPlayerAppearance(PlayerControl player, PlayerControl corpse) {
            if (player.name.Contains("Ludwig", StringComparison.InvariantCultureIgnoreCase))
                corpse.SetThickAssAndBigDumpy(true, true);
        }
    }

    public class RoleData {
        public Color IntroColor = Color.magenta;
        public string IntroDesc = "Something went horribly wrong\nwhile displaying this intro!";
        public string IntroName = "uh oh";
        public List<byte> IntroPlayers = new();
        public Color OutroColor = Color.green;
        public string OutroDesc = "Failed to set ending correctly!";
        public string OutroName = "Error!";
        public List<WinningPlayerData> OutroPlayers = new();
        public bool ShowPlayAgain;
        public bool ShowQuit = true;
    }

    public static class Lol {
        public static void SetThickAssAndBigDumpy(this PlayerControl playerControl, bool isThick, bool hasBigDumpy) {
            //stub
        }
    }
}