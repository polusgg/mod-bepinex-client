using System;
using System.Collections.Generic;
using System.Diagnostics;
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
                    await _client.GetAsync(PggConstants.DownloadServer + location,
                        HttpCompletionOption.ResponseHeadersRead);
                
                if (!responseMessage.IsSuccessStatusCode) {
                    //todo log and report failure on startup or during server requested download
                    PogusPlugin.Logger.LogFatal($"Failed with: {responseMessage.StatusCode}");
                    throw new CacheRequestException($"Unsuccessful attempt at getting {location}", responseMessage.StatusCode);
                }

                var cacheFile = new CacheFile {
                    Hash = hash,
                    Location = location,
                    LocalLocation = path,
                    Type = type,
                    Data = null,
                    ExtraData = null
                };

                switch (type) {
                    case ResourceType.Assembly:
                        byte[] data = responseMessage.Content.ReadAsByteArrayAsync().Result;
                        await File.WriteAllBytesAsync(path, data);
                        cacheFile.ExtraData = Assembly.ReflectionOnlyLoad(data).GetName().Name;
                        break;
                    case ResourceType.AssetBundle:
                        Stream stream = responseMessage.Content.ReadAsStreamAsync().Result;
                        FileStream fileStream = File.Create(path);
                        await stream.CopyToAsync(fileStream);
                        break;
                }

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

        public bool IsCachedAndValid(uint id, byte[] hash) =>
            _cacheFiles.ContainsKey(id) && _cacheFiles[id].Hash.SequenceEqual(hash);

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
                    case ResourceType.AssetBundle:
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