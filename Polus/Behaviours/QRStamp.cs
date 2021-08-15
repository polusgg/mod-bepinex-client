using System;
using Polus.Extensions;
using UnhollowerRuntimeLib;
using UnityEngine;

namespace Polus.Behaviours {
    public class QRStamp : MonoBehaviour {
        static QRStamp() => ClassInjector.RegisterTypeInIl2Cpp<QRStamp>();
        public QRStamp(IntPtr ptr) : base(ptr) {}

        public SpriteRenderer Renderer;

        public void Start() {
            Renderer ??= new GameObject("QRImage").EnsureComponent<SpriteRenderer>();
            Renderer.transform.parent = transform;
            Renderer.transform.localPosition = Vector3.zero;
        }

        public void SetCode(Texture2D texture2D) {
            Renderer.sprite = Sprite.Create(texture2D, new Rect(0, 0, texture2D.width, texture2D.height), new Vector2(0.5f, 0.5f));
            Renderer.sprite.name = "QRCode";
        }
    }
}