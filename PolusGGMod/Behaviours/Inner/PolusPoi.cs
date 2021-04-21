using System;
using PolusGG.Net;
using UnhollowerRuntimeLib;

namespace PolusGG.Behaviours.Inner {
    public class PolusPoi : PnoBehaviour {
        private ArrowBehaviour arrow;
        private CustomNetworkTransform cnt;

        static PolusPoi() {
            ClassInjector.RegisterTypeInIl2Cpp<PolusPoi>();
        }

        public PolusPoi(IntPtr ptr) : base(ptr) { }

        private void Start() {
            arrow = GetComponent<ArrowBehaviour>();
            arrow.image = GetComponent<PolusGraphic>().renderer;
            cnt = GetComponent<CustomNetworkTransform>();
        }

        private void FixedUpdate() {
            arrow.target = cnt.transform.position;
        }
    }
}