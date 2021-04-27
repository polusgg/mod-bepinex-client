using System;
using Hazel;
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

        // todo recreate dead body lmao
        // todo patch murder player to not show dead body lmao
        // todo patch reporting to correctly allow reporting custom dead bodies

        public PolusDeadBody(IntPtr ptr) : base(ptr) { }

        private void Start() {
            pno = new PggObjectManager().LocateNetObject(this);
            pno.OnData = Deserialize;
            rend = GetComponent<SpriteRenderer>();
            anim = GetComponent<SpriteAnim>();
            deadBody = GetComponent<DeadBody>();
            netTransform = GetComponent<PolusNetworkTransform>();
            clickBehaviour = GetComponent<PolusClickBehaviour>();
        }

        private void FixedUpdate() {
            if (pno.HasSpawnData()) Deserialize(pno.GetSpawnData());
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
    }
}