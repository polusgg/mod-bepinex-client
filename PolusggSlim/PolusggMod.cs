using System;
using BepInEx;
using BepInEx.IL2CPP;
using HarmonyLib;
using PolusggSlim.Auth;
using PolusggSlim.Auth.Behaviours;
using PolusggSlim.Configuration;
using PolusggSlim.Coroutines;
using PolusggSlim.PacketLogger;
using PolusggSlim.Patches.Misc;
using PolusggSlim.Utils;
using PolusggSlim.Utils.Attributes;
using PolusggSlim.Utils.Extensions;

namespace PolusggSlim
{
    [BepInPlugin(ID, "Polus.gg", "0.1.0")]
    public class PolusggMod : BasePlugin
    {
        public const string ID = "gg.polus.bepinexmod";

        private Harmony PermanentHarmony { get; set; }
        private Harmony Harmony { get; set; }

        public PggConfiguration Configuration { get; private set; }

        // Domain-Specific objects
        internal AuthContext AuthContext { get; private set; }
        internal SigningHelper SigningHelper { get; private set; }
        internal PacketLoggerService PacketLogger { get; private set; }

        public override void Load()
        {
            PluginSingleton<PolusggMod>.Instance = this;
            try
            {
                // Harmony
                PermanentHarmony = new Harmony(ID + ".permanent");
                Harmony = new Harmony(ID);

                // Configuration
                Configuration = new PggConfiguration();

                // Services
                AuthContext = new AuthContext();
                SigningHelper = new SigningHelper(AuthContext);
                PacketLogger = new PacketLoggerService();


                // Permanent bootstrap patches
                RegisterInIl2CppAttribute.Register();
                PermanentHarmony.PatchAll(typeof(PermanentPatches));
                CoroutineManagerInitializer.Load();
                PacketLogger.Start();
                //TODO: SkipIntroSplash.Load();

                LocalLoad();
            }
            catch (Exception e)
            {
                PggLog.Error($"Error loading {ID}: {e.Message}");
                PggLog.Error($"Stack Trace: {e.StackTrace}");
            }
        }

        public override bool Unload()
        {
            LocalUnload();

            PacketLogger.Dispose();
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