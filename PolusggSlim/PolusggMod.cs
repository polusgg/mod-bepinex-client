using System;
using System.IO;
using BepInEx;
using BepInEx.IL2CPP;
using HarmonyLib;
using Newtonsoft.Json;
using PolusggSlim.Api;
using PolusggSlim.Auth;
using PolusggSlim.Behaviours;
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
            PermanentHarmony.UnpatchSelf();
            AuthContext.Dispose();

            return base.Unload();
        }

        internal void LocalLoad()
        {
            PggLog.Message("Loading Polusgg mod");
            PggLog.Message($"Polusgg Server at {Configuration.Server.IpAddress}");
            
#if DEBUG
            var context = PluginSingleton<PolusggMod>.Instance.AuthContext;
            var result = context.ApiClient
                .LogIn("saghetti@polus.gg", "")
                .GetAwaiter().GetResult();
            if (result != null)
            {
                context.ParseClientIdAsUuid(result.Data.ClientId);
                context.ClientToken = result.Data.ClientToken;
                context.DisplayName = result.Data.DisplayName;
                context.Perks = result.Data.Perks;
            }
#elif RELEASE
            var filePath = Path.Combine(Paths.GameRootPath, "api.txt");
            if (File.Exists(filePath))
            {
                var authModel = JsonConvert.DeserializeObject<SavedAuthModel>(File.ReadAllText(filePath));
                if (authModel is not null)
                {
                    AuthContext.ClientId = authModel.ClientId;
                    AuthContext.ClientToken = authModel.ClientToken;
                    AuthContext.DisplayName = authModel.DisplayName;
                    AuthContext.Perks = authModel.Perks;
                }
            }
#endif
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