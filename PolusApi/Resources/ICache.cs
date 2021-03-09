using System;
using System.Threading.Tasks;
using PolusApi.Resources;

namespace PolusApi.Resources {
	public interface ICache {
		public Task<CacheFile> AddToCache(uint id, string location, byte[] hash, ResourceType type);
		public bool IsCachedAndValid(uint id, byte[] hash);
	}
}