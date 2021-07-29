using System;
using Polus.Extensions;
using UnhollowerRuntimeLib;
using UnityEngine;

namespace Polus.Net.Objects {
    public class PnoBehaviour : MonoBehaviour {
        public PolusNetObject pno;
        public bool setByPnob;

        static PnoBehaviour() {
            ClassInjector.RegisterTypeInIl2Cpp<PnoBehaviour>();
        }

        public PnoBehaviour(IntPtr ptr) : base(ptr) { }

        public virtual void TestVirtual() {
            "Base called".Log();
        }
    }
}