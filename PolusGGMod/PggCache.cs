using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Hazel;
using PolusApi.Resources;

namespace PolusGGMod {
	public class PggCache : ICache {
		private Dictionary<uint, CacheFile> _cacheFiles = new();
		private HttpClient _client = new();

		public Dictionary<uint, CacheFile> CachedFiles => _cacheFiles;

		public async Task<CacheFile> AddToCache(uint id, string location, byte[] hash, ResourceType type) {
			string path = Path.Join(PggConstants.DownloadFolder, $"{id}");

			if (!IsCachedAndValid(id, hash)) {
				var responseMessage =
					await _client.GetAsync(PggConstants.DownloadServer + PggConstants.ModListingFolder + location);
				byte[] data = responseMessage.Content.ReadAsByteArrayAsync().Result;
				if (responseMessage.StatusCode != HttpStatusCode.OK) {
					//todo log and report failure on startup or during server requested download
					PogusPlugin.Logger.LogFatal(Encoding.UTF8.GetString(data));
					throw new Exception($"Unsuccessful attempt at getting {location}");
				}

				await File.WriteAllBytesAsync(path, data);
				var cacheFile = new CacheFile {
					Hash = hash,
					Location = location,
					LocalLocation = path,
					Type = type,
					Data = data,
					ExtraData = type switch {
						ResourceType.Texture => null,
						ResourceType.Sound => null,
						ResourceType.Video => null,
						ResourceType.Reserved => null,
						ResourceType.Assembly => Assembly.ReflectionOnlyLoad(data).GetName().Name,
						_ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
					}
				};
				_cacheFiles[id] = cacheFile;
				BinaryWriter writer = new(File.Create(PggConstants.CacheLocation));
				Serialize(writer);
				writer.Close();
				PogusPlugin.Logger.LogMessage($"Downloaded and cached file at {location} ({id}, {hash})");
				return cacheFile;
			}

			CacheFile cached = _cacheFiles[id];
			PogusPlugin.Logger.LogMessage($"Using cached file {cached.LocalLocation} ({cached.Location})");
			return cached;
		}

		public bool IsCachedAndValid(uint id, byte[] hash) => _cacheFiles.ContainsKey(id) && _cacheFiles[id].Hash.SequenceEqual(hash);

		public void Serialize(BinaryWriter writer) {
			writer.Write(_cacheFiles.Count);
			foreach (var (x, y) in _cacheFiles) {
				writer.Write(x);
				writer.Write((byte) y.Type);
				writer.Write(y.Hash);
				writer.Write(y.LocalLocation);
				writer.Write(y.Location);
				switch (y.Type) {
					case ResourceType.Assembly:
						writer.Write(y.ExtraData as string ?? string.Empty);
						break;
					case ResourceType.Texture:
						break;
					case ResourceType.Sound:
						break;
					case ResourceType.Video:
						break;
					case ResourceType.Reserved:
						break;
					default:
						throw new ArgumentOutOfRangeException();
				}
			}
		}
		public void Deserialize(BinaryReader reader) {
			int length = reader.ReadInt32();
			int i = 0;
			while (i++ < length) {
				DeserializeIndexFile(reader);
			}
		}

		private void DeserializeIndexFile(BinaryReader reader) {
			CacheFile file = _cacheFiles[reader.ReadUInt32()] = new CacheFile {
				Type = (ResourceType) reader.ReadByte(),
				Hash = reader.ReadBytes(16),
				LocalLocation = reader.ReadString(),
				Location = reader.ReadString(),
			};
			file.ExtraData = file.Type switch {
				ResourceType.Assembly => reader.ReadString(),
				ResourceType.Texture => null,
				ResourceType.Sound => null,
				ResourceType.Video => null,
				ResourceType.Reserved => null,
				_ => null
			};
		}
	}

	public static class CacheFileExtensions {
		public static Stream GetDataStream(this CacheFile cacheFile) {
			return File.OpenRead(cacheFile.LocalLocation);
		}
		public static byte[] GetData(this CacheFile cacheFile) {
			return cacheFile.Data ?? (cacheFile.Data = File.ReadAllBytes(cacheFile.LocalLocation));
		}
	}
}