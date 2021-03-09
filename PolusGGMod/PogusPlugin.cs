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
using Hazel;
using PolusApi;
using PolusApi.Net;
using PolusApi.Resources;
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
        public static System.AppDomain publicDomain;
        public static bool AllPatched;
        public static PggCache Cache = new();
        
        //todo stop using appdomain

        public override void Load() {
            try {
                Logger = Log;
                Logger.LogInfo("reuben scoobenson");
                var join = Path.Join(PggConstants.DownloadFolder, "cache.dat");
                if (File.Exists(join)) {
                    var reader = new MessageReader();
                    reader.Buffer = File.ReadAllBytes(join);
                    reader.Length = reader.Buffer.Length;
                    reader.Position = 0;
                    Cache.Deserialize(reader);
                }
                PermanentMod.LoadPatches("gg.polus.permanent",
                    Assembly.GetExecutingAssembly().GetTypes()
                        .Where(x => x.GetCustomAttribute(typeof(HarmonyPatch)) != null).ToArray());
                PermanentMod.Patch();
                LoadMods();
                PatchMods();
            }
            catch (Exception e) {
                Log.LogFatal($"Failed to load!");
                Log.LogFatal(e);
                throw;
            }
        }

        public static void LoadMods() {
            HttpClient http = new();

            HttpResponseMessage dl = http.GetAsync(PggConstants.DownloadServer + PggConstants.ModListing).Result;
            string modListContent = dl.Content.ReadAsStringAsync().Result;
            System.Console.WriteLine(modListContent);
            (string, uint, byte[])[] modList = modListContent.Split("\n").Where(str => str != String.Empty).Select(x => {
                var strings = x.Split(";");
                Logger.LogInfo($"{strings[0]} {strings[1]} {strings[2]}");
                return (strings[0], uint.Parse(strings[1]), Enumerable.Range(0, strings[2].Length)
                    .Where(z => z % 2 == 0)
                    .Select(y => Convert.ToByte(strings[2].Substring(y, 2), 16))
                    .ToArray());
            }).ToArray();
            Logger.LogInfo($"found {modList.Length}");
            
            if (Directory.Exists(PggConstants.DownloadFolder)) {
                Directory.Delete(PggConstants.DownloadFolder, true);
            }
            Directory.CreateDirectory(PggConstants.DownloadFolder);
            List<Task<CacheFile>> tasks = new();
            foreach ((string mod, uint id, byte[] hash) in modList) {
                Logger.LogInfo($"{mod} {id} {hash}");
                tasks.Add(Cache.AddToCache(id, mod, hash, ResourceType.Assembly));
            }

            Task.WhenAll(tasks).Wait();

            CacheFile[] assemblies = tasks.Select(x => x.Result).ToArray();

            // publicDomain = AppDomain.CreateDomain("PggDomain");
            // publicDomain.Load(typeof(ModLoader).Assembly.Location);
            // publicDomain. = PggConstants.DownloadFolder;

            List<(PggMod, Mod)> mods = new();
            foreach (CacheFile assemblyData in assemblies) {
                try {
                    Mod mod2;
                    Assembly assembly = AppDomain.CurrentDomain.Load(assemblyData.Data);
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
            IObjectManager.Instance = new PggObjectManager();
            Logger.LogInfo(TemporaryMods.Length);
            foreach ((PggMod pggMod, Mod mod) in TemporaryMods) {
                Logger.LogInfo("sex");
                mod.Load(Logger, IObjectManager.Instance);
                pggMod.Patch();
            }

            AllPatched = true;
        }

        public static void UnpatchMods() {
            foreach ((PggMod pggMod, Mod mod) in TemporaryMods) {
                pggMod.Unpatch();
                mod.Unload();
            }

            ((PggObjectManager) IObjectManager.Instance).UnregisterAll();

            AllPatched = false;

            // TemporaryMods = new PggMod[0];
            //
            // AppDomain.Unload(publicDomain);
        }
    }
}