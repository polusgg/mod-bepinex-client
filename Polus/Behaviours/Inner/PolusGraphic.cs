using System;
using Hazel;
using Polus.Extensions;
using Polus.Net.Objects;
using Polus.Resources;
using UnhollowerRuntimeLib;
using UnityEngine;

namespace Polus.Behaviours.Inner {
    public class PolusGraphic : PnoBehaviour {
        public SpriteRenderer renderer;
        private uint id;

        static PolusGraphic() {
            ClassInjector.RegisterTypeInIl2Cpp<PolusGraphic>();
        }

        public PolusGraphic(IntPtr ptr) : base(ptr) { }

        private void Start() {
            renderer = gameObject.EnsureComponent<SpriteRenderer>();
            renderer.sprite = ModManager.Instance.ModStamp.sprite;
            $"Renderer start called first {pno.NetId}".Log(10);
            gameObject.EnsureComponent<CacheListenerBehaviour>().Initialize(new CacheListener(id, (current, old) => {
                Texture2D tex = current.Get<Texture2D>();
                if (tex) SetSprite(tex);
            }));
        }

        private void FixedUpdate() {
            if (pno.HasData()) Deserialize(pno.GetData());
        }

        private void SetSprite(Texture2D tex) {
            renderer.sprite = Sprite.Create(tex, new Rect(new Vector2(), new Vector2(tex.width, tex.height)),
                new Vector2(0.5f, 0.5f));
            renderer.sprite.name = tex.name;
        }
        private void Deserialize(MessageReader reader) {
            $"Deserialize called later? {pno.NetId}".Log(10);
            Texture2D tex = PogusPlugin.Cache.GetCacheFile(id = reader.ReadPackedUInt32())?.Get<Texture2D>();
            if (tex) SetSprite(tex);
        }
    }
}