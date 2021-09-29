using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Polus.ServerList
{
    public static class ServerListLoader
    {
        private static readonly HttpClient Client = new();
        public static async Task<ServerModel[]> Load()
        {
            try
            {
                var successfulServers = new List<ServerModel>();

                var servers = JsonConvert.DeserializeObject<ServerModel[]>(
                    await Client.GetStringAsync("https://serverlist.polus.gg/regions.json")
                ) ?? Array.Empty<ServerModel>();

                foreach (var server in servers)
                {
                    try
                    {
                        var ipAddr = (await Dns.GetHostAddressesAsync(server.Address)).FirstOrDefault();
                        if (ipAddr is not null && !server.Maintenance)
                        {
                            server.Ip = ipAddr.ToString();
                            successfulServers.Add(server);
                        }
                    }
                    catch (Exception) { /* ignored */ }
                }

                return successfulServers.ToArray();
            }
            catch (Exception) { /* ignored */ }


            return Array.Empty<ServerModel>();
        }
    }
}