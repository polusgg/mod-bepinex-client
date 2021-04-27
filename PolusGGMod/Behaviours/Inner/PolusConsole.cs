using System;
using Hazel;
using PolusGG.Net;
using UnhollowerRuntimeLib;

namespace PolusGG.Behaviours.Inner {
    public class PolusConsole : PnoBehaviour {
        static PolusConsole() {
            ClassInjector.RegisterTypeInIl2Cpp<PolusConsole>();
        }

        // private float timer;
        public PolusConsole(IntPtr ptr) : base(ptr) { }

        private void Start() {
            pno = new PggObjectManager().LocateNetObject(this);
            pno.OnData = Deserialize;
        }

        private void Update() { }

        private void FixedUpdate() {
            if (pno.HasSpawnData()) Deserialize(pno.GetSpawnData());
        }

        private void Deserialize(MessageReader reader) { }
    }
}