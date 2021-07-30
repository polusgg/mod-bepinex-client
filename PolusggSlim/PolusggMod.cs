using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
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
using PolusggSlim.Utils.Extensions;

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


                // Permanent bootstrap patches
                RegisterInIl2CppAttribute.Register();
                PermanentHarmony.PatchAll(typeof(PermanentPatches));
                CoroutineManagerInitializer.Load();
                //TODO: SkipIntroSplash.Load();

                LocalLoad();
            }
            catch (Exception e)
            {
                PggLog.Error($"Error loading {Id}: {e.Message}");
                PggLog.Error($"Stack Trace: {e.StackTrace}");
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

        private void LocalLoad()
        {
            PggLog.Message("Loading Polusgg mod");

            AuthContext.LoadFromFile();
            AccountLoginBehaviour.Load();

            Harmony.PatchAllExcept(typeof(PermanentPatches));
        }


        private void LocalUnload()
        {
            PggLog.Message("Unloading Polusgg mod");

            AuthContext.SaveToFile();
            AccountLoginBehaviour.Unload();

            Harmony.UnpatchSelf();
        }
    }
}