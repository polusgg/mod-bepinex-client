using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Hazel;
using PolusApi.Resources;

namespace PolusGGMod {
	public class PggCache : ICache {
		private Dictionary<uint, CacheFile> _cacheFiles = new();
		private HttpClient _client = new();
		
		public async Task<CacheFile> AddToCache(uint id, string location, byte[] hash, ResourceType type) {
			string path = Path.Join(PggConstants.DownloadFolder, $"{id}");
			if (!IsCachedAndValid(id, hash)) {
				var responseMessage = await _client.GetAsync(PggConstants.DownloadServer+location);
				byte[] data = responseMessage.Content.ReadAsByteArrayAsync().Result;
				File.WriteAllBytes(path, data);
				var cacheFile = new CacheFile {
					Hash = hash,
					Location = location,
					Type = type,
					Data = data
				};
				_cacheFiles.Add(id, cacheFile);
				PogusPlugin.Logger.LogDebug($"Downloaded and cached file at {location} ({id}, {hash})");
				return cacheFile;
			}
			PogusPlugin.Logger.LogDebug($"Using cached file {id} ({hash})");

			return _cacheFiles[id];
		}

		public bool IsCachedAndValid(uint id, byte[] hash) =>
			_cacheFiles.ContainsKey(id) && _cacheFiles[id].Hash.SequenceEqual(hash);

		public void Deserialize(MessageReader reader) {
			while (reader.Position < reader.Length) {
				DeserializeIndexFile(reader.ReadMessage());
			}
		}

		private void DeserializeIndexFile(MessageReader reader) {
			//todo read hash, location, type
			//todo load file as well
			
		}
	}
}