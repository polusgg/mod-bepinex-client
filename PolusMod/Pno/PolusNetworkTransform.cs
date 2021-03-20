using System;
using System.IO;
using Hazel;
using PolusApi.Net;
using UnhollowerRuntimeLib;
using UnityEngine;

namespace PolusMod.Pno {
    public class PolusNetworkTransform : PnoBehaviour {
        private float _interpolateMovement = 1f;
        private Vector2 _targetSyncPosition;
        private Vector2 _targetSyncVelocity;
        private Rigidbody2D _rigidbody2D;
        private float _sendInterval = 0.1f;
        private float snapThreshold = 5f;
        private ushort _lastSequenceId;
        private static readonly FloatRange _xRange = new(-40f, 40f);
        private static readonly FloatRange _yRange = new(-40f, 40f);

        public PolusNetworkTransform(IntPtr ptr) : base(ptr) { }

        static PolusNetworkTransform() {
            ClassInjector.RegisterTypeInIl2Cpp<PolusNetworkTransform>();
        }

        private void Start() {
            pno = IObjectManager.Instance.LocateNetObject(this);
            pno.OnRpc = HandleRpc;
            pno.OnData = reader => Deserialize(reader, false);
            _rigidbody2D = GetComponent<Rigidbody2D>();
        }

        public void HandleRpc(MessageReader reader, byte callId) {
            if (callId == (int) RpcCalls.SnapTo) {
                Vector2 position = ReadVector2(reader);
                ushort minSid = reader.ReadUInt16();
                SnapTo(position, minSid);
            }
        }

        private void SnapTo(Vector2 position, ushort minSid) {
            if (!SidGreaterThan(minSid, _lastSequenceId)) {
                return;
            }

            _lastSequenceId = minSid;
            _rigidbody2D.position = position;
            _targetSyncPosition = position;
            transform.position = position;
            _targetSyncVelocity = _rigidbody2D.velocity = Vector2.zero;
        }

        public void FixedUpdate() {
            if (pno.HasSpawnData()) Deserialize(pno.GetSpawnData(), true);
            if (_interpolateMovement != 0f) {
                Vector2 vector = _targetSyncPosition - _rigidbody2D.position;
                if (vector.sqrMagnitude >= 0.0001f) {
                    float num = _interpolateMovement / _sendInterval;
                    vector.x *= num;
                    vector.y *= num;

                    _rigidbody2D.velocity = vector;
                } else {
                    _rigidbody2D.velocity = Vector2.zero;
                }
            }

            _targetSyncPosition += _targetSyncVelocity * (Time.fixedDeltaTime * 0.1f);
        }

        public void Deserialize(MessageReader reader, bool initialState) {
            if (initialState) {
                "Spawn time for cnt~".Log();
                _lastSequenceId = reader.ReadUInt16();
                _targetSyncPosition = transform.position = ReadVector2(reader);
                _targetSyncVelocity = ReadVector2(reader);
                return;
            }

            ushort newSid = reader.ReadUInt16();
            if (!SidGreaterThan(newSid, _lastSequenceId)) {
                return;
            }

            _lastSequenceId = newSid;
            if (!isActiveAndEnabled) {
                return;
            }

            _targetSyncPosition = ReadVector2(reader).Log(4, "position");
            _targetSyncVelocity = ReadVector2(reader).Log(4, "velocity");
            if (Vector2.Distance(_rigidbody2D.position, _targetSyncPosition) > snapThreshold) {
                if (_rigidbody2D) {
                    _rigidbody2D.position = _targetSyncPosition;
                    _rigidbody2D.velocity = _targetSyncVelocity;
                } else {
                    transform.position = _targetSyncPosition;
                }
            }

            if (_interpolateMovement == 0f && _rigidbody2D) {
                _rigidbody2D.position = _targetSyncPosition;
            }
        }

        public static Vector2 ReadVector2(MessageReader reader) {
            float v = reader.ReadUInt16() / 65535f;
            float v2 = reader.ReadUInt16() / 65535f;
            return new Vector2(_xRange.Lerp(v), _yRange.Lerp(v2));
        }

        private static bool SidGreaterThan(ushort newSid, ushort prevSid) {
            ushort num = (ushort) (prevSid + 32767);
            if (prevSid < num) {
                return newSid > prevSid && newSid <= num;
            }

            return newSid > prevSid || newSid <= num;
        }
    }
}