using System;
using Hazel;
using PolusGG.Net;
using UnhollowerRuntimeLib;
using UnityEngine;

namespace PolusGG.Inner {
    public class PolusVent : PnoBehaviour {
        private Vent vent;
        public PolusVent(IntPtr ptr) : base(ptr) { }

        static PolusVent() {
            ClassInjector.RegisterTypeInIl2Cpp<PolusVent>();
        }

        private void Start() {
            pno = IObjectManager.Instance.LocateNetObject(this);
            pno.OnData = Deserialize;
        }
        
        private void FixedUpdate() {
            if (pno.HasSpawnData()) Deserialize(pno.GetSpawnData());
        }

        private void Deserialize(MessageReader reader) {
            vent.Id = reader.ReadPackedInt32();
        }
    }
}