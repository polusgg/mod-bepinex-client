using System.Net;
using System.Net.Http;

namespace PolusGG.Api {
    public static class ApiHelper {
        internal static HttpClient Client;
        static ApiHelper() {
            Client = new HttpClient();
        }
    }
}