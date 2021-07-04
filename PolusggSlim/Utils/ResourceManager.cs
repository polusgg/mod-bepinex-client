using System.Collections.Generic;
using PolusggSlim.Utils.Extensions;
using UnityEngine;

namespace PolusggSlim.Utils
{
    public static class ResourceManager
    {
        private static readonly Dictionary<string, AssetBundle> AssetBundleCache = new();

        public static Texture2D GetTextureFromImg(string name)
        {
            var imgBuffer = typeof(ResourceManager).Assembly
                .GetManifestResourceStream($"PolusggSlim.Resources.{name}")
                .ReadAll();
            if (imgBuffer == null)
                return null;

            var tex = new Texture2D(1, 1, TextureFormat.ARGB32, false);
            tex.LoadImage(imgBuffer);
            return tex;
        }

        public static AssetBundle GetAssetBundle(string name)
        {
            if (AssetBundleCache.ContainsKey(name))
                return AssetBundleCache[name];

            var buffer = typeof(ResourceManager).Assembly
                .GetManifestResourceStream($"PolusggSlim.Resources.BundledAssets.{name}")
                .ReadAll();
            if (buffer == null)
                return null;
            var assetBundle = AssetBundle.LoadFromMemory(buffer);

            AssetBundleCache.Add(name, assetBundle);
            return assetBundle;
        }
    }
}