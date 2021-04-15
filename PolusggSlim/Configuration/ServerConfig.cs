using System.Net;

namespace PolusggSlim.Configuration
{
    public class ServerConfig
    {
        public string RegionName { get; } = "Localhost";
        public string IpAddress { get; } = "127.0.0.1";
        public ushort Port { get; } = 22023;
        
        public string ServerName => $"{RegionName}-Master-1";
    }
}