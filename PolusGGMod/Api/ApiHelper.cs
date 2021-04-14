using System.Net;

namespace PolusGG.Api {
    public static class ApiHelper {
        internal static WebClient WebClient;
        static ApiHelper() {
            WebClient = new WebClient();
        }
    }
}