using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using BepInEx.Logging;
using HarmonyLib;
using Hazel;
using InnerNet;
using Polus.Behaviours;
using Polus.Behaviours.Inner;
using Polus.Enums;
using Polus.Extensions;
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
using UnityEngine;
using DiscordPatches = Polus.Patches.Permanent.DiscordPatches;
using Object = UnityEngine.Object;

namespace Polus {
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
        private bool lobbyExisted;
        public static PolusMod Instance;
        private GameObject maintnet;
        private static CoroutineManager coMan;
        public static List<Action> Dispatcher = new();
        private static List<Action> TempQueue = new();
        public bool CannotMove;

        public override string Name => "PolusMod";

        public override ManualLogSource Logger {
            get => _loggee;
            set => _loggee = value;
        }

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
                    animator.hatColor.a = alpha;
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
                    netObject.gameObject.EnsureComponent<PlayerAnimPlayer>().HandleMessage(reader);
                    break;
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
                    int hatId = (int) reader.ReadPackedUInt32();
                    uint resourceId = reader.ReadPackedUInt32();
                    bool isFree = reader.ReadBoolean();

                    AddDispatch(() => {
                        HatBehaviour hat = Cache.CachedFiles[resourceId].Get<HatBehaviour>();
                        CosmeticManager.Instance.SetHat((uint) hatId, hat);
                        if (isFree) hat.LimitedMonth = 0;
                        if (hat.AltShader != null) hat.AltShader = PlayerControl.LocalPlayer.myRend.material;
                        RefreshCpmTab();
                    });

                    break;
                }
                case PolusRootPackets.LoadPet: {
                    int petId = (int) reader.ReadPackedUInt32();
                    uint resourceId = reader.ReadPackedUInt32();
                    bool isFree = reader.ReadBoolean();

                    AddDispatch(() => {
                        PetBehaviour pet = Cache.CachedFiles[resourceId].Get<GameObject>().GetComponent<PetBehaviour>();
                        CosmeticManager.Instance.SetPet((uint) petId, pet);
                        pet.Free = isFree;
                        RefreshCpmTab();
                    });

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
                            HudShowMapPatch.sabotagesEnabled = enabled;
                            break;
                        }
                        case HudItem.MapDoorButtons: {
                            HudShowMapPatch.doorsEnabled = enabled;
                            break;
                        }
                        case HudItem.SabotageButton: {
                            UseButtonTargetPatch.sabotageButtonEnabled = enabled;
                            break;
                        }
                        case HudItem.VentButton: {
                            UseButtonTargetPatch.ventButtonEnabled = enabled;
                            break;
                        }
                        case HudItem.UseButton: {
                            UseButtonTargetPatch.useButtonEnabled = enabled;
                            break;
                        }
                        case HudItem.TaskListPopup: {
                            TaskPanelUpdatePatch.enabled = enabled;
                            break;
                        }
                        case HudItem.TaskProgressBar: {
                            ProgressTrackerUpdatePatch.enabled = enabled;
                            break;
                        }
                        case HudItem.ReportButton: {
                            ReportButtonDisablePatch.enabled = enabled;
                            break;
                        }
                        case HudItem.CallMeetingButton: {
                            UseButtonTargetPatch.meetingButtonEnabled = enabled;
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
                    Debug.Log($"Redirected to: {AmongUsClient.Instance.networkAddress}:{AmongUsClient.Instance.networkPort}");
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
                    $"Got QR Code {StupidModStampPatches.QrVisible} {StupidModStampPatches.QrActuallyVisible}".Log(10);
                    if (StupidModStampPatches.QrActuallyVisible) {
                        string data = reader.ReadString();
                    
                        AddDispatch(() => {
                            QRCodeGenerator qrGenerator = new();
                            QRCodeData qrCodeData = qrGenerator.CreateQrCode(data.Log(comment: "perhaps"), QRCodeGenerator.ECCLevel.L, true, false, QRCodeGenerator.EciMode.Default, -1);
                            UnityQRCode qrCode = new(qrCodeData);
                            Texture2D qrCodeAsTexture2D = qrCode.GetGraphic(2, Color.black, new Color(1f, 1f, 1f, 0.5f)).DontDestroy();
                            qr.SetCode(qrCodeAsTexture2D);
                        });
                    }

                    break;
                }
                default: {
                    Logger.LogError($"Invalid packet with id {reader.Tag}");
                    break;
                }
            }
        }

        public void RefreshCpmTab() {
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
            if (PlayerControl.LocalPlayer) {
                if (CannotMove != !PlayerControl.LocalPlayer.CanMove) {
                    CannotMove = !PlayerControl.LocalPlayer.CanMove;
                    CannotMove.Log(comment: "Cannot move changed !!!!");
                    PolusClickBehaviour.SetLock(ButtonLocks.PlayerCanMove, CannotMove);
                }
            } else {
                CannotMove = false;
                PolusClickBehaviour.SetLock(ButtonLocks.PlayerCanMove, CannotMove);
            }

            lock (Dispatcher) {
                if (AmongUsClient.Instance && AmongUsClient.Instance.InOnlineScene) {
                    TempQueue.AddRange(Dispatcher);
                    Dispatcher.Clear();
                }

                if (AmongUsClient.Instance && !AmongUsClient.Instance.AmConnected) {
                    Dispatcher.Clear();
                }
            }

            foreach (Action t in TempQueue) {
                CatchHelper.TryCatch(() => t());
            }

            TempQueue.Clear();
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
            //todo write code that calls this
            WhyDidntHazelHavePacketOrderingPatch.Reset();
        }

        public override void ConnectionDestroyed() {
            //todo write code that calls this
        }

        [HarmonyPatch(typeof(InnerNetClient), nameof(InnerNetClient.SendClientReady))]
        public class ReadyUp {
            [PermanentPatch]
            [HarmonyPrefix]
            public static void Ready() {
                "Ready Called (this logs three times, please tell sanae if this calls more than that for whatever stupid reason)".Log(3);
            }
        }

        public override void LobbyJoined() {
            Logger.LogInfo("Joined Lobby!");
            maintnet = new GameObject("maintent").DontDestroy();
            maintnet.AddComponent<MaintenanceBehaviour>();
            PingTrackerTextPatch.PingText = null;
            RoomTrackerTextPatch.RoomText = null;
            StupidModStampPatches.Reset();
            ResizeHandlerPatch.SetResolution(Screen.width, Screen.height);
            AmongUsClient.Instance.mode = MatchMakerModes.Client;
        }

        public override void LobbyLeft() {
            Object.Destroy(maintnet);
            maintnet = null;
            PingTrackerTextPatch.PingText = null;
            RoomTrackerTextPatch.RoomText = null;
            GameOptionsPatches.OnEnablePatch.Reset();
            StupidModStampPatches.Reset();
            CosmeticManager.Instance.Reset();
        }

        public override void PlayerSpawned(PlayerControl player) { }

        public override void PlayerDestroyed(PlayerControl player) { }

        public override void BecameHost() {
            optionsDirty = true;
        }

        public override void LostHost() {
            optionsDirty = true;
        }

        public override void GameEnded() {
            StupidModStampPatches.Reset();
        }

        private IEnumerator FetchResource(MessageReader reader) {
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
                    writer = StartSendResourceResponse(resource, ResponseType.DownloadFailed);
                    if (e is CacheRequestException exception)
                        writer.WritePacked((uint) exception.Code);
                    else
                        writer.WritePacked(0x69420);

                    EndSend(writer);
                }

                yield return null;
            }

            writer = StartSendResourceResponse(resource, ResponseType.DownloadEnded);
            writer.Write(addToCache.Current.Cached);
            EndSend(writer);
            Logger.LogInfo($"Cached {resource}!");
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
        public static void SetThickAssAndBigDumpy(this PlayerControl playerControl, bool isThick, bool hasBigDumpy) {
            //stub
            // "uwu kissies mwah! @Sanae#4092 on discord!!!".Log(level: LogLevel.Debug);
        }
    }
}