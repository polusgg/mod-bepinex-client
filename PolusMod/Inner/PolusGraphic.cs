using System;
using Hazel;
using PolusApi.Net;
using PolusApi.Resources;
using UnhollowerRuntimeLib;
using UnityEngine;

namespace PolusMod.Inner {
    public class PolusGraphic : PnoBehaviour {
        public SpriteRenderer renderer;
        public PolusGraphic(IntPtr ptr) : base(ptr) { }

        static PolusGraphic() {
            ClassInjector.RegisterTypeInIl2Cpp<PolusGraphic>();
        }

        private void Start() {
            pno = IObjectManager.Instance.LocateNetObject(this);
            pno.OnData = Deserialize;
            renderer = GetComponent<SpriteRenderer>();
        }

        private void FixedUpdate() {
            if (pno.HasSpawnData()) Deserialize(pno.GetSpawnData());
        }

        private void Deserialize(MessageReader reader) {
            uint id = reader.ReadPackedUInt32();
            // if (!ICache.Instance.IsCachedAndValid(id, )) return;
            Sprite sprite = ICache.Instance.CachedFiles[id].Get<Sprite>();
            renderer.sprite = sprite;
        }
    }
}