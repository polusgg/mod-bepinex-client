using System;
using Hazel;
using PolusApi.Net;
using PolusApi.Resources;
using UnhollowerRuntimeLib;
using UnityEngine;

namespace PolusMod.Pno {
    public class PolusGraphic : PnoBehaviour {
        public SpriteRenderer Renderer;
        public PolusGraphic(IntPtr ptr) : base(ptr) { }

        static PolusGraphic() {
            ClassInjector.RegisterTypeInIl2Cpp<PolusGraphic>();
        }

        private void Start() {
            pno = IObjectManager.Instance.LocateNetObject(this);
            pno.OnData = Deserialize;
            Renderer = GetComponent<SpriteRenderer>();
        }

        private void FixedUpdate() {
            if (pno.HasSpawnData()) Deserialize(pno.GetSpawnData());
        }

        private void Deserialize(MessageReader reader) {
            Sprite sprite = ICache.Instance.CachedFiles[reader.ReadUInt32()].Get<Sprite>();
            Renderer.sprite = sprite;
        }
    }
}