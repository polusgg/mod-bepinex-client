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
        internal AuthHelper AuthHelper { get; private set; }

        public override void Load()
        {
            // Harmony
            PermanentHarmony = new Harmony(Id);
            Harmony = new Harmony(Id);
            
            // Configuration
            Configuration = new PggConfiguration();

            // Services
            AuthContext = new AuthContext();
            AuthHelper = new AuthHelper(AuthContext);
            
            
            // Domain-Specific patches
            RegisterInIl2CppAttribute.Register();
            PermanentPatches.PatchAll(PermanentHarmony);
            SkipIntroSplash.Load();
            
            LocalLoad();
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
        }

        internal void LocalUnload()
        {
            PggLog.Message("Unloading Polusgg mod");
            Harmony.UnpatchSelf();
        }
    }
}