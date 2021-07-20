using System;
using PolusggSlim.Api;
using PolusggSlim.Utils.Extensions;

namespace PolusggSlim.Auth
{
    public class AuthContext : IDisposable
    {
        public byte[] ClientId { get; set; } = { };
        
        public string ClientIdString { get; set; } = string.Empty;

        // Base64 string Client Token, but used as UTF-8 encoded
        public string ClientToken { get; set; } = "";
        public string DisplayName { get; set; } = "";

        public string[] Perks { get; set; } = { };

        public bool LoggedIn => !string.IsNullOrEmpty(DisplayName);

        public ApiClient ApiClient { get; } = new();

        public void Dispose()
        {
            ApiClient.Dispose();
        }

        public void ParseClientIdAsUuid(string uuid)
        {
            ClientIdString = uuid;
            ClientId = uuid.Replace("-", "").HexStringToByteArray();
        }
    }
}