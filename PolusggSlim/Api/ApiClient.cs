using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using PolusggSlim.Api.Response;
using PolusggSlim.Utils;

namespace PolusggSlim.Api
{
    public class ApiClient : IDisposable
    {
        private HttpClient _client;

        private JsonSerializerSettings _settings;
        
        public ApiClient()
        {
            var config = PluginSingleton<PolusggMod>.Instance.Configuration.AuthConfig;
            _client = new HttpClient
            {
                BaseAddress = new Uri(config.AuthEndpoint + config.PublicApiBaseUrl),
            };

            _client.DefaultRequestHeaders.Accept.Clear();
            _client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            
            _settings = new JsonSerializerSettings
            {
                ContractResolver = new DefaultContractResolver
                {
                    NamingStrategy = new SnakeCaseNamingStrategy()
                }
            };
        }

        internal async Task<GenericResponse<LoginResponse>> LogIn(string email, string password)
        {
            var request = new HttpRequestMessage
            {
                RequestUri = new Uri(_client.BaseAddress, "auth/token"),
                Method = HttpMethod.Post,
            };
            request.Headers.Add("Email", email);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", password);
            
            var response = await _client.SendAsync(request);
            
            if (response.IsSuccessStatusCode)
            {
                return JsonConvert.DeserializeObject<GenericResponse<LoginResponse>>(
                    await response.Content.ReadAsStringAsync(),
                    _settings
                );
            }

            return null;
        }

        internal async Task<GenericResponse<CheckTokenData>> CheckToken(string clientId, string clientToken)
        {
            var request = new HttpRequestMessage
            {
                RequestUri = new Uri(_client.BaseAddress, "auth/check"),
                Method = HttpMethod.Post,
            };
            request.Headers.Add("Client-ID", clientId);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", clientToken);
            
            var response = await _client.SendAsync(request);
            
            if (response.IsSuccessStatusCode)
            {
                return JsonConvert.DeserializeObject<GenericResponse<CheckTokenData>>(
                    await response.Content.ReadAsStringAsync(),
                    _settings
                );
            }

            return null;
        }
        
        public void Dispose() => _client?.Dispose();
    }
}