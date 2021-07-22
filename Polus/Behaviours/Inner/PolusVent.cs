using System;
using Hazel;
using Polus.Net.Objects;
using UnhollowerRuntimeLib;

namespace Polus.Behaviours.Inner {
    public class PolusVent : PnoBehaviour {
        private Vent vent;

        static PolusVent() {
            ClassInjector.RegisterTypeInIl2Cpp<PolusVent>();
        }

        public PolusVent(IntPtr ptr) : base(ptr) { }

        private void Start() {
            vent = GetComponent<Vent>();
            pno = PogusPlugin.ObjectManager.LocateNetObject(this);
        }

        private void FixedUpdate() {
            if (pno != null && pno.HasData()) Deserialize(pno.GetSpawnData());
        }

        private void Deserialize(MessageReader reader) {
            vent.Id = reader.ReadPackedInt32();
        }
    }
}