using UnhollowerBaseLib;

namespace PolusggSlim.Configuration
{
    public class ServerConfig
    {
        public string RegionName { get; } = "master-na-west";
        public string IpAddress { get; } = "144.126.208.61";
        public ushort Port { get; } = 22023;

        public string ServerName => $"{RegionName}-Master-1";
    }
}