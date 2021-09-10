namespace Polus.ServerList
{
    public class ServerModel
    {
        public string Address { get; set; }
        public string Region { get; set; }
        public string Subregion { get; set; }
        public string Name { get; set; }
        public bool Maintenance { get; set; }
        
        public string Ip { get; set; }
    }
}