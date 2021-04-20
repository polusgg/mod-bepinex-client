using Newtonsoft.Json;

namespace PolusggSlim.Api.Request
{
    public class LogInRequest
    {
        [JsonProperty("email")] public string Email { get; set; } = "";

        [JsonProperty("password")] public string Password { get; set; } = "";
    }
}