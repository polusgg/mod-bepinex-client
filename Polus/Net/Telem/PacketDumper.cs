using System;
using Hazel;
using UnhollowerRuntimeLib;
using UnityEngine;

namespace Polus.Net.Telem {
    public class PacketDumper : MonoBehaviour {
        static PacketDumper() {
            ClassInjector.RegisterTypeInIl2Cpp<PacketDumper>();
        }

        private static PacketDumper _instance;
        public static PacketDumper Instance {
            get {
                if (_instance) return _instance;
                return _instance = new GameObject("hey cuties uwu how are youwu").AddComponent<PacketDumper>();
            }
        }
        private void Start() {
        }

        public uint time;

        public PacketDumper(IntPtr ptr) : base(ptr) { }
        public void Connected() { }
        public void Disconnected() { }
        public void WriteInputPacket(MessageReader reader) { }
        public void WriteOutputPacket(MessageWriter writer) { }
    }
}