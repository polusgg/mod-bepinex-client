using System;
using Newtonsoft.Json;
using PolusGG.Enums.AuthModels;

namespace PolusGG.Api {
    public class PolusAuth {
        public static string Token { get; set; }
        public static Guid Uuid { get; set; }
        public static string DisplayName { get; set; }
        public static string[] Perks { get; set; }
        public static bool IsPlayerSignedIn { get; set; }

        public static bool Login(string email, string password) {
            ApiHelper.WebClient.BaseAddress = PggConstants.AuthBaseUrl;

            var response = ApiHelper.WebClient.UploadString("/log-in", JsonConvert.SerializeObject(new {
                Email = email,
                Authorization = $"Bearer {password}"
            }));

            var obj = JsonConvert.DeserializeObject(response);
            if (obj is LogInSuccess) {
                Uuid = new Guid(((LogInSuccess) obj).Data.ClientId);
                Token = ((LogInSuccess) obj).Data.ClientToken;
                return true;
            } else {
                return false;
            }
        }

        public static bool GetUserInfo() {
            var response = ApiHelper.WebClient.UploadString("/log-in", JsonConvert.SerializeObject(new {
                Authorization = $"Bearer {Token}"
            }));

            var obj = JsonConvert.DeserializeObject(response);
            if (obj is UserDataResponse) {
                DisplayName = ((UserDataResponse) obj).Data.DisplayName;
                Perks = ((UserDataResponse) obj).Data.Perks;
            }

            return false;
        }
    }
}