using System.IO;

namespace PolusGGMod {
    public static class PggConstants {
        public static readonly string DownloadServer = "http://localhost:6969/";
        //todo hate rose for not supporting more than one mod
        public static readonly string ModListing = "modlist";
        public static readonly string ModListingFolder = "mods/";
        public static readonly string DownloadFolder = "Polus.gg Mods";
        public static readonly string CacheLocation = Path.Join(DownloadFolder, "cache.dat");

        public static readonly ServerInfo Server = new("Pogger-Lmoa-Master-1", "127.0.0.1", 22023);
        public static readonly StaticRegionInfo Region = new("Polus.gg Server", StringNames.NoTranslation, Server.Ip, new[] { Server });
    }
}