#if no
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
            deadBody = GetComponent<DeadBody>();
            rend = deadBody.bodyRenderer;
            anim = deadBody.bodyRenderer.gameObject.GetComponent<SpriteAnim>();
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
            rend.flipX = reader.ReadBoolean();
            Color32 mainColor = new Color32(reader.ReadByte(), reader.ReadByte(), reader.ReadByte(), reader.ReadByte());
            Color32 secondColor =
                new Color32(reader.ReadByte(), reader.ReadByte(), reader.ReadByte(), reader.ReadByte());
            rend.material.SetColor("_BackColor", mainColor);
            rend.material.SetColor("_BodyColor", secondColor);
            rend.material.SetColor("_VisorColor", secondColor);
        }

        public void OnReported() => AmongUsClient.Instance.SendRpcImmediately(pno.NetId, (byte) PolusRpcCalls.ReportDeadBody);
    }
}
#endif