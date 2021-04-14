using System;
using Hazel;
using PolusGG.Extensions;
using PolusGG.Net;
using PolusGG.Resources;
using UnhollowerRuntimeLib;
using UnityEngine;

namespace PolusGG.Inner {
    public class PolusGraphic : PnoBehaviour {
        public SpriteRenderer renderer;
        public PolusGraphic(IntPtr ptr) : base(ptr) { }

        static PolusGraphic() {
            ClassInjector.RegisterTypeInIl2Cpp<PolusGraphic>();
        }

        private void Start() {
            pno = IObjectManager.Instance.LocateNetObject(this);
            pno.OnData = Deserialize;
            renderer = this.EnsureComponent<SpriteRenderer>();
        }

        private void FixedUpdate() {
            if (pno.HasSpawnData()) Deserialize(pno.GetSpawnData());
        }

        private void Deserialize(MessageReader reader) {
            renderer.sprite = ICache.Instance.CachedFiles[reader.ReadPackedUInt32()].Get<Sprite>();
        }
    }
}