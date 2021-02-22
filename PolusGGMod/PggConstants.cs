namespace PolusGGMod {
    public static class PggConstants {
        public static readonly string DownloadServer = "http://localhost:6969";
        //todo hate rose for not supporting more than one mod
        public static readonly string ModListing = "modList.txt";

        public static readonly ServerInfo Server = new("Pogger-Lmoa-Master-1", "72.69.195.100", 22023);
        public static readonly RegionInfo Region = new("Polus.gg Server", Server.Ip, new ServerInfo[] {Server});
    }
}