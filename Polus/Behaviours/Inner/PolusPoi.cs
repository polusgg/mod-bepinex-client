using System;
using Polus.Net.Objects;
using UnhollowerRuntimeLib;
using UnityEngine;

namespace Polus.Behaviours.Inner {
    public class PolusPoi : PnoBehaviour {
        private static readonly int UILayer = LayerMask.NameToLayer("UI");
        private ArrowBehaviour arrow;
        private PolusNetworkTransform cnt;

        static PolusPoi() {
            ClassInjector.RegisterTypeInIl2Cpp<PolusPoi>();
        }

        public PolusPoi(IntPtr ptr) : base(ptr) { }

        private void Start() {
            arrow = gameObject.AddComponent<ArrowBehaviour>();
            cnt = GetComponent<PolusNetworkTransform>();
            cnt.ManuallyUsesPosition = true;
        }

        private void Update() {
            if (arrow.image == null) {
                arrow.image = GetComponent<PolusGraphic>().renderer;
            }

            gameObject.layer = UILayer;

            // if these stupid arrows aren't pointing to the point they should
            // arrow.image.enabled = cnt.MissingParent;
            arrow.target = cnt.MissingParent || cnt.CannotParent ? cnt.Position : transform.parent.position;
        }
    }
}