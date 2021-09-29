using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Polus.Resources {
    public interface ICache {
        public record CacheAddResult(CacheFile File, CacheResult Cached, Exception Exception);

        public delegate void CacheUpdateHandler(uint id, CacheFile current, [MaybeNull] CacheFile old);

        public Dictionary<uint, CacheFile> CachedFiles { get; }

        public CacheFile GetCacheFile(uint id) {
            CachedFiles.TryGetValue(id, out CacheFile result);
            return result;
        }

        public IEnumerator<CacheAddResult> AddToCache(uint id, string location, byte[] hash, ResourceType type,
            uint parentId = uint.MaxValue);

        public event CacheUpdateHandler CacheUpdated;
        public bool IsCachedAndValid(uint id, byte[] hash);
    }
}