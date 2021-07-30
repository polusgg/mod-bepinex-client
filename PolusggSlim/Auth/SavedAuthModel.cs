using System;

namespace PolusggSlim.Auth
{
    public class SavedAuthModel
    {
        public string ClientIdString { get; set; } = string.Empty;

        // Base64 string Client Token, but used as UTF-8 encoded
        public string ClientToken { get; set; } = "";
        public string DisplayName { get; set; } = "";

        public string[] Perks { get; set; } = { };
        
        public DateTime LoggedInDateTime { get; set; }
    }
}