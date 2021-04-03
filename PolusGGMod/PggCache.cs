using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Threading;
using Newtonsoft.Json;
using PolusApi;
using PolusApi.Resources;
using UnityEngine;

namespace PolusGGMod {
    public class PggCache : ICache {
        private Dictionary<uint, CacheFile> _cacheFiles = new();
        private HttpClient _client = new();

        public Dictionary<uint, CacheFile> CachedFiles { get; }

        public CacheFile AddToCache(uint id, string location, byte[] hash, ResourceType type, uint parentId = uint.MaxValue) {
            string path = Path.Join(PggConstants.DownloadFolder, $"{id}");

            if (!IsCachedAndValid(id, hash)) {
                HttpResponseMessage responseMessage =
                    null;
                if (type == ResourceType.Asset) {
                    goto AssetOnly;
                }

                responseMessage =
                    _client.GetAsync(PggConstants.DownloadServer + location,
                        HttpCompletionOption.ResponseHeadersRead).Result;
                if (!responseMessage.IsSuccessStatusCode) {
                    //todo log and report failure on startup or during server requested download
                    PogusPlugin.Logger.LogFatal($"Failed with: {responseMessage.StatusCode}");
                    throw new CacheRequestException($"Unsuccessful attempt at getting {location}", responseMessage.StatusCode);
                }

                AssetOnly:
                CacheFile cacheFile = new() {
                    Hash = hash,
                    Location = location,
                    LocalLocation = path,
                    Type = type,
                    Data = null,
                    ExtraData = null
                };

                byte[] data;
                switch (type) {
                    case ResourceType.Assembly:
                        data = responseMessage.Content.ReadAsByteArrayAsync().Result;
                        File.WriteAllBytes(path, data);
                        cacheFile.ExtraData = Assembly.ReflectionOnlyLoad(data).GetName().Name;
                        break;
                    case ResourceType.AssetBundle:
                        data = responseMessage.Content.ReadAsByteArrayAsync().Result;
                        File.WriteAllBytes(path, data);
                        "Woozy".Log(2, "asset bundle");
                        AssetBundle bundle = AssetBundle.LoadFromMemory(data);

                        Thread.CurrentThread.ManagedThreadId.Log(16, "CURRENT THREAD AT Load");
                        Bundle bundone = JsonConvert.DeserializeObject<Bundle>(bundle.LoadAsset<TextAsset>("Assets/AssetListing.json".Log(2)).Log(2).Log(2).text.Log(2));
                        "Loggers".Log(2, "asset bundle");

                        uint assetId = bundone.BaseId;
                        foreach (string bundoneAsset in bundone.Assets)
                            AddToCache(++assetId, bundoneAsset, hash, ResourceType.Asset, parentId);
                        cacheFile.ExtraData = bundone.Assets;
                        break;
                    case ResourceType.Asset:
                        cacheFile.ExtraData = parentId;
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
            PogusPlugin.Logger.LogInfo($"Using cached file {cached.LocalLocation} ({cached.Location})");
            return cached;
        }

        public bool IsCachedAndValid(uint id, byte[] hash) =>
            _cacheFiles.ContainsKey(id) && _cacheFiles[id].Hash.SequenceEqual(hash);

        public void Serialize(BinaryWriter writer) {
            writer.Write(_cacheFiles.Count);
            foreach ((uint x, CacheFile y) in _cacheFiles) {
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
                        var extra = (string[]) y.ExtraData;
                        writer.Write(extra.Length);
                        foreach (string s in extra) {
                            writer.Write(s);
                        }
                        break;
                    case ResourceType.Asset:
                        (y.ExtraData == null).Log();
                        writer?.Write((uint) y.ExtraData);
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
                ResourceType.AssetBundle => Enumerable.Range(0, reader.ReadInt32()).Select(x => reader.ReadString()).ToArray(),
                ResourceType.Asset => reader.ReadUInt32(),
                _ => null
            };
        }

        public void Invalidate() {
            _cacheFiles = new Dictionary<uint, CacheFile>();
        }
    }
}