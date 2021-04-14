using System;
using PolusGG.Extensions;
using UnityEngine;
using Object = UnityEngine.Object;

namespace PolusGG.Resources {
	public class CacheFile {
		public ResourceType Type;
		public byte[] Hash;
		public string Location;
		public byte[] Data;
		public string LocalLocation;
		public object ExtraData;
		public object InternalData;

		private AssetBundle LoadAssetBundle() {
			return (AssetBundle) (InternalData ??= AssetBundle.LoadFromFile(LocalLocation));
		}

		public T Get<T>() where T : Object {
			if (Type != ResourceType.Asset) throw new Exception("Invalid Get call to non-asset");
			// return ICache.Instance.CachedFiles[(uint) cacheFile.ExtraData].LoadAssetBundle().LoadAsset(cacheFile.Location).Cast<T>();
			if (InternalData == null) {
				T dontDestroy = PogusPlugin.Cache.CachedFiles[(uint) ExtraData].LoadAssetBundle().LoadAsset(Location).Cast<T>().DontDestroy();
				InternalData = dontDestroy;
				return dontDestroy;
			}

			return (T) InternalData;
		}
	}
}