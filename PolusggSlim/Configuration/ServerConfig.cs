namespace PolusggSlim.Configuration
{
    public class ServerConfig
    {
        public string RegionName { get; } = "Polus.gg-na-west";
        public string IpAddress { get; } = "143.198.246.211";
        public ushort Port { get; } = 22023;

        public string ServerName => $"{RegionName}-Master-1";
    }
}