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
		public Stream GetDataStream() {
			return File.OpenRead(LocalLocation);
		}

		public byte[] GetData() {
			return Data ??= File.ReadAllBytes(LocalLocation);
		}

		private AssetBundle LoadAssetBundle() {
			return (AssetBundle) (InternalData ??= AssetBundle.LoadFromFile(Location));
		}

		public T Get<T>() where T : Object {
			if (Type != ResourceType.Asset) throw new Exception("Invalid Get call to non-asset");
			// return ICache.Instance.CachedFiles[(uint) cacheFile.ExtraData].LoadAssetBundle().LoadAsset(cacheFile.Location).Cast<T>();
			if (InternalData == null) {
				T dontDestroy = ICache.Instance.CachedFiles[(uint) ExtraData].LoadAssetBundle().LoadAsset<T>(Location).DontDestroy();
				InternalData = dontDestroy;
				return dontDestroy;
			}

			return (T) InternalData;
		}
	}

	public static class CacheFileExtensions {
	}
}