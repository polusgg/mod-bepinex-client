#if no
using System;
using System.Linq;
using HarmonyLib;
using Hazel;
using PolusGG.Enums;
using PolusGG.Extensions;
using PolusGG.Net;
using UnhollowerRuntimeLib;
using UnityEngine;

namespace PolusGG.Behaviours.Inner {
    public class PolusNetworkTransform : PnoBehaviour {
        internal static readonly FloatRange XRange = new(50f, -50f);
        internal static readonly FloatRange YRange = new(50f, -50f);
        internal AspectPosition _aspectPosition;
        // private Vector2 _start;
        // private Vector2 _target;

        static PolusNetworkTransform() {
            ClassInjector.RegisterTypeInIl2Cpp<PolusNetworkTransform>();
        }

        public PolusNetworkTransform(IntPtr ptr) : base(ptr) { }

        public void Start() {
            pno = PogusPlugin.ObjectManager.LocateNetObject(this);
            _aspectPosition = gameObject.AddComponent<AspectPosition>();
            _aspectPosition.updateAlways = true;
            // _rigidbody2D = GetComponent<Rigidbody2D>();
            // _rigidbody2D.constraints = RigidbodyConstraints2D.FreezeAll;
        }

        public void FixedUpdate() {
            if (pno != null && pno.HasData()) Deserialize(pno.GetSpawnData());
            if (pno != null && pno.HasRpc()) HandleRpc(pno.GetRpcData());
        }

        public void HandleRpc(PolusNetObject.Rpc rpc) {
            if (rpc.CallId == (int) PolusRpcCalls.SnapTo) transform.position = rpc.Reader.ReadVector2();
        }

        public void Deserialize(MessageReader reader) {
            byte aln = reader.ReadByte();
            _aspectPosition.Alignment = (AspectPosition.EdgeAlignments) aln;
            Vector3 pos = reader.ReadVector2();
            float z = reader.ReadSingle();
            if (aln != 0) {
                transform.parent = HudManager.Instance.gameObject.transform;
                _aspectPosition.enabled = true;
                _aspectPosition.DistanceFromEdge = new Vector3(0, 0, z) - pos;
                _aspectPosition.AdjustPosition();
            } else {
                _aspectPosition.enabled = false;
                int parent = reader.ReadPackedInt32();
                transform.parent = parent < 0 ? null : PogusPlugin.ObjectManager.GetNetObject((uint) parent);
                // if (transform.parent) transform.parent.name.Log(5, "got it bbg");
                // else $"no homies irl {parent}".Log(5);
                transform.localPosition = new Vector3(pos.x, pos.y, z);
            }
        }
    }
}
#endif