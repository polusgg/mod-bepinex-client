using System;
using System.IO;
using UnhollowerBaseLib;

namespace PolusGG {
    public static class PggConstants {
        public static readonly string DownloadServer = "https://polusgg-assetbundles.nyc3.digitaloceanspaces.com/";
        public static readonly string SaveFileName = "pggSaveData";
        private static string _dlFolder;

        public static string DownloadFolder {
            get {
                if (_dlFolder != null) return _dlFolder;
#if DEBUG
                _dlFolder = "PolusCache" + Guid.NewGuid();
#else
                _dlFolder = "PolusCache";
#endif
                return _dlFolder;
            }
        }

        public static readonly string CacheLocation = Path.Join(DownloadFolder, "cache.dat");

        private static DnsRegionInfo _cachedRegion;

        public static IRegionInfo Region {
            get {
                if (_cachedRegion != null) return _cachedRegion.Duplicate();
                if (!File.Exists("region.txt")) {
                    // todo load region from interwebs
                    throw new Exception("region.txt does not exist and you are dumb and stupid and dumb");
                }

                string[] lines = File.ReadAllLines("region.txt");
                return (_cachedRegion = new DnsRegionInfo("sanae6.ca", "Polus.gg Server", StringNames.NoTranslation,
                    lines[0], ushort.Parse(lines[1]))).Duplicate();
            }
        }

        // new("", "Polus.gg Server", StringNames.NoTranslation, "127.0.0.1", 22023);
        // new("sanae6.ca", "Polus.gg Server", StringNames.NoTranslation, "97.70.174.210", 22023);
        // new("sanae6.ca", "Polus.gg Server", StringNames.NoTranslation, "151.204.156.54", 22023);
    }
}