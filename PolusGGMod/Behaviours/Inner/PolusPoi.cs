using System;
using PolusGG.Net;
using UnhollowerRuntimeLib;

namespace PolusGG.Behaviours.Inner {
    public class PolusPoi : PnoBehaviour {
        private ArrowBehaviour arrow;
        private PolusNetworkTransform cnt;

        static PolusPoi() {
            ClassInjector.RegisterTypeInIl2Cpp<PolusPoi>();
        }

        public PolusPoi(IntPtr ptr) : base(ptr) { }

        private void Start() {
            arrow = gameObject.AddComponent<ArrowBehaviour>();
            cnt = GetComponent<PolusNetworkTransform>();
        }

        private void FixedUpdate() {
            if (arrow.image == null) {
                arrow.image = GetComponent<PolusGraphic>().renderer;
            }
            arrow.target = cnt.transform.position;
        }
    }
}