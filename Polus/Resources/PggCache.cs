﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Polus.Extensions;
using UnityEngine;

namespace Polus.Resources {
    public class PggCache : ICache {
        public Dictionary<uint, CacheFile> CachedFiles { get; private set; } = new();

        internal HttpClient Client { get; } = new();

        public IEnumerator<ICache.CacheAddResult> AddToCache(uint id, string location, byte[] hash, ResourceType type,
            uint parentId = uint.MaxValue) {
            string path = Path.Join(PggConstants.DownloadFolder, $"{id}");

            if (!IsCachedAndValid(id, hash)) {
                HttpResponseMessage responseMessage = null;
                if (type == ResourceType.Asset) goto AssetOnly;

                Task<HttpResponseMessage> responseTask =
                    Client.GetAsync(PggConstants.DownloadServer + location,
                        HttpCompletionOption.ResponseHeadersRead);
                while (!responseTask.IsCompleted) yield return null;
                responseMessage = responseTask.Result;
                if (!responseMessage.IsSuccessStatusCode) {
                    PlayerControl.LocalPlayer.SetThickAssAndBigDumpy(true, true);
                    PogusPlugin.Logger.LogFatal($"Failed with: {responseMessage.StatusCode}");
                    throw new CacheRequestException($"Unsuccessful attempt at getting {location}",
                        responseMessage.StatusCode);
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
                    case ResourceType.Assembly: {
                        data = responseMessage.Content.ReadAsByteArrayAsync().Result;
                        using (FileStream fs = GetFileStream(path, FileMode.Create, FileAccess.Write, FileShare.None)) fs.Write(data);
                        cacheFile.ExtraData = Assembly.ReflectionOnlyLoad(data).GetName().Name;
                        break;
                    }
                    case ResourceType.AssetBundle: {
                        if (CachedFiles.ContainsKey(id) && CachedFiles[id] is {Type: ResourceType.AssetBundle} oldAssetBundle) oldAssetBundle.Unload();
                        data = responseMessage.Content.ReadAsByteArrayAsync().Result;
                        using (FileStream fs = GetFileStream(path, FileMode.Create, FileAccess.Write, FileShare.None)) fs.Write(data);
                        AssetBundle bundle = AssetBundle.LoadFromFile(path);
                        cacheFile.InternalData = bundle;

                        Bundle bundone =
                            JsonConvert.DeserializeObject<Bundle>(bundle.LoadAsset("Assets/AssetListing.json")
                                .Cast<TextAsset>().text);

                        uint assetId = bundone.BaseId;
                        foreach (string bundoneAsset in bundone.Assets) {
                            IEnumerator<ICache.CacheAddResult> assetCache = AddToCache(++assetId, bundoneAsset, hash, ResourceType.Asset, id);
                            while (assetCache.MoveNext()) yield return null;
                        }
                        cacheFile.ExtraData = bundone.Assets;
                        break;
                    }
                    case ResourceType.Asset: {
                        cacheFile.ExtraData = parentId;
                        break;
                    }
                }

                CachedFiles[id] = cacheFile;
                // if (!WaitForFile(PggConstants.CacheLocation)) {
                //     throw new Exception("Failed to get unlocked cache file!");
                // }

                using (BinaryWriter writer = new(GetFileStream(PggConstants.CacheLocation, FileMode.OpenOrCreate, FileAccess.Write, FileShare.None))) {
                    try {
                        Serialize(writer);
                    } catch (Exception ex) {
                        PogusPlugin.Logger.LogWarning($"Failed to cache {location}!");
                        PogusPlugin.Logger.LogWarning(ex);
                    }
                }

                PogusPlugin.Logger.LogMessage($"Downloaded file at {location} ({id}, {hash.Hex()})");
                yield return new ICache.CacheAddResult(cacheFile, false);
            }

            CacheFile cached = CachedFiles[id];
            PogusPlugin.Logger.LogInfo($"Using cached file {cached.LocalLocation} ({cached.LocalLocation} {cached.Hash.Hex()})");
            PogusPlugin.Logger.LogInfo($"Over {location} ({cached.Hash.Hex()})");
            yield return new ICache.CacheAddResult(cached, true);
        }

        public bool IsCachedAndValid(uint id, byte[] hash) {
            return CachedFiles.ContainsKey(id) && CachedFiles[id].Hash.SequenceEqual(hash);
        }

        public static FileStream GetFileStream(string path, FileMode mode, FileAccess access, FileShare share) {
            while (true) {
                try {
                    return new FileStream(path, mode, access, share);
                } catch {
                    /* failed, wait for unlock*/
                    Thread.Sleep(100);
                }
            }
        }

        public void Serialize(BinaryWriter writer) {
            writer.Write(CachedFiles.Count);
            foreach ((uint x, CacheFile y) in CachedFiles) {
                writer.Write(x);
                writer.Write((byte) y.Type);
                writer.Write(y.Hash);
                writer.Write(y.LocalLocation);
                writer.Write(y.Location);
                switch (y.Type) {
                    case ResourceType.Assembly: {
                        writer.Write(y.ExtraData as string ?? string.Empty);
                        break;
                    }
                    case ResourceType.AssetBundle: {
                        string[] extra = (string[]) y.ExtraData;
                        writer.Write(extra.Length);
                        foreach (string s in extra) writer.Write(s);
                        break;
                    }
                    case ResourceType.Asset: {
                        writer.Write((uint) y.ExtraData);
                        break;
                    }
                    default: {
                        PogusPlugin.Logger.LogError($"Invalid asset type {y.Type}");
                        throw new ArgumentOutOfRangeException();
                    }
                }
            }
        }

        public void Deserialize(BinaryReader reader) {
            int length = reader.ReadInt32();
            int i = 0;
            while (i++ < length) DeserializeIndexFile(reader);
        }

        private void DeserializeIndexFile(BinaryReader reader) {
            uint id = reader.ReadUInt32();
            CacheFile file = CachedFiles[id] = new CacheFile {
                Type = (ResourceType) reader.ReadByte(),
                Hash = reader.ReadBytes(32),
                LocalLocation = reader.ReadString(),
                Location = reader.ReadString()
            };
            file.ExtraData = file.Type switch {
                ResourceType.Assembly => reader.ReadString(),
                ResourceType.AssetBundle => Enumerable.Range(0, reader.ReadInt32()).Select(x => reader.ReadString())
                    .ToArray(),
                ResourceType.Asset => reader.ReadUInt32(),
                _ => null
            };
            (file.ExtraData == null).Log(comment: $"for null on {id} which is {file.Type}");
        }

        public void Invalidate() {
            CachedFiles = new Dictionary<uint, CacheFile>();
        }
    }
}