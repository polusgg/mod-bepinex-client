using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using PolusApi.Resources;

namespace PolusApi.Resources {
	public interface ICache {
		public static ICache Instance { get; }
		public Dictionary<uint, CacheFile> CachedFiles { get; }
		public CacheFile AddToCache(uint id, string location, byte[] hash, ResourceType type, uint parentId = UInt32.MaxValue);
		public bool IsCachedAndValid(uint id, byte[] hash);
	}
}