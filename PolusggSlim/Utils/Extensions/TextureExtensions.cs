using System;
using UnhollowerBaseLib;
using UnityEngine;

namespace PolusggSlim.Utils.Extensions
{
    public static class TextureExtensions
    {
        public static bool LoadImage(this Texture2D tex, byte[] data, bool markNonReadable = false)
        {
            var iCallLoadImage = InternalCallHelper.GetICall<DLoadImage>("UnityEngine.ImageConversion::LoadImage");

            var il2CppArray = (Il2CppStructArray<byte>) data;

            return iCallLoadImage.Invoke(tex.Pointer, il2CppArray.Pointer, markNonReadable);
        }

        public static Sprite CreateSprite(this Texture texture, Rect rect, Vector2 pivot, float pixelsPerUnit,
            uint extrude, Vector4 border)
        {
            var iCallCreateSprite = InternalCallHelper.GetICall<DCreateSprite>("UnityEngine.Sprite::CreateSprite_Injected");

            var ptr = iCallCreateSprite.Invoke(texture.Pointer, ref rect, ref pivot, pixelsPerUnit, extrude, 1,
                ref border, false);

            return ptr == IntPtr.Zero ? null : new Sprite(ptr);
        }

        public static Sprite CreateSprite(this Texture texture)
        {
            return texture.CreateSprite(
                new Rect(0, 0, texture.width, texture.height),
                new Vector2(0.5f, 0.5f),
                100f, 0u, Vector4.zero);
        }

        private delegate bool DLoadImage(IntPtr tex, IntPtr data, bool markNonReadable);

        private delegate IntPtr DCreateSprite(
            IntPtr texture, ref Rect rect, ref Vector2 pivot, float pixelsPerUnit,
            uint extrude, int meshType, ref Vector4 border, bool generateFallbackPhysicsShape);
    }
}