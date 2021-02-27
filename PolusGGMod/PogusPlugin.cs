using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;
using BepInEx;
using BepInEx.IL2CPP;
using BepInEx.Logging;
using HarmonyLib;
using PolusApi;
using UnityEngine;

namespace PolusGGMod {
    [BepInPlugin(Id)]
    public class PogusPlugin : BasePlugin {
        public const string Id = "gg.polus.bepismod";
        public static ManualLogSource Logger;
        //todo add all permanent patches to details
        //todo add all permanent patches to details automatically
        public static PggMod PermanentMod = new PermanentPggMod();
        public static (PggMod, Mod)[] TemporaryMods;
        // public static AssemblyLoadContext LoadContext = 
        public static AppDomain publicDomain;
        public static bool AllPatched;
        
        //todo stop using appdomain

        public override void Load() {
            try {
                Logger = Log;
                Logger.LogInfo("reuben scoobenson");
                PermanentMod.LoadPatches("gg.polus.permanent",
                    Assembly.GetExecutingAssembly().GetTypes()
                        .Where(x => x.GetCustomAttribute(typeof(HarmonyPatch)) != null).ToArray());
                PermanentMod.Patch();
                LoadMods();
                PatchMods();
                // domain.Load();
                //todo download mod from website
            }
            catch (Exception e) {
                Log.LogFatal($"Failed to load!");
                Log.LogFatal(e);
                throw;
            }
        }

        public static void LoadMods() {
            HttpClient http = new HttpClient();

            HttpResponseMessage dl = http.GetAsync(PggConstants.DownloadServer + PggConstants.ModListing).Result;
            string[] modList = dl.Content.ReadAsStringAsync().Result.Split("\n");
            List<Task<HttpResponseMessage>> tasks = new();
            foreach (string mod in modList) {
                tasks.Add(http.GetAsync(PggConstants.DownloadServer + mod));
            }

            Task.WhenAll(tasks).Wait();

            byte[][] assemblies = tasks.Select(x => x.Result.Content.ReadAsByteArrayAsync().Result).ToArray();

            //todo loading cached dlls from download folder 
            if (Directory.Exists(PggConstants.DownloadFolder)) {
                Directory.Delete(PggConstants.DownloadFolder, true);
            }
            Directory.CreateDirectory(PggConstants.DownloadFolder);
            int i = 0;
            string path;
            foreach (byte[] assembly in assemblies) {
                path = Path.Join(PggConstants.DownloadFolder, modList[i++]);
                if (!Directory.Exists(Path.GetDirectoryName(path))) {
                    Directory.CreateDirectory(Path.GetDirectoryName(path));
                }

                File.WriteAllBytes(path, assembly);
            }

            // publicDomain = AppDomain.CreateDomain("PggDomain");
            // publicDomain.Load(typeof(ModLoader).Assembly.Location);
            // publicDomain. = PggConstants.DownloadFolder;

            List<(PggMod, Mod)> mods = new();
            foreach (byte[] assemblyData in assemblies) {
                try {
                    Mod mod2;
                    Assembly assembly = AppDomain.CurrentDomain.Load(assemblyData);
                    Type modType = assembly.GetTypes().First(x => x.IsSubclassOf(typeof(Mod)));
                    mod2 = (Mod) Activator.CreateInstance(modType);
                    PggMod mod = new();
                    mod.LoadPatches(assembly);
                    mods.Add((mod, mod2));
                }
                catch (Exception e) {
                    Logger.LogFatal(e);
                }
            }

            TemporaryMods = mods.ToArray();
        }

        public static void PatchMods() {
            foreach ((PggMod pggMod, Mod mod) in TemporaryMods) {
                ManualLogSource logger = BepInEx.Logging.Logger.CreateLogSource(mod.Name);
                logger.LogInfo("sex");
                mod.Load(logger);
                pggMod.Patch();
            }

            AllPatched = true;
        }

        public static void UnpatchMods() {
            foreach ((PggMod pggMod, Mod mod) in TemporaryMods) {
                pggMod.Unpatch();
                mod.Unload();
            }

            AllPatched = false;

            // TemporaryMods = new PggMod[0];
            //
            // AppDomain.Unload(publicDomain);
        }
    }
}