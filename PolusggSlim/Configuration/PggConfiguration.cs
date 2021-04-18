using System;
using System.IO;
using BepInEx;
using PolusggSlim.Configuration;

namespace PolusggSlim
{
    public class PggConfiguration
    {
        public AuthEndpointConfig AuthConfig { get; } = new();
        public string DownloadServer { get; } = "https://polusgg-assetbundles.nyc3.digitaloceanspaces.com/";
        public string DownloadFolder { get; } = Path.Combine(Paths.PluginPath, "PolusggCache", Guid.NewGuid().ToString());

        public ServerConfig Server { get; } = new();
    }
}