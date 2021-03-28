using System;
using System.IO;
using Hazel;
using PolusApi.Net;
using UnhollowerRuntimeLib;
using UnityEngine;

namespace PolusMod.Pno {
    public class PolusNetworkTransform : PnoBehaviour {
        private static readonly FloatRange _xRange = new(40f, -40f);
        private static readonly FloatRange _yRange = new(40f, -40f);
        private AspectPosition _aspectPosition;

        public PolusNetworkTransform(IntPtr ptr) : base(ptr) { }

        static PolusNetworkTransform() {
            ClassInjector.RegisterTypeInIl2Cpp<PolusNetworkTransform>();
        }

        public void Start() {
            pno = IObjectManager.Instance.LocateNetObject(this);
            pno.OnRpc = HandleRpc;
            pno.OnData = reader => Deserialize(reader);
            _aspectPosition = GetComponent<AspectPosition>();
            // _rigidbody2D = GetComponent<Rigidbody2D>();
            // _rigidbody2D.constraints = RigidbodyConstraints2D.FreezeAll;
        }

        public void HandleRpc(MessageReader reader, byte callId) {
            if (callId == (int) PolusRpcCalls.SnapTo) transform.position = ReadVector2(reader);
        }

        public void FixedUpdate() {
            if (pno.HasSpawnData()) Deserialize(pno.GetSpawnData());
        }

        public void Deserialize(MessageReader reader) {
            bool aspectPositionEnabled = _aspectPosition.enabled = reader.ReadBoolean();
            if (aspectPositionEnabled) {
                _aspectPosition.Alignment = (AspectPosition.EdgeAlignments) reader.ReadByte();
                _aspectPosition.DistanceFromEdge = ReadVector2(reader).Log(4, "distance from edge");
                _aspectPosition.AdjustPosition();
            } else {
                transform.position = ReadVector2(reader).Log(4, "position");
            }
        }

        public static Vector2 ReadVector2(MessageReader reader) {
            float v = reader.ReadUInt16() / 65535f;
            float v2 = reader.ReadUInt16() / 65535f;
            return new Vector2(_xRange.Lerp(v), _yRange.Lerp(v2));
        }
    }
}