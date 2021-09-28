using System;
using Hazel;
using Polus.Extensions;
using Polus.Net.Objects;
using UnhollowerRuntimeLib;
using UnityEngine;

namespace Polus.Behaviours.Inner {
    public class PolusGraphic : PnoBehaviour {
        public SpriteRenderer renderer;

        static PolusGraphic() {
            ClassInjector.RegisterTypeInIl2Cpp<PolusGraphic>();
        }

        public PolusGraphic(IntPtr ptr) : base(ptr) { }

        private void Start() {
            renderer = gameObject.EnsureComponent<SpriteRenderer>();
            renderer.sprite = ModManager.Instance.ModStamp.sprite;
            $"Renderer start called first {pno.NetId}".Log(10);
        }

        private void FixedUpdate() {
            if (pno.HasData()) Deserialize(pno.GetSpawnData());
        }

        private void Deserialize(MessageReader reader) {
            $"Deserialize called later? {pno.NetId}".Log(10);
            Texture2D tex = PogusPlugin.Cache.CachedFiles[reader.ReadPackedUInt32()].Get<Texture2D>();
            renderer.sprite = Sprite.Create(tex, new Rect(new Vector2(), new Vector2(tex.width, tex.height)),
                new Vector2(0.5f, 0.5f));
            renderer.sprite.name = tex.name;
        }
    }
}