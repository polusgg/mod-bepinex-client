using System;

namespace Polus.Resources {
    public class CacheListener : IDisposable {
        public CacheListener(ICache.CacheUpdateHandler cacheUpdated) {
            updateFunc = cacheUpdated;
            PogusPlugin.Cache.CacheUpdated += updateFunc;
        }

        public CacheListener(uint id, Action<CacheFile, CacheFile> cacheUpdated) : this((cacheId, current, old) => {
            if (id == cacheId) cacheUpdated(current, old);
        }) {}

        private readonly ICache.CacheUpdateHandler updateFunc;

        public void Dispose() {
            PogusPlugin.Cache.CacheUpdated -= updateFunc;
        }
    }
}