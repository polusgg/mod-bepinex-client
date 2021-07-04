using System;
using System.IO;
using System.Text;
using BepInEx;
using BepInEx.IL2CPP;
using HarmonyLib;
using Newtonsoft.Json;
using PolusggSlim.Auth;
using PolusggSlim.Behaviours;
using PolusggSlim.Configuration;
using PolusggSlim.Patches.Misc;
using PolusggSlim.Utils;
using PolusggSlim.Utils.Attributes;

namespace PolusggSlim
{
    [BepInPlugin(Id, "Polus.gg", "0.1.0")]
    public class PolusggMod : BasePlugin
    {
        public const string Id = "gg.polus.bepinexmod";

        private Harmony PermanentHarmony { get; set; }
        private Harmony Harmony { get; set; }

        public PggConfiguration Configuration { get; private set; }

        // Domain-Specific objects
        internal AuthContext AuthContext { get; private set; }
        internal SigningHelper SigningHelper { get; private set; }

        public override void Load()
        {
            PluginSingleton<PolusggMod>.Instance = this;
            try
            {
                // Harmony
                PermanentHarmony = new Harmony(Id + ".permanent");
                Harmony = new Harmony(Id);

                // Configuration
                Configuration = new PggConfiguration();

                // Services
                AuthContext = new AuthContext();
                SigningHelper = new SigningHelper(AuthContext);


                // Domain-Specific patches
                RegisterInIl2CppAttribute.Register();
                PermanentPatches.PatchAll(PermanentHarmony);
                CoroutineManagerInitializer.Load();
                //TODO: SkipIntroSplash.Load();

                LocalLoad();
            }
            catch (Exception e)
            {
                PggLog.Error($"Error {e.Message}, Stack trace: {e.StackTrace}");
            }
        }

        public override bool Unload()
        {
            LocalUnload();

            //TODO: SkipIntroSplash.Unload();
            CoroutineManagerInitializer.Unload();
            PermanentHarmony.UnpatchSelf();
            AuthContext.Dispose();

            return base.Unload();
        }

        internal void LocalLoad()
        {
            PggLog.Message("Loading Polusgg mod");
            PggLog.Message($"Polusgg Server at {Configuration.Server.IpAddress}");

            var filePath = Path.Combine(Paths.GameRootPath, "api.txt");
            if (File.Exists(filePath))
            {
                var authModel = JsonConvert.DeserializeObject<SavedAuthModel>(
                    Encoding.UTF8.GetString(Convert.FromBase64String(File.ReadAllText(filePath)))
                );
                if (authModel is not null)
                {
                    AuthContext.ClientId = authModel.ClientId;
                    AuthContext.ClientToken = authModel.ClientToken;
                    AuthContext.DisplayName = authModel.DisplayName;
                    AuthContext.Perks = authModel.Perks;
                }
            }

            AccountLoginBehaviour.Load();

            Harmony.PatchAll();
        }


        internal void LocalUnload()
        {
            PggLog.Message("Unloading Polusgg mod");

            AccountLoginBehaviour.Unload();

            Harmony.UnpatchSelf();
        }
    }
}