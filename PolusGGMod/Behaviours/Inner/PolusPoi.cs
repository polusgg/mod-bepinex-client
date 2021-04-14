using System;
using Hazel;
using PolusGG.Net;
using UnhollowerRuntimeLib;

namespace PolusGG.Inner {
    public class PolusPoi : PnoBehaviour {
        private ArrowBehaviour arrow;
        private CustomNetworkTransform cnt;
        public PolusPoi(IntPtr ptr) : base(ptr) { }

        static PolusPoi() {
            ClassInjector.RegisterTypeInIl2Cpp<PolusPoi>();
        }

        private void Start() {
            arrow.image = GetComponent<PolusGraphic>().renderer;
            cnt = GetComponent<CustomNetworkTransform>();
        }

        private void FixedUpdate() {
            arrow.target = cnt.transform.position;
        }
    }
}