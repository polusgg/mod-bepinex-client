using System;
using BepInEx;
using Il2CppSystem.IO;
using Il2CppSystem.Text;
using Newtonsoft.Json;
using PolusggSlim.Api;
using PolusggSlim.Utils.Extensions;

namespace PolusggSlim.Auth
{
    public class AuthContext : IDisposable
    {
        #region Auth Data
        
        public byte[] ClientId { get; set; } = { };
        public string ClientIdString { get; set; } = string.Empty;

        // Base64 string Client Token, but used as UTF-8 encoded
        public string ClientToken { get; set; } = "";
        
        public string DisplayName { get; set; } = "";

        public string[] Perks { get; set; } = { };
        
        #endregion

        #region Associated Properties
        
        public bool LoggedIn => !string.IsNullOrEmpty(DisplayName);
        public DateTime LoggedInDateTime { get; set; }
        
        #endregion
        
        public ApiClient ApiClient { get; } = new();
        private string SaveFilePath => Path.Combine(Paths.GameRootPath, "api.txt");
        
        public void Dispose()
        {
            ApiClient.Dispose();
        }

        public void ParseClientIdAsUuid(string uuid)
        {
            ClientIdString = uuid;
            ClientId = uuid.Replace("-", "").HexStringToByteArray();
        }

        public bool LoadFromFile()
        {
            if (File.Exists(SaveFilePath))
            {
                var authModel = JsonConvert.DeserializeObject<SavedAuthModel>(
                    Encoding.UTF8.GetString(Convert.FromBase64String(File.ReadAllText(SaveFilePath)))
                );
                if (authModel is not null)
                {
                    ParseClientIdAsUuid(authModel.ClientIdString);
                    ClientToken = authModel.ClientToken;
                    DisplayName = authModel.DisplayName;
                    Perks = authModel.Perks;
                    LoggedInDateTime = authModel.LoggedInDateTime;

                    return true;
                }
            }

            return false;
        }
        
        public void SaveToFile()
        {
            File.WriteAllText(SaveFilePath, 
                Convert.ToBase64String(Encoding.UTF8.GetBytes(
                    JsonConvert.SerializeObject(new SavedAuthModel
                    {
                        ClientIdString = ClientIdString,
                        ClientToken = ClientToken,
                        DisplayName = DisplayName,
                        Perks = Perks,
                        LoggedInDateTime = LoggedInDateTime
                    })
                ))
            );
        }

        public void DeleteSaveFile()
        {
            if (File.Exists(SaveFilePath))
                File.Delete(SaveFilePath);
        }
    }
}