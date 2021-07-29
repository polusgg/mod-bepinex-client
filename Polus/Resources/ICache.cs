using System.Collections.Generic;

namespace Polus.Resources {
    public interface ICache {
        public record CacheAddResult(CacheFile File, bool Cached);

        public Dictionary<uint, CacheFile> CachedFiles { get; }

        public IEnumerator<CacheAddResult> AddToCache(uint id, string location, byte[] hash, ResourceType type,
            uint parentId = uint.MaxValue);

        public bool IsCachedAndValid(uint id, byte[] hash);
    }
}