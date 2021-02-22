using System;
using System.Reflection;
using BepInEx;
using BepInEx.IL2CPP;
using BepInEx.Logging;

namespace PolusGGMod {
    [BepInPlugin(Id)]
    public class PogusPlugin : BasePlugin {
        public const string Id = "gg.polus.bepismod";
        public static ManualLogSource Logger;
        //todo add all permanent patches to details
        //todo add all permanent patches to details automatically
        public static PggMod PermanentMods;
        public static PggMod[] TemporaryMods;

        public override void Load() {
            Logger = Log;
            Logger.LogInfo("reuben scooben");
            AppDomain domain = AppDomain.CreateDomain("PolusGGDomain");
            AppDomain.Unload(domain);
            // domain.Load();
            //todo download mod from website
            // PggMod.LoadPatches(Assembly.GetExecutingAssembly());
        }
    }
}