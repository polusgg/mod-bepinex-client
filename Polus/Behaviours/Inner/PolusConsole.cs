using System;
using Hazel;
using Polus.Net.Objects;
using UnhollowerRuntimeLib;

namespace Polus.Behaviours.Inner {
    public class PolusConsole : PnoBehaviour {
        static PolusConsole() {
            ClassInjector.RegisterTypeInIl2Cpp<PolusConsole>();
        }

        // private float timer;
        public PolusConsole(IntPtr ptr) : base(ptr) { }

        private void Start() {
            pno = PogusPlugin.ObjectManager.LocateNetObject(this);
        }

        private void Update() {
            if (pno != null && pno.HasData()) Deserialize(pno.GetSpawnData());
        }

        private void Deserialize(MessageReader reader) { }
    }
}