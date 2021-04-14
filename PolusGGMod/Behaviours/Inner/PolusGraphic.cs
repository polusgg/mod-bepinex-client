using System;
using Hazel;
using PolusGG.Extensions;
using PolusGG.Net;
using PolusGG.Resources;
using UnhollowerRuntimeLib;
using UnityEngine;

namespace PolusGG.Behaviours.Inner {
    public class PolusGraphic : PnoBehaviour {
        public SpriteRenderer renderer;
        public PolusGraphic(IntPtr ptr) : base(ptr) { }

        static PolusGraphic() {
            ClassInjector.RegisterTypeInIl2Cpp<PolusGraphic>();
        }

        private void Start() {
            pno = PogusPlugin.ObjectManager.LocateNetObject(this);
            pno.OnData = Deserialize;
            renderer = gameObject.EnsureComponent<SpriteRenderer>();
        }

        private void FixedUpdate() {
            if (pno.HasSpawnData()) Deserialize(pno.GetSpawnData());
        }

        private void Deserialize(MessageReader reader) {
            renderer.sprite = PogusPlugin.Cache.CachedFiles[reader.ReadPackedUInt32()].Get<Sprite>();
        }
    }
}