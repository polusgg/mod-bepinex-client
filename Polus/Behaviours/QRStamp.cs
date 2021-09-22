using System;
using Polus.Extensions;
using UnhollowerRuntimeLib;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using Il2CppSystem.Collections;
using Enumerable = System.Linq.Enumerable;

namespace Polus.Behaviours {
    public class QRStamp : MonoBehaviour {
        static QRStamp() => ClassInjector.RegisterTypeInIl2Cpp<QRStamp>();
        protected Texture2D texture;

        public QRStamp(IntPtr ptr) : base(ptr) { }

        public SpriteRenderer Renderer;

        public void Start() {
            Renderer ??= new GameObject("QRImage").EnsureComponent<SpriteRenderer>();
            Renderer.transform.parent = transform;
            Renderer.transform.localPosition = Vector3.zero;
        }

        public void SetCode(Texture2D texture2D) {
            texture = texture2D;
            Renderer.sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
            Renderer.sprite.name = "QRCode";
        }
    }
}