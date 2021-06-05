using System;
using Hazel;
using PolusGG.Enums;
using PolusGG.Extensions;
using PolusGG.Net;
using PowerTools;
using UnhollowerRuntimeLib;
using UnityEngine;

namespace PolusGG.Behaviours.Inner {
    public class PolusDeadBody : PnoBehaviour {
        private static readonly int BodyColor = Shader.PropertyToID("_BodyColor");
        private static readonly int BackColor = Shader.PropertyToID("_BackColor");
        public SpriteAnim anim;
        public DeadBody deadBody;
        public SpriteRenderer rend;
        public PolusNetworkTransform netTransform;
        public PolusClickBehaviour clickBehaviour;

        static PolusDeadBody() {
            ClassInjector.RegisterTypeInIl2Cpp<PolusDeadBody>();
        }

        // todo patch reporting to correctly allow reporting custom dead bodies

        public PolusDeadBody(IntPtr ptr) : base(ptr) { }

        private void Start() {
            pno = PogusPlugin.ObjectManager.LocateNetObject(this);
            rend = GetComponent<SpriteRenderer>();
            anim = GetComponent<SpriteAnim>();
            deadBody = GetComponent<DeadBody>();
            deadBody.ParentId = 255;
            netTransform = GetComponent<PolusNetworkTransform>();
            clickBehaviour = GetComponent<PolusClickBehaviour>();
            if (pno.HasData()) Deserialize(pno.GetSpawnData());
        }

        private void FixedUpdate() {
            if (pno != null && pno.HasData()) Deserialize(pno.GetSpawnData());
        }

        public void Deserialize(MessageReader reader) {
            anim.SetNormalizedTime(reader.ReadBoolean() ? 1 : 0);
            // reader.ReadBoolean();
            rend.flipX = reader.ReadBoolean();
            // transform.localScale = new Vector3(reader.ReadBoolean() ? -0.7f : 0.7f, 0.7f, 0.7f);
            rend.material.SetColor(BackColor,
                new Color32(reader.ReadByte(), reader.ReadByte(), reader.ReadByte(), reader.ReadByte()));
            rend.material.SetColor(BodyColor,
                new Color32(reader.ReadByte(), reader.ReadByte(), reader.ReadByte(), reader.ReadByte()));
        }

        public void OnReported() => AmongUsClient.Instance.SendRpcImmediately(pno.NetId, (byte) PolusRpcCalls.ReportDeadBody);
    }
}