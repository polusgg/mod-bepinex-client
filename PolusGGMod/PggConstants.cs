using System;
using System.IO;

namespace PolusGG {
    public static class PggConstants {
        public static readonly string AuthBaseUrl = "https://account.polus.gg/api/v1";
        public static readonly string DownloadServer = "https://polusgg-assetbundles.nyc3.digitaloceanspaces.com/";
        public static string DownloadFolder {
            get {
                #if DEBUG
                return "PolusCache" + Guid.NewGuid();
                #else
                
                #endif
            }
        }

        public static readonly string CacheLocation = Path.Join(DownloadFolder, "cache.dat");

        public static readonly DnsRegionInfo Region =
            // new("sanae6.ca", "Polus.gg Server", StringNames.NoTranslation, "97.70.174.210", 22023);
            new("sanae6.ca", "Polus.gg Server", StringNames.NoTranslation, "151.204.156.54", 22023);
    }
}