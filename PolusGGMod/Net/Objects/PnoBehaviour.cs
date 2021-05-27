using System;
using UnhollowerRuntimeLib;
using UnityEngine;

namespace PolusGG.Net {
    public class PnoBehaviour : MonoBehaviour {
        public PolusNetObject pno;

        static PnoBehaviour() {
            ClassInjector.RegisterTypeInIl2Cpp<PnoBehaviour>();
        }

        public PnoBehaviour(IntPtr ptr) : base(ptr) { }
    }
}