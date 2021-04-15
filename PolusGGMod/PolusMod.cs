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
using Exception = System.Exception;
using Object = UnityEngine.Object;
using StringComparison = System.StringComparison;

namespace PolusGG {
    [Mod(Id, "1.0.0", "Sanae6")]
    public class PolusMod : Mod {
        public const string Id = "PolusMain";
        public bool loaded;
        public ICache Cache;
        public static RoleData RoleData = new();

        public override void Load() {
            Logger.LogInfo("Loaded " + Id);
        }

        public override void Start(IObjectManager objectManager, ICache cache) {
            if (!loaded) {
                loaded = true;
            }
            objectManager.InnerRpcReceived += OnInnerRpcReceived;
            objectManager.Register(0x80, RegisterPnos.CreateImage());
            objectManager.Register(0x81, RegisterPnos.CreateButton());
            objectManager.Register(0x83, RegisterPnos.CreateDeadBodyPrefab());
            objectManager.Register(0x85, RegisterPnos.CreateSoundSource());
            objectManager.Register(0x87, RegisterPnos.CreatePoi());
            objectManager.Register(0x88, RegisterPnos.CreateCameraController());
            objectManager.Register(0x89, RegisterPnos.CreatePrefabHandle());
            // objectManager.Register(0x89, RegisterPnos.CreatePrefabHandle());

            
            ResolutionManagerPlus.Resolution();
            Cache = cache;
        }

        public override void Unload() { }

        private void OnInnerRpcReceived(object sender, RpcEventArgs e) {
            InnerNetObject netObject = (InnerNetObject) sender;
            PlayerControl playerControl;
            switch ((PolusRpcCalls) e.callId) {
                case PolusRpcCalls.ChatVisibility: {
                    var lol = e.reader.ReadByte() > 0;
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

                    if (e.reader.ReadBoolean()) {
                        if (playerControl == PlayerControl.LocalPlayer) {
                            DestroyableSingleton<HudManager>.Instance.KillButton.gameObject.SetActive(true);
                            playerControl.SetKillTimer(PlayerControl.GameOptions.KillCooldown);
                        }

                        if (PlayerControl.LocalPlayer.Data.IsImpostor) {
                            playerControl.Data.Object.nameText.color = Palette.ImpostorRed;
                        }
                    } else {
                        if (playerControl == PlayerControl.LocalPlayer) {
                            playerControl.SetKillTimer(21f);
                            HudManager.Instance.KillButton.gameObject.SetActive(false);
                            foreach (GameData.PlayerInfo player in GameData.Instance.AllPlayers) {
                                player.Object.nameText.color = Palette.White;
                            }
                        }

                        playerControl.Data.Object.nameText.color = Palette.White;
                    }
                    foreach (GameData.PlayerInfo player in GameData.Instance.AllPlayers) {
                        $"{player.Object.name} - {player.IsImpostor}".Log();
                        player.Object.nameText.color = player.IsImpostor && PlayerControl.LocalPlayer.Data.IsImpostor ? Palette.ImpostorRed : Palette.White;
                    }

                    break;
                }
                case PolusRpcCalls.Revive: {
                    netObject.Cast<PlayerControl>().Revive();
                    break;
                }
                case PolusRpcCalls.SetHat: {
                    Cache.CachedFiles[e.reader.ReadPackedUInt32()].Get<Sprite>();
                    Cache.CachedFiles[e.reader.ReadPackedUInt32()].Get<Sprite>();
                    Cache.CachedFiles[e.reader.ReadPackedUInt32()].Get<Sprite>();
                    break;
                }
                case PolusRpcCalls.SetOpacity: {
                    PlayerControl control = netObject.Cast<PlayerControl>();
                    var color = control.myRend.color;
                    color = new Color(color.r, color.g, color.b, e.reader.ReadByte() / 255f);
                    control.myRend.color = color;
                    control.MyPhysics.Skin.layer.color = new Color32(Byte.MaxValue, Byte.MaxValue, Byte.MaxValue, e.reader.ReadByte());
                    break;
                }
                case PolusRpcCalls.DespawnAllVents: {
                    IEnumerable<int> enumerable = Enumerable.Range(0, e.reader.ReadByte()).Select(_ => (int)e.reader.ReadByte());
                    IEnumerable<Vent> vents = Object.FindObjectsOfType<Vent>().Where(v => enumerable.Contains<int>(v.Id));
                    foreach (Vent vent in vents) {
                        Object.Destroy(vent);
                    }
                    break;
                }
                case PolusRpcCalls.BeginAnimationPlayer:
                    while (e.reader.Position < e.reader.Length) {
                        MessageReader message = e.reader.ReadMessage();
                        
                    }
                    break;
                default: {
                    throw new ArgumentOutOfRangeException();
                }
            }
        }

        public override void RootPacketReceived(MessageReader reader) {
            Logger.LogInfo($"LOL {reader.Tag}");
            switch ((PolusRootPackets) reader.Tag) {
                case PolusRootPackets.FetchResource: {
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
                        Logger.LogError($"Failed to cache {resource}");
                        Logger.LogError(e);
                        writer = StartSendResourceResponse(resource, ResponseType.DownloadFailed);
                        if (e is CacheRequestException exception) {
                            writer.WritePacked((uint) exception.Code);
                        } else {
                            writer.WritePacked(0x69420);
                        }

                        EndSendResourceResponse(writer);
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
                    RoleData.ShowQuit = reader.ReadBoolean();
                    RoleData.ShowPlayAgain = reader.ReadBoolean();

                    // test go directly to endgame
                    // SceneManager.LoadScene("EndGame");
                    break;
                }
                case PolusRootPackets.SetString: {
                    StringLocations stringLocation = (StringLocations) reader.ReadByte();
                    string text = reader.ReadString();
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
                            HudManager.Instance.TaskCompleteOverlay.GetComponent<TextRenderer>().Text = text;
                            break;
                        }
                        case StringLocations.TaskText: {
                            if (PlayerControl.LocalPlayer) {
                                ImportantTextTask importantTextTask = new GameObject("_Player").AddComponent<ImportantTextTask>();
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
                    if ((hat.InFront = !reader.ReadBoolean()) == true) {
                        Cache.CachedFiles[back].Get<Sprite>();
                    }

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
                case PolusRootPackets.SetGameOption:
                {

                    var name = reader.ReadString();
                    var optionType = (OptionType) reader.ReadByte();
                    
                    if (GameOptionsPatches.Options.TryGetValue(name, out GameOption option))
                    {
                        option.Value = optionType switch
                        {
                            OptionType.Boolean => new BooleanValue(reader.ReadBoolean()),
                            OptionType.Number => new FloatValue(reader.ReadSingle(), reader.ReadSingle(),
                                reader.ReadSingle(), reader.ReadSingle()),
                            OptionType.Enum => EnumValue.ConstructEnumValue(reader.ReadPackedUInt32(),
                                reader.ReadPackedUInt32(), reader),
                            _ => throw new ArgumentOutOfRangeException()
                        };
                    }
                    else
                    {
                        GameOptionsPatches.Options.Add(name, new GameOption
                        {
                            Type = optionType,
                            Value = optionType switch
                            {
                                OptionType.Boolean => new BooleanValue(reader.ReadBoolean()),
                                OptionType.Number => new FloatValue(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle()),
                                OptionType.Enum => EnumValue.ConstructEnumValue(reader.ReadPackedUInt32(), reader.ReadPackedUInt32(), reader),
                                _ => throw new ArgumentOutOfRangeException()
                            }
                        });
                    }
                    break;
                }
                case PolusRootPackets.DeleteGameOption:
                {
                    break;
                }
                default: {
                    Logger.LogError($"Invalid packet with id {reader.Tag}");
                    break;
                }
            }
        }

        public override void LobbyJoined() {
            Logger.LogInfo("Joined Lobby!");
            new GameObject().AddComponent<MaintenanceBehaviour>();
        }

        public override void LobbyLeft() {
            PingTrackerTextPatch.PingText = null;
            RoomTrackerTextPatch.RoomText = null;
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
        public string IntroName = "Poobscoob";
        public string IntroDesc = "you failed to set this on time";
        public Color IntroColor = Color.magenta;
        public List<byte> IntroPlayers = new();
        public string OutroName = "Error!";
        public string OutroDesc = "Failed to set ending correctly!";
        public Color OutroColor = Color.green;
        public List<WinningPlayerData> OutroPlayers = new();
        public bool ShowPlayAgain = false;
        public bool ShowQuit = true;
    }

    public static class Lol {
        public static void SetThickAssAndBigDumpy(this PlayerControl playerControl, bool isThick, bool hasBigDumpy) {
            //stub as fuck
        }
    }
}