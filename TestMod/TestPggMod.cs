using System;
using BepInEx.Logging;
using Hazel;
using PolusApi;
using PolusApi.Net;
using PolusApi.Resources;
using UnhollowerRuntimeLib;

namespace TestMod {
    [Mod(Id, "1.0.0", "Sanae6")]
    public class TestPggMod : Mod {
        public const string Id = "TestMod Lmoa";
        public bool loaded;
        public override void Load() {
            if (!loaded) {
                loaded = true;
            }
            Logger.LogInfo("Loaded " + Id);
        }

        public override void Start(IObjectManager objectManager, ICache cache) {
            objectManager.InnerRpcReceived += OnInnerRpcReceived;
            objectManager.Register(0x81, RegisterPnos.CreateDeadBodyPrefab());
        }

        public override void Unload() {
        }

        private void OnInnerRpcReceived(object sender, RpcEventArgs e) {
            InnerNetObject netObject = (InnerNetObject) sender;
            
            switch (e.callId) {
                case 0x8B:
                    GameStartManager.Instance.GameRoomName.Text = e.reader.ReadString();
                    break;
                case 0x8D:
                    var lol = e.reader.ReadByte();
                    Logger.LogInfo($"Got it lmoao {lol}");
                    HudManager.Instance.Chat.SetVisible(lol > 0);
                    break;
                case 0x89:
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
                    
                    break;
                case 0x81:
                    break;
            }
        }

        public override string Name => "TestMod";
        public ManualLogSource _loggee;

        public override ManualLogSource Logger {
            get => _loggee;
            set => _loggee = value;
        }
    }
}