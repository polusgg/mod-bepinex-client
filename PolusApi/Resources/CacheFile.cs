using System;
using System.IO;
using UnityEngine;
using Object = UnityEngine.Object;

namespace PolusApi.Resources {
	public struct CacheFile {
		public ResourceType Type;
		public byte[] Hash;
		public string Location;
		public byte[] Data;
		public string LocalLocation;
		public object ExtraData;
		public object InternalData;
	}

	public static class CacheFileExtensions {
		public static Stream GetDataStream(this CacheFile cacheFile) {
			return File.OpenRead(cacheFile.LocalLocation);
		}

		public static byte[] GetData(this CacheFile cacheFile) {
			return cacheFile.Data ?? (cacheFile.Data = File.ReadAllBytes(cacheFile.LocalLocation));
		}

		private static AssetBundle LoadAssetBundle(this CacheFile cacheFile) {
			return (AssetBundle) (cacheFile.InternalData ?? (cacheFile.InternalData = AssetBundle.LoadFromFile(cacheFile.Location)));
		}

		public static T Get<T>(this CacheFile cacheFile) where T : Object {
			if (cacheFile.Type != ResourceType.Asset) throw new Exception("Invalid Get call to non-asset");
			// return ICache.Instance.CachedFiles[(uint) cacheFile.ExtraData].LoadAssetBundle().LoadAsset(cacheFile.Location).Cast<T>();
			if (cacheFile.InternalData == null) {
				T dontDestroy = ICache.Instance.CachedFiles[(uint) cacheFile.ExtraData].LoadAssetBundle().LoadAsset<T>(cacheFile.Location).DontDestroy();
				cacheFile.InternalData = dontDestroy;
				return dontDestroy;
			}

			return (T) cacheFile.InternalData;
		}
	}
}