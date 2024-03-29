﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using BepInEx.Logging;
using Polus.Extensions;
using HarmonyLib;
using Hazel;
using InnerNet;
using Polus.Behaviours;
using Polus.Behaviours.Inner;
using Polus.Enums;
using Polus.Mods;
using Polus.Mods.Patching;
using Polus.Patches.Permanent;
using Polus.Patches.Temporary;
using Polus.Resources;
using Polus.Utils;
using PowerTools;
using QRCoder;
using QRCoder.Unity;
using TMPro;
using UnhollowerBaseLib;
using UnhollowerRuntimeLib;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;
using UnityQRCode = QRCoder.Unity.UnityQRCode;

namespace Polus {
    [Mod(Id, "1.0.0", "Sanae6")]
    public class PolusMod : Mod {
        private const string Id = "PolusMain";
        public static RoleData RoleData = new();
        private static readonly int OutlineColor = Shader.PropertyToID("_OutlineColor");
        private static readonly int Outline = Shader.PropertyToID("_Outline");
        private ICache Cache;
        private bool loaded;
        private bool optionsDirty;
        private bool lobbyExisted;
        public static PolusMod Instance;
        private MaintenanceBehaviour maintenance;
        private static CoroutineManager coMan;
        public static List<Action> Dispatcher = new();
        private static List<Action> TempQueue = new();
        public bool GameCodeHidden;
        private readonly QRCodeGenerator qrGenerator = new();
        private readonly object fetchLock = new();

        public override string Name => "PolusMod";
        public override byte? ProtocolId => 0; //first mod baybee

        public ManualLogSource Logger = BepInEx.Logging.Logger.CreateLogSource("Polus");

        public override void Start() {
            Instance = this;
            if (loaded) return;

            loaded = true;

            if (!coMan) coMan = new GameObject("PMC").DontDestroy().AddComponent<CoroutineManager>();

            PogusPlugin.ObjectManager.InnerRpcReceived += OnInnerRpcReceived;
            PogusPlugin.ObjectManager.Register(0x80, RegisterPnos.CreateImage(PogusPlugin.ObjectManager));
            PogusPlugin.ObjectManager.Register(0x81, RegisterPnos.CreateButton(PogusPlugin.ObjectManager));
            PogusPlugin.ObjectManager.Register(0x83, RegisterPnos.CreateDeadBodyPrefab(PogusPlugin.ObjectManager));
            PogusPlugin.ObjectManager.Register(0x85, RegisterPnos.CreateSoundSource(PogusPlugin.ObjectManager));
            PogusPlugin.ObjectManager.Register(0x87, RegisterPnos.CreatePoi(PogusPlugin.ObjectManager));
            PogusPlugin.ObjectManager.Register(0x88, RegisterPnos.CreateCameraController(PogusPlugin.ObjectManager));
            PogusPlugin.ObjectManager.Register(0x89, RegisterPnos.CreatePrefabHandle(PogusPlugin.ObjectManager));

            Cache = PogusPlugin.Cache;

            ModManager.Instance.ShowModStamp();
        }

        public override void Stop() { }

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
                        if (PlayerControl.LocalPlayer.Data.IsImpostor) {
                            playerControl.Data.Object.nameText.color = Palette.ImpostorRed;
                        }
                    } else if (playerControl == PlayerControl.LocalPlayer) {
                        foreach (GameData.PlayerInfo player in GameData.Instance.AllPlayers)
                            player.Object.nameText.color = Palette.White;
                    }

                    foreach (GameData.PlayerInfo player in GameData.Instance.AllPlayers) {
                        // $"{player.Object.name} - {player.IsImpostor}".Log();
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
                    PlayerAnimPlayer animator = netObject.gameObject.EnsureComponent<PlayerAnimPlayer>();
                    float alpha = reader.ReadByte() / 255.0f;
                    animator.playerColor.a = alpha;
                    animator.hatOpacity = alpha;
                    animator.petColor.a = alpha;
                    animator.skinColor.a = alpha;
                    animator.nameAlpha = alpha;
                    break;
                }
                case PolusRpcCalls.SetOutline: {
                    PlayerControl control = netObject.Cast<PlayerControl>();
                    control.myRend.material.SetFloat(Outline, reader.ReadByte());
                    control.myRend.material.SetColor(OutlineColor, reader.ReadColor());
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
                    netObject.gameObject.EnsureComponent<PlayerAnimPlayer>().BeginAnimation(reader);
                    break;
                // case PolusRpcCalls.BeginAnimationPlayer:
                //     netObject.gameObject.EnsureComponent<PlayerAnimPlayer>().BeginAnimation(reader);
                //     break;
                case PolusRpcCalls.SetAliveState: {
                    PlayerControl player = netObject.Cast<PlayerControl>();
                    SetAliveState(player, reader.ReadBoolean());
                    break;
                }
                case PolusRpcCalls.DisplayKillAnimation: {
                    PlayerControl killer = GameData.Instance.GetPlayerById(reader.ReadByte()).Object;
                    PlayerControl target = GameData.Instance.GetPlayerById(reader.ReadByte()).Object;
                    Vector2 targetPosition = reader.ReadVector2();
                    bool killOverlayEnabled = reader.ReadBoolean();
                    killer.MyPhysics.StartCoroutine(DisplayKillAnimation(killer, target, targetPosition, killOverlayEnabled));
                    break;
                }
                case PolusRpcCalls.SetSpeedModifier: {
                    PlayerControl player = netObject.Cast<PlayerControl>();

                    player.gameObject.EnsureComponent<IndividualModifierManager>().SpeedModifer = reader.ReadSingle();
                    break;
                }
                case PolusRpcCalls.SetVisionModifier: {
                    PlayerControl player = netObject.Cast<PlayerControl>();

                    player.gameObject.EnsureComponent<IndividualModifierManager>().VisionModifier = reader.ReadSingle();
                    break;
                }
                case PolusRpcCalls.SetRemainingEmergencies: {
                    PlayerControl player = netObject.Cast<PlayerControl>();

                    player.RemainingEmergencies = reader.ReadInt32();
                    break;
                }
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public override void RootPacketReceived(MessageReader reader) {
            // Logger.LogInfo($"LOL {reader.Tag}");
            switch ((PolusRootPackets) reader.Tag) {
                case PolusRootPackets.UpdateDiscordRichPresence: {
                    // AddDispatch(() => DiscordPatches.ClearPresence(() => DiscordPatches.UpdateRichPresence(reader)));
                    break;
                }
                case PolusRootPackets.FetchResource: {
                    StartCoroutine(FetchResource(reader));
                    break;
                }
                case PolusRootPackets.Intro: {
                    RoleData.IntroName = reader.ReadString();
                    RoleData.IntroDesc = reader.ReadString();
                    RoleData.IntroColor = reader.ReadColor();
                    RoleData.IntroPlayers = Enumerable.Repeat(15, reader.Length - reader.Position)
                        .Select(_ => reader.ReadByte()).ToList();
                    StupidModStampPatches.TextColor ??= RoleData.IntroColor;
                    break;
                }
                case PolusRootPackets.EndGame: {
                    RoleData.OutroName = reader.ReadString();
                    RoleData.OutroDesc = reader.ReadString();
                    RoleData.OutroColor = reader.ReadColor();
                    RoleData.OutroPlayers = Enumerable.Repeat(15, reader.ReadByte())
                        .Select(_ => new WinningPlayerData(GameData.Instance.GetPlayerById(reader.ReadByte())))
                        .ToList();
                    RoleData.ShowQuit = reader.ReadBoolean();
                    RoleData.ShowPlayAgain = reader.ReadBoolean();
                    RoleData.WinSound = (WinSounds) reader.ReadByte();
                    if (RoleData.WinSound == WinSounds.CustomSound) RoleData.WinSoundCustom = reader.ReadPackedUInt32();

                    // test go directly to endgame
                    // SceneManager.LoadScene("EndGame");
                    break;
                }
                case PolusRootPackets.SetString: {
                    string text = reader.ReadString();
                    StringLocations stringLocation = (StringLocations) reader.ReadByte();
                    AddDispatch(() => {
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
                                HudUpdatePatch.TaskText = text == "__unset" ? null : text;
                                break;
                            }
                            case StringLocations.MeetingButtonHudText: {
                                EmergencyTextPatch.EmergencyText = text == "__unset" ? null : text;
                                break;
                            }
                        }
                    });

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
                    maintenance.ShowToast(reader.ReadString());
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
                    int hatId = (int) reader.ReadPackedUInt32();
                    uint resourceId = reader.ReadPackedUInt32();
                    bool isFree = reader.ReadBoolean();

                    HatBehaviour hat = Cache.CachedFiles[resourceId].Get<HatBehaviour>();
                    $"Loading hat {hat.name} at id {hatId}".Log();
                    CosmeticManager.Instance.SetHat((uint) hatId, hat, isFree);
                    if (hat.AltShader != null) hat.AltShader = AmongUsClient.Instance.PlayerPrefab.myRend.material;
                    RefreshCpmTab();
                    break;
                }
                case PolusRootPackets.LoadPet: {
                    int petId = (int) reader.ReadPackedUInt32();
                    uint resourceId = reader.ReadPackedUInt32();
                    bool isFree = reader.ReadBoolean();

                    PetBehaviour pet = Cache.CachedFiles[resourceId].Get<GameObject>().GetComponent<PetBehaviour>();
                    $"Loading hat {pet.name} at id {petId}".Log();
                    CosmeticManager.Instance.SetPet((uint) petId, pet, isFree);
                    RefreshCpmTab();
                    break;
                }
                case PolusRootPackets.SetHudVisibility: {
                    HudItem item = (HudItem) reader.ReadByte();
                    bool enabled = reader.ReadBoolean();

                    switch (item) {
                        case HudItem.MapButton: {
                            HudManager.Instance.MapButton.gameObject.SetActive(enabled);
                            break;
                        }
                        case HudItem.MapSabotageButtons: {
                            SetHudVisibilityPatches.HudShowMapPatch.SabotagesEnabled = enabled;
                            break;
                        }
                        case HudItem.MapDoorButtons: {
                            SetHudVisibilityPatches.HudShowMapPatch.DoorsEnabled = enabled;
                            break;
                        }
                        case HudItem.SabotageButton: {
                            SetHudVisibilityPatches.SabotageButtonEnabled = enabled;
                            break;
                        }
                        case HudItem.VentButton: {
                            SetHudVisibilityPatches.VentButtonEnabled = enabled;
                            break;
                        }
                        case HudItem.UseButton: {
                            SetHudVisibilityPatches.UseButtonEnabled = enabled;
                            break;
                        }
                        case HudItem.TaskListPopup: {
                            SetHudVisibilityPatches.TaskPanelUpdatePatch.Enabled = enabled;
                            break;
                        }
                        case HudItem.TaskProgressBar: {
                            SetHudVisibilityPatches.ProgressTrackerUpdatePatch.Enabled = enabled;
                            break;
                        }
                        case HudItem.ReportButton: {
                            SetHudVisibilityPatches.ReportButtonDisablePatch.Enabled = enabled;
                            break;
                        }
                        case HudItem.CallMeetingButton: {
                            SetHudVisibilityPatches.MeetingButtonEnabled = enabled;
                            break;
                        }
                        case HudItem.AdminTable: {
                            SetHudVisibilityPatches.AdminTableEnabled = enabled;
                            break;
                        }
                        case HudItem.GameCode: {
                            GameCodeHidden = enabled;
                            break;
                        }
                        default:
                            throw new ArgumentOutOfRangeException($"Invalid hud item sent: {item}");
                    }

                    break;
                }
                case PolusRootPackets.AllowTaskInteraction: {
                    AllowTaskInteractionPatch.TaskInteractionAllowed = reader.ReadBoolean();
                    break;
                }
                case PolusRootPackets.MarkAssBrown: {
                    uint address = reader.ReadUInt32();
                    ushort port = reader.ReadUInt16();
                    AmongUsClient.Instance.SetEndpoint(InnerNetClient.AddressToString(address), port);
                    $"Redirected to: {AmongUsClient.Instance.networkAddress}:{AmongUsClient.Instance.networkPort}".Log();
                    AmongUsClient.Instance.Connect(AmongUsClient.Instance.mode);
                    ServerMigrationPatches.HostGamePacketChange.Migrated = true;
                    break;
                }
                case PolusRootPackets.StampSetString: {
                    StupidModStampPatches.TextColor = reader.ReadColor();
                    StupidModStampPatches.Suffix = reader.ReadString();
                    break;
                }
                case PolusRootPackets.SetQrCodeContents: {
                    QRStamp qr = StupidModStampPatches.qr;
                    qr.Start();
                    StupidModStampPatches.QrVisible = StupidModStampPatches.QrActuallyVisible = reader.ReadBoolean();
                    // $"Got QR Code {StupidModStampPatches.QrVisible} {StupidModStampPatches.QrActuallyVisible}".Log(10);
                    if (StupidModStampPatches.QrActuallyVisible) {
                        string data = reader.ReadString();

                        AddDispatch(() => {
                            QRCodeData qrCodeData = qrGenerator.CreateQrCode(data, QRCodeGenerator.ECCLevel.M, true, false, QRCodeGenerator.EciMode.Default, -1);
                            UnityQRCode qrCode = new(qrCodeData);
                            Texture2D qrCodeAsTexture2D = qrCode.GetGraphic(2, Color.black, Color.white);
                            qr.SetCode(qrCodeAsTexture2D);
                        });
                    }

                    break;
                }
                case PolusRootPackets.SendMessage: {
                    ChatManager.Instance?.ReceiveChatMessage(reader);
                    break;
                }
                case PolusRootPackets.DeleteMessage:
                    Guid guid = reader.ReadGuid();
                    ChatManager.Instance.DeleteMessage(guid);
                    break;
                default: {
                    Logger.LogError($"Invalid packet with id {reader.Tag}");
                    break;
                }
            }

            reader.Recycle();
        }

        public static void RefreshCpmTab() {
            PlayerTab player = Object.FindObjectOfType<PlayerTab>();
            if (player != null) {
                player.OnDisable();
                player.OnEnable();
            }

            PetsTab pets = Object.FindObjectOfType<PetsTab>();
            if (pets != null) {
                pets.OnDisable();
                pets.OnEnable();
            }

            HatsTab hats = Object.FindObjectOfType<HatsTab>();
            if (hats != null) {
                hats.OnDisable();
                hats.OnEnable();
            }

            SkinsTab skins = Object.FindObjectOfType<SkinsTab>();
            if (skins != null) {
                skins.OnDisable();
                skins.OnEnable();
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
            if (GameStartManager.InstanceExists) {
                GameStartManager.Instance.GameRoomName.renderer.enabled = !GameCodeHidden;
                if (GameCodeHidden) {
                    GameStartManager.Instance.PlayerCounter.transform.localPosition = new Vector3(1.1135f, -0.96f, 0f);
                    GameStartManager.Instance.MakePublicButton.transform.localPosition = new Vector3(-0.75f, -0.95f, 0f);
                } else {
                    GameStartManager.Instance.PlayerCounter.transform.localPosition = new Vector3(1.627f, -0.96f, 0f);
                    GameStartManager.Instance.MakePublicButton.transform.localPosition = new Vector3(-1.5f, -0.95f, 0f);
                }
            }
            
            if (PlayerControl.LocalPlayer) {
                if (PolusClickBehaviour.GetLock(ButtonLocks.PlayerCanMove) == PlayerControl.LocalPlayer.CanMove)
                    PolusClickBehaviour.SetLock(ButtonLocks.PlayerCanMove, (!PlayerControl.LocalPlayer.CanMove).Log(comment: "Cannot move changed"));
            } else {
                PolusClickBehaviour.SetLock(ButtonLocks.PlayerCanMove, false);
            }

            if (!HudManager.InstanceExists) {
                PolusClickBehaviour.SetLock(ButtonLocks.SetHudActive, false);
            }

            if (maintenance && !maintenance.WasCollected && !maintenance.coroutineRunning && Input.GetKeyDown(KeyCode.F5)) {
                StupidModStampPatches.QrToggled = !StupidModStampPatches.QrToggled;
                StupidModStampPatches.QrVisible = StupidModStampPatches.qr.gameObject.active;
                maintenance.ShowToast(StupidModStampPatches.QrToggled ? "Showing QR code, press F5 to disable it again." : "Now hiding the QR code, press F5 to enable it.", 1f);
            }

            lock (Dispatcher) {
                if (AmongUsClient.Instance) {
                    if (AmongUsClient.Instance.AmConnected) TempQueue.AddRange(Dispatcher);
                    Dispatcher.Clear();
                }
            }

            if (AmongUsClient.Instance && AmongUsClient.Instance.AmConnected && AmongUsClient.Instance.connection.State == ConnectionState.Connected) {
                foreach (Action t in TempQueue) {
                    CatchHelper.TryCatch(() => t());
                }

                TempQueue.Clear();
            }

            if (LobbyBehaviour.Instance != lobbyExisted) {
                lobbyExisted = LobbyBehaviour.Instance;
                if (lobbyExisted) optionsDirty = true;
            }

            if (HudManager.InstanceExists) HudManager.Instance.GameSettings.gameObject.SetActive(LobbyBehaviour.Instance);
            if (!optionsDirty) return;
            GameSettingMenu menu = Object.FindObjectOfType<GameSettingMenu>();
            if (menu) menu.OnEnable();
            if (LobbyBehaviour.Instance)
                GameOptionsPatches.UpdateHudString();
            optionsDirty = false;
        }

        public override void ConnectionEstablished() {
            WhyDidntHazelHave.PacketOrderingPatch.Reset();
        }

        public override void ConnectionDestroyed() {
            //lol there is literally no shit here
        }

        public override void WriteExtraData(MessageWriter writer) {
            (DateTime date, int? packageVersion) = StereotypicalClientModderVersionShowerPatch.Ver;
            writer.Write(Constants.GetVersion(date.Year, date.Month, date.Day, packageVersion ?? 0));
            writer.Write(PogusPlugin.LauncherBuild);
            writer.WritePacked(Screen.width);
            writer.WritePacked(Screen.height);
            
            if (Constants.GetPlatformType() != Platforms.Android) writer.Write((byte) Constants.GetPlatformType());        //trollface
            else writer.Write((byte) HelloAnalytics.Platform.PlayStore); // todo: android lmao
            writer.Write(SystemInfo.operatingSystem);
            writer.WritePacked(SystemInfo.graphicsMemorySize);
            writer.WritePacked(SystemInfo.systemMemorySize);
        }

        [HarmonyPatch(typeof(InnerNetClient), nameof(InnerNetClient.SendClientReady))]
        public class ReadyUp {
            [PermanentPatch]
            [HarmonyPrefix]
            public static void Ready() {
                "Ready Called (tell sanae if this calls more than once in one game)".Log();
            }
        }

        public override void LobbyJoined() {
            Logger.LogInfo("Joined Lobby!");
            maintenance = new GameObject("maintent").DontDestroy().AddComponent<MaintenanceBehaviour>();
            PingTrackerTextPatch.PingText = null;
            RoomTrackerTextPatch.RoomText = null;
            StupidModStampPatches.Reset();
            AmongUsClient.Instance.mode = MatchMakerModes.Client;
        }

        public override void LobbyLeft() {
            Logger.LogInfo("Left Lobby!");
            Object.Destroy(maintenance.gameObject);
            maintenance = null;
            PolusClickBehaviour.Buttons.Clear();
            PingTrackerTextPatch.PingText = null;
            RoomTrackerTextPatch.RoomText = null;
            GameOptionsPatches.OnEnablePatch.Reset();
            StupidModStampPatches.Reset();
            StupidModStampPatches.QrToggled = false;
            CosmeticManager.Instance.Reset();
            GameCodeHidden = false;
        }

        public override void PlayerSpawned(PlayerControl player) {
            player.gameObject.EnsureComponent<PlayerAnimPlayer>();
            player.gameObject.AddComponent<CacheListenerBehaviour>().Initialize(new CacheListener((id, current, old) => {
                if (id >= CosmeticManager.CosmeticStartId && player.Data.HatId == id - 1) {
                    player.SetHat(id - 1, player.Data.ColorId);
                }

                if (id >= CosmeticManager.CosmeticStartId && player.Data.PetId == id - 1) {
                    player.SetPet(id - 1);
                }

                if (id >= CosmeticManager.CosmeticStartId && player.Data.SkinId == id - 1) {
                    player.SetSkin(id - 1);
                }
            }));
        }

        public override void PlayerDestroyed(PlayerControl player) {
            if (player == PlayerControl.LocalPlayer) {
                SearsPatches.Reset();
            }
        }

        public override void BecameHost() {
            optionsDirty = true;
        }

        public override void LostHost() {
            optionsDirty = true;
        }

        public override void GameEnded() {
            StupidModStampPatches.Reset();
            SetHudVisibilityPatches.Reset();
            PolusClickBehaviour.UnlockAll();
        }

        public override void SceneChanged(Scene scene) {
            if (scene.name == GameScenes.MMOnline) RemoveHostGameMenuPatches.ChangeCreateGame();
        }

        public override void DebugGui() {
            
        }

        private IEnumerator FetchResource(MessageReader reader) {
            lock (fetchLock) {
                uint resource = reader.ReadPackedUInt32();
                string location = reader.ReadString();
                byte[] hash = reader.ReadBytes(32);
                uint resourceType = reader.ReadByte();
                MessageWriter writer;
                // if (Cache.IsCachedAndValid(resource, hash)) {
                //     Logger.LogInfo($"{resource} is already cached");
                //     writer = StartSendResourceResponse(resource, ResponseType.DownloadEnded);
                //     writer.Write(1);
                //     EndSend(writer);
                //     return;
                // }

                Logger.LogInfo($"Trying to download and cache {resource} ({location})");
                IEnumerator<ICache.CacheAddResult> addToCache = Cache.AddToCache(resource, location, hash,
                    (ResourceType) resourceType);

                while (true) {
                    try {
                        if (!addToCache.MoveNext()) break;
                    } catch (Exception e) {
                        Logger.LogError($"Failed to cache {resource} ({location})");
                        Logger.LogError(e);
                        e.ReportException();
                        writer = StartSendResourceResponse(resource, ResponseType.DownloadFailed);
                        if (e is CacheRequestException exception)
                            writer.WritePacked((uint) exception.Code);
                        else
                            writer.WritePacked(0x69420);

                        EndSend(writer);
                    }

                    yield return null;
                }

                if (addToCache.Current.Cached == CacheResult.Invalid) {
                    writer = StartSendResourceResponse(resource, ResponseType.DownloadInvalid);
                    EndSend(writer);
                    Logger.LogInfo($"Invalid hash but still saved {resource}, telling the server about this");
                    yield break;
                }

                writer = StartSendResourceResponse(resource, ResponseType.DownloadEnded);
                writer.Write(addToCache.Current.Cached == CacheResult.Success);
                EndSend(writer);
                Logger.LogInfo($"Cached {resource}!");
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

        public static void EndSend(MessageWriter writer) {
            writer.EndMessage();
            AmongUsClient.Instance.SendOrDisconnect(writer);
            writer.Recycle();
        }

        public static void AddDispatch(Action action) {
            lock (Dispatcher) Dispatcher.Add(action);
        }

        public static IEnumerator StartCoroutine(IEnumerator coroutine) {
            return coMan.Start(coroutine);
        }

        public void SetPlayerAppearance(PlayerControl player, PlayerControl corpse) {
            if (player.name.Contains("Ludwig", StringComparison.InvariantCultureIgnoreCase))
                corpse.SetThickAssAndBigDumpy(true, true);
        }

        private void SetAliveState(PlayerControl player, bool alive) {
            if (alive) player.Revive();
            else {
                player.Die(DeathReason.Kill);
                ImportantTextTask importantTextTask = new GameObject("_Player").AddComponent<ImportantTextTask>();
                importantTextTask.transform.SetParent(player.transform, false);
                if (!PlayerControl.GameOptions.GhostsDoTasks) {
                    player.ClearTasks();
                    importantTextTask.Text = DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.GhostIgnoreTasks, new Il2CppReferenceArray<Il2CppSystem.Object>(0));
                } else {
                    importantTextTask.Text = DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.GhostDoTasks, new Il2CppReferenceArray<Il2CppSystem.Object>(0));
                }

                player.myTasks.Insert(0, importantTextTask);
            }
        }

        private IEnumerator DisplayKillAnimation(PlayerControl killer, PlayerControl target, Vector2 pos,
            bool killOverlayEnabled) {
            if (Constants.ShouldPlaySfx() && killer.AmOwner) {
                SoundManager.Instance.PlaySound(killer.KillSfx, false, 0.8f);
            }

            if (target.AmOwner) {
                if (Minigame.Instance) {
                    try {
                        Minigame.Instance.Close();
                        Minigame.Instance.Close();
                    } catch { }
                }

                if (killOverlayEnabled) {
                    DestroyableSingleton<HudManager>.Instance.KillOverlay.ShowKillAnimation(killer.Data, target.Data);
                    DestroyableSingleton<HudManager>.Instance.ShadowQuad.gameObject.SetActive(false);
                }

                target.nameText.GetComponent<MeshRenderer>().material.SetInt("_Mask", 0);
                target.RpcSetScanner(false);
            }

            FollowerCamera cam = Camera.main.GetComponent<FollowerCamera>();
            bool isParticipant = PlayerControl.LocalPlayer == killer || PlayerControl.LocalPlayer == target;
            PlayerPhysics sourcePhys = killer.MyPhysics;
            KillAnimation.SetMovement(killer, false);
            KillAnimation.SetMovement(target, false);
            //target.SetPlayerMaterialColors(deadBody.bloodSplatter);
            if (isParticipant) {
                cam.Locked = true;
                ConsoleJoystick.SetMode_Task();
                if (PlayerControl.LocalPlayer.AmOwner) {
                    PlayerControl.LocalPlayer.MyPhysics.inputHandler.enabled = true;
                }
            }

            SpriteAnim sourceAnim = killer.MyAnim;
            yield return new WaitForAnimationFinish(sourceAnim, killer.KillAnimations[0].BlurAnim);
            killer.NetTransform.SnapTo(pos);
            sourceAnim.Play(sourcePhys.IdleAnim, 1f);
            KillAnimation.SetMovement(killer, true);
            KillAnimation.SetMovement(target, true);
            cam.Locked = false;
        }
    }

    // used to test whether secureNew parsing works now, can be used in any other scenario which requires steam
    // [HarmonyPatch(typeof(SteamManager), nameof(SteamManager.Awake))]
    // public static class TempDisableSteam {
    //     [PermanentPatch]
    //     [HarmonyPrefix]
    //     public static bool Disable() => false;
    // }

    public class RoleData {
        public Color IntroColor = Color.white;
        public string IntroDesc = "Something went horribly wrong\nwhile displaying this intro!";
        public string IntroName = "☹";
        public List<byte> IntroPlayers = new();
        public Color OutroColor = Color.green;
        public string OutroDesc = "Failed to set ending correctly!";
        public string OutroName = "Error!";
        public WinSounds WinSound = WinSounds.Disconnect;
        public uint WinSoundCustom = 6942021;
        public List<WinningPlayerData> OutroPlayers = new();
        public bool ShowPlayAgain;
        public bool ShowQuit = true;
    }

    public static class IfLudwigThen {
        public static ulong thickCount;
        public static ulong dumpyCount;

        public static void SetThickAssAndBigDumpy(this PlayerControl playerControl, bool isThick, bool hasBigDumpy) {
            if (isThick) thickCount++;
            if (hasBigDumpy) dumpyCount++;
        }
    }
}