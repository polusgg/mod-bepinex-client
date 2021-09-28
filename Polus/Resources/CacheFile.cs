using System;
using System.Linq;
using Polus.Extensions;
using Polus.Utils;
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
            $"Attempting to unload {Location}".Log();
            CatchHelper.TryCatch(()=>((AssetBundle) InternalData)?.Unload(false));
        }

        private AssetBundle LoadAssetBundle() {
            return (AssetBundle) (InternalData ??= AssetBundle.LoadFromFile(LocalLocation));
        }

        public T Get<T>() where T : Object {
            if (Type != ResourceType.Asset)
                throw new InvalidOperationException(
                    $"Invalid Get call to non-asset {Type} {PogusPlugin.Cache.CachedFiles.First(x => x.Value == this).Key}");

            if (InternalData == null) {
                return (T) (InternalData = PogusPlugin.Cache.CachedFiles[(uint) ExtraData].LoadAssetBundle().LoadAsset(Location)
                    .DontDestroy().Cast<T>());
            }

            return (T) InternalData;
        }
    }
}