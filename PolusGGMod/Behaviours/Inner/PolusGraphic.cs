using System;
using Hazel;
using PolusGG.Extensions;
using PolusGG.Net;
using UnhollowerRuntimeLib;
using UnityEngine;

namespace PolusGG.Behaviours.Inner {
    public class PolusGraphic : PnoBehaviour {
        public SpriteRenderer renderer;

        static PolusGraphic() {
            ClassInjector.RegisterTypeInIl2Cpp<PolusGraphic>();
        }

        public PolusGraphic(IntPtr ptr) : base(ptr) { }

        private void Start() {
            pno = new PggObjectManager().LocateNetObject(this);
            pno.OnData = Deserialize;
            renderer = gameObject.EnsureComponent<SpriteRenderer>();
        }

        private void FixedUpdate() {
            if (pno.HasSpawnData()) Deserialize(pno.GetSpawnData());
        }

        private void Deserialize(MessageReader reader) {
            Texture2D tex = PogusPlugin.Cache.CachedFiles[reader.ReadPackedUInt32()].Get<Texture2D>();
            renderer.sprite = Sprite.Create(tex, new Rect(new Vector2(), new Vector2(tex.width, tex.height)),
                new Vector2(0.5f, 0.5f));
        }
    }
}