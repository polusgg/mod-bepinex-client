using System;
using Polus.Extensions;
using UnhollowerRuntimeLib;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using Il2CppSystem.Collections;
using Enumerable = System.Linq.Enumerable;

namespace Polus.Behaviours {
    public class QRStamp : MonoBehaviour
    {
        static QRStamp() => ClassInjector.RegisterTypeInIl2Cpp<QRStamp>();
        protected Texture2D texture; //= new Texture2D(1, 1).DontDestroy();

        public QRStamp(IntPtr ptr) : base(ptr)
        {
        }

        public SpriteRenderer Renderer;

        public void Start()
        {
            Renderer ??= new GameObject("QRImage").EnsureComponent<SpriteRenderer>();
            Renderer.transform.parent = transform;
            Renderer.transform.localPosition = Vector3.zero;
        }

        public void SetCode(Texture2D texture2D)
        // public void SetCode(QRCoder.QRCodeData qrData)
        {
            this.texture = texture2D;
            // var pixelsPerModule = 2;
            // var size = qrData.ModuleMatrix.Count * pixelsPerModule;
            //
            // if (size != texture.width)
            // {
            //     texture.Resize(size, size);
            // }
            //
            // var darkBrush = Enumerable.Repeat(Color.black, size * size).ToArray();
            // var lightBrush = Enumerable.Repeat(Color.white, size * size).ToArray();
            // for (var x = 0; x < size; x = x + pixelsPerModule)
            // {
            //     for (var y = 0; y < size; y = y + pixelsPerModule)
            //     {
            //         var module = qrData.ModuleMatrix[(Index) ((y + pixelsPerModule) / pixelsPerModule - 1)].Cast<BitArray>()[(Index) ((x + pixelsPerModule) / pixelsPerModule - 1)];
            //         if (module)
            //             texture.SetPixels(x, y, pixelsPerModule, pixelsPerModule, darkBrush);
            //         else
            //             texture.SetPixels(x, y, pixelsPerModule, pixelsPerModule, lightBrush);
            //     }
            // }
            //
            // texture.Apply();
            Renderer.sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
            Renderer.sprite.name = "QRCode";
        }
    }
}
