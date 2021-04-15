using Newtonsoft.Json;
using PolusGG.Enums.AuthModels;

namespace PolusGG.Api {
    public class PolusAuth {
        public static bool IsPlayerSignedIn { get; } 
        public static string Token => "piss";
        public static byte[] Uuid => Encoding.UTF8.GetBytes("9a8e0c94d64945b59fcf2c8870aa8975");
        public static string DisplayName => "Sane6";

        public static string[] Perks => new[] {
            "yo mama"
        };

        public static bool Login(string email, string password)
        {
            ApiHelper.WebClient.BaseAddress = PggConstants.AuthBaseUrl;

            var response = ApiHelper.WebClient.UploadString("/log-in", JsonConvert.SerializeObject(new
            {
                Email = email,
                Authorization = $"Bearer {password}"
            }));

            var obj = JsonConvert.DeserializeObject(response);
            if (obj is LogInSuccess)
            {
                Uuid = ((LogInSuccess) obj).Data.ClientId;
                Token = ((LogInSuccess) obj).Data.ClientToken;
                return true;
            }
            else
            {
                return false;
            }
        }

        public static bool GetUserInfo()
        {
            var response = ApiHelper.WebClient.UploadString("/log-in", JsonConvert.SerializeObject(new
            {
                Authorization = $"Bearer {Token}"
            }));

            var obj = JsonConvert.DeserializeObject(response);
            if (obj is UserDataResponse)
            {
                DisplayName = ((UserDataResponse) obj).Data.DisplayName;
                Perks = ((UserDataResponse) obj).Data.Perks;
            }
        }
    }
}