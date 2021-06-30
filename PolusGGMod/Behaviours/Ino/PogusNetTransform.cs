using System;
using Hazel;
using InnerNet;
using PolusGG.Enums;
using PolusGG.Extensions;
using UnityEngine;

namespace PolusGG.Behaviours.Ino {
    public class PogusNetTransform : InnerNetObject {
        internal static readonly FloatRange XRange = new(50f, -50f);
        internal static readonly FloatRange YRange = new(50f, -50f);
        internal AspectPosition AspectPosition;

        private void Start() {
            AspectPosition = gameObject.AddComponent<AspectPosition>();
            AspectPosition.updateAlways = true;
        }

        public override void Deserialize(MessageReader reader, bool initialState) {
            byte aln = reader.ReadByte();
            AspectPosition.Alignment = (AspectPosition.EdgeAlignments) aln;
            Vector3 pos = reader.ReadVector2();
            float z = reader.ReadSingle();
            if (aln != 0) {
                transform.parent = HudManager.Instance.gameObject.transform;
                AspectPosition.enabled = true;
                AspectPosition.DistanceFromEdge = new Vector3(0, 0, z) - pos;
                AspectPosition.AdjustPosition();
            } else {
                AspectPosition.enabled = false;
                int parent = reader.ReadPackedInt32();
                transform.parent = parent < 0 ? null : PogusPlugin.ObjectManager.GetNetObject((uint) parent);
                // if (transform.parent) transform.parent.name.Log(5, "got it bbg");
                // else $"no homies irl {parent}".Log(5);
                transform.localPosition = new Vector3(pos.x, pos.y, z);
            }
        }
 
        public override void HandleRpc(byte callId, MessageReader reader) {
            if (callId == (int) PolusRpcCalls.SnapTo) transform.position = reader.ReadVector2();
        }
    }
}