namespace PolusggSlim.Auth
{
    public class SavedAuthModel
    {
        public byte[] ClientId { get; set; } = { };

        // Base64 string Client Token, but used as UTF-8 encoded
        public string ClientToken { get; set; } = "";
        public string DisplayName { get; set; } = "";

        public string[] Perks { get; set; } = { };
    }
}