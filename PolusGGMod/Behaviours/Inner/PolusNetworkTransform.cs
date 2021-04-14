using System;
using Hazel;
using PolusGG.Enums;
using PolusGG.Extensions;
using PolusGG.Net;
using UnhollowerRuntimeLib;
using UnityEngine;

namespace PolusGG.Behaviours.Inner {
    public class PolusNetworkTransform : PnoBehaviour {
        internal static readonly FloatRange _xRange = new(50f, -50f);
        internal static readonly FloatRange _yRange = new(50f, -50f);
        private AspectPosition _aspectPosition;
        private Vector2 _target;
        private Vector2 _start;

        public PolusNetworkTransform(IntPtr ptr) : base(ptr) { }

        static PolusNetworkTransform() {
            ClassInjector.RegisterTypeInIl2Cpp<PolusNetworkTransform>();
        }

        public void Start() {
            pno = PogusPlugin.ObjectManager.LocateNetObject(this);
            pno.OnRpc = HandleRpc;
            pno.OnData = Deserialize;
            _aspectPosition = gameObject.AddComponent<AspectPosition>();
            // _rigidbody2D = GetComponent<Rigidbody2D>();
            // _rigidbody2D.constraints = RigidbodyConstraints2D.FreezeAll;
        }

        public void HandleRpc(MessageReader reader, byte callId) {
            if (callId == (int) PolusRpcCalls.SnapTo) transform.position = reader.ReadVector2();
        }

        public void FixedUpdate() {
            if (pno.HasSpawnData()) Deserialize(pno.GetSpawnData());

            Vector2 posv2 = transform.position;
            Vector2 p = _target - posv2;
            if (p.sqrMagnitude >= 0.0001f) {
                p *= Vector2.Lerp(_target, posv2, 0.1f);//todo figure out a better value for lerp
            } else {
                p = new Vector2(0, 0);
            }

            transform.position += (Vector3) (p * Time.fixedDeltaTime);
        }

        public void Deserialize(MessageReader reader) {
            _aspectPosition.Alignment = (AspectPosition.EdgeAlignments) reader.ReadByte();

            Vector3 pos = reader.ReadVector2();
            if (_aspectPosition.Alignment != 0) {
                transform.parent = HudManager.Instance.gameObject.transform;
                _aspectPosition.enabled = true;
                _aspectPosition.DistanceFromEdge = new Vector3(0, 0, -9) + pos;
                _aspectPosition.AdjustPosition();
            } else {
                _aspectPosition.enabled = false;
                int parent = reader.ReadPackedInt32();
                transform.parent = parent < 0 ? null : PogusPlugin.ObjectManager.GetNetObject((uint) parent);
                _target = pos;
            }
        }
    }
}