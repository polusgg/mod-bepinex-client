using System;
using System.Collections.Generic;

namespace PolusGG.Enums.AuthModels
{
    public class UserDataResponse
    {
        public bool Success { get; set; }
        public UserData Data { get; set; }
    }

    public class UserData
    {
        public string Uuid { get; set; }
        public string ApiToken { get; set; }
        public string DisplayName { get; set; }
        public DateTime BannedUntil { get; set; }
        public string[] Perks { get; set; }
        public UserSettings Settings { get; set; }
    }

    public class UserSettings
    {
    }
}