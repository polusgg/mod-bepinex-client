using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using BepInEx.Logging;
using PolusGG.Extensions;
using PolusGG.Mods;
using UnhollowerRuntimeLib;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace PolusGG {
    public class PggModManager {
        public HashSet<(PggMod, Mod)> TemporaryMods = new();
        public bool AllPatched;
        public ManualLogSource Logger;
        public bool PostLoad;
        private bool wasOnline;
        private static readonly string[] OnlineScenes = { "EndGame", "OnlineGame" };

        public PggModManager(ManualLogSource logger) {
            Logger = logger;
            GameObject gameObject = new("ModManager");
            gameObject.DontDestroy();
            gameObject.AddComponent<EventHandlerBehaviour>().ModManager = this;

            SceneManager.add_sceneLoaded(new Action<Scene, LoadSceneMode>((scene, mode) => {
                if (!AllPatched) return;
                if (scene.name != "OnlineGame") {
                    PogusPlugin.ObjectManager.EndedGame();
                }
                if (OnlineScenes.Contains(scene.name) != wasOnline) {
                    Logger.LogInfo(scene.name);
                    wasOnline = OnlineScenes.Contains(scene.name);
                    foreach ((_, Mod mod) in TemporaryMods)
                        if (wasOnline) mod.LobbyJoined();
                        else {
                            mod.LobbyLeft();
                        }
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
            foreach (Type type in enumerable) {
                Mod mod;
                mod = (Mod) Activator.CreateInstance(type);
                LoadMod(mod);
            }
        }

        public void LoadMod(Mod mod) {
            PggMod mod2 = new();
            mod2.LoadPatches(mod.GetType().Assembly);
            TemporaryMods.Add((mod2, mod));
        }

        // public void ReloadMods() {
        //     UnpatchMods();
        //     LoadMods();
        //     PatchMods();
        // }

        public void PatchMods() {
            Logger.LogInfo(TemporaryMods.Count);
            foreach ((PggMod pggMod, Mod mod) in TemporaryMods) {
                mod.Logger = Logger;
                mod.Start(PogusPlugin.ObjectManager, PogusPlugin.Cache);
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
                mod.Stop();
            }

            ((PggObjectManager) PogusPlugin.ObjectManager).UnregisterAll();
            
            AllPatched = false;
        }

        public class EventHandlerBehaviour : MonoBehaviour {
            public PggModManager ModManager;
            static EventHandlerBehaviour() {
                ClassInjector.RegisterTypeInIl2Cpp<EventHandlerBehaviour>();
            }
            public EventHandlerBehaviour(IntPtr ptr) : base(ptr) {}

            private void FixedUpdate() {
                foreach ((PggMod _, Mod mod) in ModManager.TemporaryMods) mod.FixedUpdate();
            }
        }
    }
}