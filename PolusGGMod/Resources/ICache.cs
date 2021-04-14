using System;
using System.Collections.Generic;

namespace PolusGG.Resources {
	public interface ICache {
		public Dictionary<uint, CacheFile> CachedFiles { get; }
		public CacheFile AddToCache(uint id, string location, byte[] hash, ResourceType type, uint parentId = UInt32.MaxValue);
		public bool IsCachedAndValid(uint id, byte[] hash);
	}
}