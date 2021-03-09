using System;
using BepInEx.Logging;
using Hazel;
using PolusApi;
using PolusApi.Net;

namespace TestMod {
    [Mod(Id, "1.0.0", "Sanae6")]
    public class TestPggMod : Mod {
        public const string Id = "TestMod Lmoa";
        private static ManualLogSource Logger;
        public override void Load(ManualLogSource logger, IObjectManager objectManager) {
            Logger = logger;
            System.Console.WriteLine($"PogU {logger == null}");
            Logger.LogInfo($"Yo mama from {Id} {objectManager == null}");
            // IObjectManager.Instance = objectManager;
            objectManager.InnerRpcReceived += OnInnerRpcReceived;
        }

        public override void Unload() {
            Logger.LogInfo($"Your mother from {Id}");
        }

        private void OnInnerRpcReceived(object sender, RpcEventArgs e) {
            InnerNetObject netObject = (InnerNetObject) sender;
            
            Logger.LogInfo($"Handle custom rpc from {e.callId}");

            if (netObject is LobbyBehaviour && e.callId == 0x8b) {
                GameStartManager.Instance.GameRoomName.Text = e.reader.ReadString();
                return;
            }

            if (netObject is PlayerControl) {
                switch (e.callId) {
                    case 0x8c:
                        
                        break;
                }
            }
        }

        public override void HandleRoot(MessageReader reader) {
            switch (reader.Tag) {
                case 0x80:
                    
                case 0x81:
                    break;
            }
        }

        public override string Name => "TestMod";
    }
}