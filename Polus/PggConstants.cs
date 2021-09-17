using System;
using System.IO;
using UnhollowerBaseLib;

namespace Polus {
    public static class PggConstants {
        // public static readonly string DownloadServer = "https://client-assetbundles.polus.gg/";
        public static readonly string SaveFileName = "pggSaveData";
        private static string _dlFolder;

        public static readonly string CacheLocation = Path.Join(DownloadFolder, "cache.dat");

        // private static StaticRegionInfo _cachedRegion;

        public static string DownloadFolder {
            get {
                if (_dlFolder != null) return _dlFolder;
// #if DEBUG
                // _dlFolder = Path.Join("Cache", "PolusCache" + Guid.NewGuid());
// #else
                // _dlFolder = Path.Join("Cache","PolusCache");
                _dlFolder = "Cache";
// #endif
                return _dlFolder;
            }
        }

        public static IRegionInfo Region {
            get {
                string[] lines = File.ReadAllLines("newregion.txt");
                return new StaticRegionInfo("Region File", StringNames.NoTranslation,
                    lines[0], new Il2CppReferenceArray<ServerInfo>(new[] {
                        new ServerInfo(lines[0], lines[0], ushort.Parse(lines[1]))
                    })).Duplicate();
            }
        }
        // public static IRegionInfo Region =
        //     // new("", "Polus.gg Server", StringNames.NoTranslation, "127.0.0.1", 22023);
        //     // new("sanae6.ca", "Polus.gg Server", StringNames.NoTranslation, "97.70.174.210", 22023);
        //     new DnsRegionInfo("auvc.hall.ly", "Polus.gg Server", StringNames.NoTranslation, "151.204.156.54", 22023).Duplicate();
    }
}
