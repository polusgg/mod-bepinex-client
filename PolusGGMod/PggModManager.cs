using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Loader;
using System.Text;
using System.Threading.Tasks;
using BepInEx.Logging;
using PolusApi;
using PolusApi.Net;
using PolusApi.Resources;

namespace PolusGGMod {
    public class PggModManager {
        public (PggMod, Mod)[] TemporaryMods;
        public bool AllPatched;
        public ManualLogSource Logger;
        private AppDomain _domain;
        public bool PostLoad;

        public PggModManager(ManualLogSource logger) {
            Logger = logger;
        }

        public void LoadMods() {
            if (PostLoad) return;
            HttpClient http = new();

            HttpResponseMessage dl = http.GetAsync(PggConstants.DownloadServer + PggConstants.ModListing).Result;
            string modListContent = dl.Content.ReadAsStringAsync().Result;
            // System.Console.WriteLine(modListContent);
            (string, uint, byte[])[] modList = modListContent.Split("\n").Where(str => str != String.Empty).Select(x => {
                var strings = x.Split(";");
                Logger.LogInfo($"{strings[0]} {strings[1]} {strings[2]}");
                return (strings[0], uint.Parse(strings[1]), Enumerable.Range(0, strings[2].Length)
                    .Where(z => z % 2 == 0)
                    .Select(y => Convert.ToByte(strings[2].Substring(y, 2), 16))
                    .ToArray());
            }).ToArray();
            // Logger.LogInfo($"found {modList.Length}");
            
            if (!Directory.Exists(PggConstants.DownloadFolder)) {
                Directory.CreateDirectory(PggConstants.DownloadFolder);
                // Directory.Delete(PggConstants.DownloadFolder, true);
            }
            List<Task<CacheFile>> tasks = new();
            foreach ((string mod, uint id, byte[] hash) in modList) {
                Logger.LogInfo($"{mod} {id} {hash}");
                tasks.Add(PogusPlugin.Cache.AddToCache(id, PggConstants.ModListingFolder + mod, hash, ResourceType.Assembly));
            }

            Task.WhenAll(tasks).Wait();

            CacheFile[] assemblies = tasks.Where(x => !x.IsFaulted).Select(x => x.Result).ToArray();

            //appdomains are bad post-framework
            // i also hate this code
            // _domain = AppDomain.CreateDomain("PggDomain");
            // Type type = typeof(Proxy);
            // var value = (Proxy)publicDomain.(
            //     type.Assembly.FullName,
            //     type.FullName);
            // _domain.AssemblyResolve += MyResolver;
            // _domain.Load(typeof(ModLoader).Assembly.Location);
            // _domain. = PggConstants.DownloadFolder;
            
            // Execute(out _alcReference, out _alc);

            List<(PggMod, Mod)> mods = new();
            foreach (CacheFile assemblyData in assemblies) {
                try {
                    Logger.LogWarning($"Attempting to load {assemblyData.Location}");
                    Mod mod2;
                    // Assembly assembly = _alc.LoadFromAssemblyName(new AssemblyName((assemblyData.ExtraData as string) ?? string.Empty));
                    Assembly assembly = Assembly.Load(assemblyData.GetData());
                    // Assembly assembly = _domain.Load(assemblyData.GetData());
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

        public void ReloadMods() {
            UnpatchMods();
            UnloadMods();
            LoadMods();
            PatchMods();
        }

        public void PatchMods() {
            Logger.LogInfo(TemporaryMods.Length);
            foreach ((PggMod pggMod, Mod mod) in TemporaryMods) {
                Logger.LogInfo("sex");
                mod.Logger = Logger;
                mod.Load();
                if (PostLoad) mod.Start(IObjectManager.Instance, PogusPlugin.Cache);
                pggMod.Patch();
            }

            AllPatched = true;
        }

        public void StartMods() {
            foreach ((_, Mod mod) in TemporaryMods) {
                if (PostLoad) mod.Start(IObjectManager.Instance, PogusPlugin.Cache);
            }
        }

        public void UnpatchMods() {
            foreach ((PggMod pggMod, Mod mod) in TemporaryMods) {
                pggMod.Unpatch();
                mod.Unload();
            }

            ((PggObjectManager) IObjectManager.Instance).UnregisterAll();

            // TemporaryMods = new (PggMod, Mod)[0];
            // AppDomain.Unload(publicDomain);
            //todo do assemblyloadcontext bullshit

            AllPatched = false;
        }
        
        public void UnloadMods() {
            TemporaryMods = new (PggMod, Mod)[0];
            AppDomain.Unload(_domain);
            _domain = null;
            PostLoad = false;
        }
        
        // [MethodImpl(MethodImplOptions.NoInlining)]
        // public void Execute(out WeakReference testAlcWeakRef, out ModLoadContext alc)
        // {
        //     alc = new ModLoadContext();
        //     testAlcWeakRef = new WeakReference(alc);
        //     alc.Resolving += (alc2, assemblyName) =>
        //     {
        //         var dllName = assemblyName.Name.Split(',').First();
        //         return LoadFromStreamAlc(alc2, dllName);
        //     };
        //     // probably code for running a specific plugin's code
        //     // Assembly a = alc.LoadFromAssemblyName();
        //     // var args = new object[] { 3, 2 };
        //     //
        //     // var methodInfo = a.GetExportedTypes()[0].GetMethods().Where(m => m.Name == "MethodName").ToList()[0];
        //     // var result = methodInfo.Invoke(Activator.CreateInstance(a.GetExportedTypes()[0]), args);
        // }
        //
        // [MethodImpl(MethodImplOptions.NoInlining)]
        // private void Unload(WeakReference testAlcWeakRef, ref ModLoadContext alc)
        // {
        //     // alc.Unload(); 
        //     alc = null;
        //
        //     for (int i = 0; testAlcWeakRef.IsAlive && (i < 10); i++)
        //     {
        //         GC.Collect();
        //         GC.WaitForPendingFinalizers();
        //     }
        //
        //     Logger.LogInfo($"is alive: {testAlcWeakRef.IsAlive}");
        // }
        //
        // private Assembly LoadFromStreamAlc(AssemblyLoadContext context, string name) {
        //     return context.LoadFromStream(PogusPlugin.Cache.CachedFiles
        //         .Where(x => x.Value.Type == ResourceType.Assembly)
        //         .First(x => (x.Value.ExtraData as string) == name).Value.GetDataStream());
        // }
    // }
    //
    // public class ModLoadContext : AssemblyLoadContext
    // {
    //     protected override Assembly Load(AssemblyName assemblyName) {
    //         return null;
    //     }
    }
}