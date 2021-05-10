using System;
using Hazel;
using PolusGG.Enums;
using PolusGG.Extensions;
using PolusGG.Net;
using UnhollowerRuntimeLib;
using UnityEngine;

namespace PolusGG.Behaviours.Inner {
    public class PolusNetworkTransform : PnoBehaviour {
        // internal static readonly FloatRange _xRange = new(50f, -50f);
        // internal static readonly FloatRange _yRange = new(50f, -50f);
        private AspectPosition _aspectPosition;
        // private Vector2 _start;
        // private Vector2 _target;

        static PolusNetworkTransform() {
            ClassInjector.RegisterTypeInIl2Cpp<PolusNetworkTransform>();
        }

        public PolusNetworkTransform(IntPtr ptr) : base(ptr) { }

        public void Start() {
            pno = PogusPlugin.ObjectManager.LocateNetObject(this);
            pno.OnRpc = HandleRpc;
            pno.OnData = Deserialize;
            _aspectPosition = gameObject.AddComponent<AspectPosition>();
            _aspectPosition.updateAlways = true;
            // _rigidbody2D = GetComponent<Rigidbody2D>();
            // _rigidbody2D.constraints = RigidbodyConstraints2D.FreezeAll;
        }

        public void FixedUpdate() {
            if (pno.HasSpawnData()) Deserialize(pno.GetSpawnData());

            // Vector2 posv2 = transform.position;
            // Vector2 p = _target - posv2;
            // if (p.sqrMagnitude >= 0.0001f) {
            //     p *= Vector2.Lerp(_target, posv2, 0.1f);
            // } else {
            //     p = new Vector2(0, 0);
            // }

            // transform.position += (Vector3) (p * Time.fixedDeltaTime);
        }

        public void HandleRpc(MessageReader reader, byte callId) {
            if (callId == (int) PolusRpcCalls.SnapTo) transform.position = reader.ReadVector2();
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
                aln.Log(5, "huddies");
            } else {
                _aspectPosition.enabled = false;
                int parent = reader.ReadPackedInt32();
                transform.parent = parent < 0 ? null : PogusPlugin.ObjectManager.GetNetObject((uint) parent);
                if (transform.parent) transform.parent.name.Log(5, "got it bbg");
                else "no homies irl".Log(5);
                transform.position = new Vector3(pos.x, pos.y, z);
            }
        }
    }
}