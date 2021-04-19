using System;
using BepInEx;
using BepInEx.IL2CPP;
using HarmonyLib;
using PolusggSlim.Auth;
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
                PermanentHarmony = new Harmony(Id);
                Harmony = new Harmony(Id);
            
                // Configuration
                Configuration = new PggConfiguration();

                // Services
                AuthContext = new AuthContext();
                SigningHelper = new SigningHelper(AuthContext);
            
            
                // Domain-Specific patches
                RegisterInIl2CppAttribute.Register();
                PermanentPatches.PatchAll(PermanentHarmony);
                SkipIntroSplash.Load();
            
                LocalLoad();
            }
            catch (Exception e)
            {
                PggLog.Error($"Error {e.Message}, Stack trace: {e.StackTrace}");
            }
        }

        public override bool Unload()
        {
            Harmony.UnpatchSelf();
            SkipIntroSplash.Unload();
            AuthContext.Dispose();
            PermanentHarmony.UnpatchSelf();

            return base.Unload();
        }

        internal void LocalLoad()
        {
            PggLog.Message("Loading Polusgg mod");
            PggLog.Message($"Polusgg Server at {Configuration.Server.IpAddress}");
            Harmony.PatchAll();
            
            var result = AuthContext.ApiClient.LogIn("saghetti@polus.gg", "SDPRYpxr2vhz8xf").GetAwaiter().GetResult();
            if (result != null)
            {
                AuthContext.ParseClientIdAsUuid(result.Data.ClientId);
                AuthContext.ClientToken = result.Data.ClientToken;
                AuthContext.DisplayName = result.Data.DisplayName;
                AuthContext.Perks = result.Data.Perks;
                
                PggLog.Info($"Client Id: {AuthContext.ClientId}, Display name: {AuthContext.DisplayName}");
            }
        }

        internal void LocalUnload()
        {
            PggLog.Message("Unloading Polusgg mod");
            Harmony.UnpatchSelf();
        }
    }
}