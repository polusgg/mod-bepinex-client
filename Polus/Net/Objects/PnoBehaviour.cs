using System;
using InnerNet;
using Polus.Extensions;
using UnhollowerRuntimeLib;
using UnityEngine;

namespace Polus.Net.Objects {
    public class PnoBehaviour : MonoBehaviour {
        public PolusNetObject pno;

        static PnoBehaviour() {
            ClassInjector.RegisterTypeInIl2Cpp<PnoBehaviour>();
        }

        public PnoBehaviour(IntPtr ptr) : base(ptr) { }
    }
}