using BepInEx;
using BepInEx.IL2CPP;
using BepInEx.Logging;
using HarmonyLib;
using PolusggSlim.Patches.Misc;
using PolusggSlim.Utils;
using PolusggSlim.Utils.Attributes;

namespace PolusggSlim
{
    [BepInPlugin(Id, "Polus.gg", "0.69")]
    public class PolusggMod : BasePlugin
    {
        public const string Id = "gg.polus.bepinexmod";

        private Harmony Harmony { get; set; }
        
        public PggConfiguration Config { get; private set; }

        public override void Load()
        {
            Harmony = new Harmony(Id);
            Config = new PggConfiguration();
            
            RegisterInIl2CppAttribute.Register();
            
            
            // Domain-Specific patches
            SkipIntroSplash.Init();
        }

        public void LocalLoad()
        {
            PggLog.Message("Loading Polusgg mod");
            PggLog.Message($"Polusgg Server at {Config.Server.IpAddress}");
            Harmony.PatchAll();
        }

        public void LocalUnload()
        {
            PggLog.Message("Unloading Polusgg mod");
            Harmony.UnpatchSelf();
        }
    }
}