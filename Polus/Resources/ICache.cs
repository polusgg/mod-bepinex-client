using System.Collections.Generic;

namespace Polus.Resources {
    public interface ICache {
        public Dictionary<uint, CacheFile> CachedFiles { get; }

        public CacheFile AddToCache(uint id, string location, byte[] hash, ResourceType type,
            uint parentId = uint.MaxValue);

        public bool IsCachedAndValid(uint id, byte[] hash);
    }
}