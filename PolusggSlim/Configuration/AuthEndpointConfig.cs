namespace PolusggSlim.Configuration
{
    public class AuthEndpointConfig
    {
        public string AuthEndpoint => "https://account.polus.gg";

        // Forward slash on the end of endpoint url because of HttpClient convention
        public string PublicApiBaseUrl => "/api/v1/";
    }
}