using System;
using Newtonsoft.Json;
using PolusGG.Enums.AuthModels;

namespace PolusGG.Api {
    public class PolusAuth {
        public static string Token { get; set; }
        public static Guid Uuid { get; set; }
        public static string DisplayName { get; set; }
        public static bool IsPlayerSignedIn { get; private set; }

        public static bool Login(string email, string password) {
            ApiHelper.Client.BaseAddress = new Uri(PggConstants.AuthBaseUrl);
            //
            // var response = ApiHelper.Client.PostAsync("/auth/token", JsonConvert.SerializeObject(new {
            //     Email = email,
            //     Authorization = $"Bearer {password}"
            // }));

            // var obj = JsonConvert.DeserializeObject(response);
            // if (obj is LogInSuccess success) {
            //     Uuid = new Guid(success.Data.ClientId);
            //     Token = success.Data.ClientToken;
            //     
            //     return true;
            // } else {
                return false;
            // }
        }
    }
}