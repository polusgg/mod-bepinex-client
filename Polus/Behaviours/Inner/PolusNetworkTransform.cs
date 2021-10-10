using System;
using Hazel;
using Polus.Enums;
using Polus.Extensions;
using Polus.Net.Objects;
using UnhollowerBaseLib.Attributes;
using UnhollowerRuntimeLib;
using UnityEngine;

namespace Polus.Behaviours.Inner {
    public class PolusNetworkTransform : PnoBehaviour {
        public static readonly FloatRange XRange = new(-50f, 50f);
        public static readonly FloatRange YRange = new(-50f, 50f);
        public static readonly LayerMask UILayer = LayerMask.NameToLayer("UI");
        public static readonly LayerMask DefaultLayer = LayerMask.NameToLayer("Default");

        private int? parent;
        // private Vector2 _start;
        // private Vector2 _target;

        public bool IsOnHud => AspectPosition.Alignment != 0;
        public bool CannotParent => !parent.HasValue;
        public bool MissingParent => parent.HasValue && !transform.parent;

        [HideFromIl2Cpp]
        public Vector3 Position { get; set; }

        [HideFromIl2Cpp]
        public AspectPosition AspectPosition { get; set; }
        
        [HideFromIl2Cpp]
        public bool ManuallyUsesPosition { private get; set; }


        static PolusNetworkTransform() {
            ClassInjector.RegisterTypeInIl2Cpp<PolusNetworkTransform>();
        }

        public PolusNetworkTransform(IntPtr ptr) : base(ptr) { }

        public void Start() {
            AspectPosition = gameObject.AddComponent<AspectPosition>();
            AspectPosition.updateAlways = true;
            // _rigidbody2D = GetComponent<Rigidbody2D>();
            // _rigidbody2D.constraints = RigidbodyConstraints2D.FreezeAll;
        }

        public void Update() {
            if (pno.HasData()) Deserialize(pno.GetData());
            if (pno.HasRpc()) HandleRpc(pno.GetRpcData());
            
            if (!IsOnHud && MissingParent) {
                transform.parent = PogusPlugin.ObjectManager.GetNetObject((uint) parent.Value);
                if (!ManuallyUsesPosition) transform.localPosition = Position;
            }
        }

        public void HandleRpc(PolusNetObject.Rpc rpc) {
            if (rpc.CallId == (int) PolusRpcCalls.SnapTo) transform.position = rpc.Reader.ReadVector2();
        }

        public void Deserialize(MessageReader reader) {
            byte aln = reader.ReadByte();
            AspectPosition.Alignment = (AspectPosition.EdgeAlignments) aln;
            Vector3 pos = reader.ReadVector2();
            float z = reader.ReadSingle();
            if (aln != 0) {
                transform.parent = HudManager.Instance.gameObject.transform;
                AspectPosition.enabled = true;
                AspectPosition.DistanceFromEdge = new Vector3(0, 0, z) - pos;
                AspectPosition.AdjustPosition();
                gameObject.layer = UILayer;
            } else {
                AspectPosition.enabled = false;
                parent = reader.ReadPackedInt32() switch {
                    -1 => null,
                    int x => x
                };
                Position = new Vector3(pos.x, pos.y, z);
                gameObject.layer = DefaultLayer;
            }
        }
    }
}