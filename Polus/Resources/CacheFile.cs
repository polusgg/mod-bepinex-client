using System;
using System.Linq;
using Polus.Extensions;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Polus.Resources {
    public class CacheFile {
        public byte[] Data;
        public object ExtraData;
        public byte[] Hash;
        public object InternalData;
        public string LocalLocation;
        public string Location;
        public ResourceType Type;

        public void Unload() {
            "POIOOOIOIOIOIOIIOIOIOIIIOIOI".Log();
            ((AssetBundle) InternalData)?.Unload(true);
        }

        private AssetBundle LoadAssetBundle() {
            return (AssetBundle) (InternalData ??= AssetBundle.LoadFromFile(LocalLocation));
        }

        public T Get<T>() where T : Object {
            if (Type != ResourceType.Asset)
                throw new Exception(
                    $"Invalid Get call to non-asset {Type} {PogusPlugin.Cache.CachedFiles.First(x => x.Value == this).Key}");
            // return ICache.Instance.CachedFiles[(uint) cacheFile.ExtraData].LoadAssetBundle().LoadAsset(cacheFile.Location).Cast<T>();
            if (InternalData == null) {
                T dontDestroy = PogusPlugin.Cache.CachedFiles[(uint) ExtraData].LoadAssetBundle().LoadAsset(Location)
                    .Cast<T>().DontDestroy();
                InternalData = dontDestroy;
                return dontDestroy;
            }

            return (T) InternalData;
        }
    }
}