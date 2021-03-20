using System;
using UnhollowerRuntimeLib;
using UnityEngine;

namespace PolusApi.Net {
    public class PnoBehaviour : MonoBehaviour {
        public PolusNetObject pno;
        public PnoBehaviour(IntPtr ptr) : base(ptr) {}

        static PnoBehaviour() {
            ClassInjector.RegisterTypeInIl2Cpp<PnoBehaviour>();
        }
    }
}