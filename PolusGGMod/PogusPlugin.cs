using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Runtime.Loader;
using System.Threading.Tasks;
using BepInEx;
using BepInEx.IL2CPP;
using BepInEx.Logging;
using HarmonyLib;
using Hazel;
using PolusApi;
using PolusApi.Net;
using PolusApi.Resources;
using UnhollowerRuntimeLib;
using UnityEngine;

namespace PolusGGMod {
    [BepInPlugin(Id)]
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
            try {
                Logger = Log;
                if (File.Exists(PggConstants.CacheLocation)) {
                    BinaryReader reader = new(File.OpenRead(PggConstants.CacheLocation));
                    Cache.Deserialize(reader);
                    reader.Close();
                }

                Logger.LogInfo("reuben scoobenson");
                ClassInjector.RegisterTypeInIl2Cpp<PolusNetObject>();
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
    }
}