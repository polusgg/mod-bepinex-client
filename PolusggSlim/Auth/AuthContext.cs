using System;
using PolusggSlim.Api;

namespace PolusggSlim.Auth
{
    public class AuthContext : IDisposable
    {
        public string ClientId { get; set; } = "";
        public string ClientToken { get; set; } = "";
        public string DisplayName { get; set; } = "";

        public string[] Perks { get; set; } = { };

        public ApiClient ApiClient { get; } = new();
        
        public void ParseClientIdAsUuid(string uuid) => ClientId = uuid.Replace("-", "");

        public void Dispose() => ApiClient.Dispose();
    }
}