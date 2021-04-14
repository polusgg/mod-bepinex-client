using System;
using System.IO;

namespace PolusGG {
    public static class PggConstants {
        public static readonly string AuthBaseUrl = "https://polus.gg/api or whatever/";
        public static readonly string DownloadServer = "https://polusgg-assetbundles.nyc3.digitaloceanspaces.com/";
        public static readonly string DownloadFolder = "PolusCache" + Guid.NewGuid();
        public static readonly string CacheLocation = Path.Join(DownloadFolder, "cache.dat");

        public static readonly DnsRegionInfo Region =
            // new("", "Polus.gg Server", StringNames.NoTranslation, "127.0.0.1", 22023);
            new("", "Polus.gg Server", StringNames.NoTranslation, "151.204.156.54", 22023);
    }
}