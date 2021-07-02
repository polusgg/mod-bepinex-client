using System;
using Hazel;
using InnerNet;
using PolusGG.Extensions;
using PowerTools;
using UnityEngine;

namespace PolusGG.Behaviours.Ino {
    public class PogusDeadBody : InnerNetObject {
        private static readonly int BodyColor = Shader.PropertyToID("_BodyColor");
        private static readonly int BackColor = Shader.PropertyToID("_BackColor");
        public SpriteAnim anim;
        public DeadBody deadBody;
        public SpriteRenderer rend;
        public PolusNetworkTransform netTransform;
        public PogusClickBehaviour clickBehaviour;

        private void Start() {
            rend = GetComponent<SpriteRenderer>();
            anim = GetComponent<SpriteAnim>();
            deadBody = GetComponent<DeadBody>();
            deadBody.ParentId = 255;
            netTransform = GetComponent<PolusNetworkTransform>();
            clickBehaviour = GetComponent<PogusClickBehaviour>();
        }

        public override void Deserialize(MessageReader reader, bool initialState) {
            
        }

        public void OnReported() {
            "todo report dead body".Log();
        }
    }
}