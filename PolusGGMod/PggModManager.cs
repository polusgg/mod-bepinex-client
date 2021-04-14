using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using BepInEx.Logging;
using PolusGG.Mods;
using PolusGG.Net;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace PolusGG {
    public class PggModManager {
        public (PggMod, Mod)[] TemporaryMods;
        public bool AllPatched;
        public ManualLogSource Logger;
        private AppDomain _domain;
        public bool PostLoad;
        private bool wasOnline;
        private static readonly string[] OnlineScenes = { "EndGame", "OnlineGame" };

        public PggModManager(ManualLogSource logger) {
            Logger = logger;
            
            SceneManager.add_sceneLoaded(new Action<Scene, LoadSceneMode>((scene, mode) => {
                // yeah i could be putting this shit in a better place but who cares
    
                if (!AllPatched) return;
                if (OnlineScenes.Contains(scene.name) != wasOnline) {
                    Logger.LogInfo(scene.name);
                    wasOnline = OnlineScenes.Contains(scene.name);
                    foreach ((_, Mod mod) in TemporaryMods)
                        if (wasOnline) mod.LobbyJoined();
                        else {
                            mod.LobbyLeft();
                            PogusPlugin.ObjectManager.EndedGame();
                        }
                }

                if (scene.name == "MMOnline") {
                    AccountMenu.InitializeAccountMenu(scene);
                }
            }));
        }

        public void LoadMods() {
            if (PostLoad) return;
            
            if (!Directory.Exists(PggConstants.DownloadFolder)) {
                Directory.CreateDirectory(PggConstants.DownloadFolder);
                // Directory.Delete(PggConstants.DownloadFolder, true);
            }

            IEnumerable<Type> enumerable = Assembly.GetExecutingAssembly().GetTypes().Where(x => x.IsSubclassOf(typeof(Mod)));
            List<(PggMod, Mod)> mods = new();
            foreach (Type type in enumerable) {
                try {
                    Mod mod2;
                    mod2 = (Mod) Activator.CreateInstance(type);
                    PggMod mod = new();
                    mod.LoadPatches(Assembly.GetAssembly(type));
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
                if (PostLoad) mod.Start(PogusPlugin.ObjectManager, PogusPlugin.Cache);
                pggMod.Patch();
            }

            
            AllPatched = true;
        }

        public void StartMods() {
            foreach ((_, Mod mod) in TemporaryMods) {
                if (PostLoad) mod.Start(PogusPlugin.ObjectManager, PogusPlugin.Cache);
            }
        }

        public void UnpatchMods() {
            foreach ((PggMod pggMod, Mod mod) in TemporaryMods) {
                pggMod.Unpatch();
                mod.Unload();
            }

            ((PggObjectManager) PogusPlugin.ObjectManager).UnregisterAll();

            AllPatched = false;
        }
        
        public void UnloadMods() {
            TemporaryMods = new (PggMod, Mod)[0];
            AppDomain.Unload(_domain);
            _domain = null;
            PostLoad = false;
        }
    }
}