using System;
using System.IO;
using System.Linq;
using System.Reflection;
using BepInEx;
using BepInEx.IL2CPP;
using BepInEx.Logging;
using HarmonyLib;
using PolusApi;
using PolusApi.Net;

namespace PolusGGMod {
    [BepInPlugin(Id, "Polus.gg", "0.69")]
    public class PogusPlugin : BasePlugin {
        public const string Id = "gg.polus.bepismod";

        public static ManualLogSource Logger;

        public static PggMod PermanentMod = new PermanentPggMod();
        public static PggModManager ModManager;
        public static PggCache Cache = new();

        //todo stop using appdomain
        //todo shut the fuck up i'm using assembly load context
        //todo shut the fuck up oh my god stop trying to put hot-reloading 

        public override void Load() {
            Logger = Log;
            if (File.Exists(PggConstants.CacheLocation)) {
                Stream file = null;
                try {
                    BinaryReader reader = new(file = File.OpenRead(PggConstants.CacheLocation));
                    Cache.Deserialize(reader);
                    reader.Close();
                } catch {
                    Logger.LogError("Cache is invalid! Not using anything from it");
                    Cache.Invalidate();
                } finally {
                    file?.Dispose();
                }
            }
            try {
                PermanentMod.LoadPatches("gg.polus.permanent",
                    Assembly.GetExecutingAssembly().GetTypes()
                        .Where(x => x.GetCustomAttribute(typeof(HarmonyPatch)) != null).ToArray());
                PermanentMod.Patch();
                IObjectManager.Instance = new PggObjectManager();
                ModManager = new PggModManager(Log);
                ModManager.LoadMods();
                ModManager.PatchMods();
            }
            catch (Exception e) {
                Log.LogFatal($"Failed to load!");
                Log.LogFatal(e);
                throw;
            }

            ModManager.PostLoad = true;
        }

        public override bool Unload() {
            "Unload is never used".Log();
            return base.Unload();
        }
    }
}