using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using BepInEx.Logging;
using HarmonyLib;
using PolusGG.Behaviours;
using PolusGG.Extensions;
using PolusGG.Mods;
using TMPro;
using UnhollowerRuntimeLib;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace PolusGG {
    public class PggModManager {
        private static readonly string[] OnlineScenes = {"EndGame", "OnlineGame"};
        public bool AllPatched;
        public ManualLogSource Logger;
        public bool PostLoad;
        public readonly HashSet<(PggMod, Mod)> TemporaryMods = new();
        private bool _wasOnline;
        private string _previousSceneName;

        public PggModManager(ManualLogSource logger) {
            Logger = logger;
            GameObject gameObject = new("ModManager");
            gameObject.DontDestroy();
            gameObject.AddComponent<EventHandlerBehaviour>().ModManager = this;

            SceneManager.add_sceneLoaded(new Action<Scene, LoadSceneMode>((scene, mode) => {
                try {
                    // if (scene.name == "MMOnline") AccountMenu.InitializeAccountMenu(scene);
                    if (scene.name == "MMOnline") {
                        GameObject.Find("NormalMenu").FindRecursive(x => x.name == "RegionText_TMP").AddComponent<RegionTextMonitorTMP>();
                    }
                } catch {
                    throw; // ignored
                }

                if (!AllPatched) return;
                if (scene.name != "OnlineGame") PogusPlugin.ObjectManager.EndedGame();
                if (OnlineScenes.Contains(scene.name) != _wasOnline) {
                    Logger.LogInfo(scene.name);
                    _wasOnline = OnlineScenes.Contains(scene.name);
                    foreach ((_, Mod mod) in TemporaryMods)
                        if (_wasOnline) mod.LobbyJoined();
                        else
                            mod.LobbyLeft();
                }
                
                if (_previousSceneName == "OnlineGame" && scene.name == "EndGame") 
                    foreach ((_, Mod mod) in TemporaryMods)
                        mod.GameEnded();

                _previousSceneName = scene.name;
            }));
        }

        public void LoadMods() {
            if (PostLoad) return;

            if (!Directory.Exists(PggConstants.DownloadFolder)) Directory.CreateDirectory(PggConstants.DownloadFolder);
            // Directory.Delete(PggConstants.DownloadFolder, true);

            IEnumerable<Type> enumerable =
                Assembly.GetExecutingAssembly().GetTypes().Where(x => x.IsSubclassOf(typeof(Mod)));
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
                mod.Start();
                pggMod.Patch();
            }


            AllPatched = true;
        }

        public void StartMods() {
            foreach ((_, Mod mod) in TemporaryMods)
                if (PostLoad)
                    mod.Start();
        }

        public void UnpatchMods() {
            foreach ((PggMod pggMod, Mod mod) in TemporaryMods) {
                pggMod.Unpatch();
                mod.Stop();
            }

            PogusPlugin.ObjectManager.UnregisterAll();

            AllPatched = false;
        }

        public class EventHandlerBehaviour : MonoBehaviour {
            public PggModManager ModManager;

            static EventHandlerBehaviour() {
                ClassInjector.RegisterTypeInIl2Cpp<EventHandlerBehaviour>();
            }

            private void Start() {
                PlayerSpawnedEventPatch.ModManager = ModManager;
                PlayerDestroyedEventPatch.ModManager = ModManager;
            }

            public EventHandlerBehaviour(IntPtr ptr) : base(ptr) { }

            private void Update() {
                foreach ((PggMod _, Mod mod) in ModManager.TemporaryMods) mod.Update();
            }

            private void FixedUpdate() {
                foreach ((PggMod _, Mod mod) in ModManager.TemporaryMods) mod.FixedUpdate();
            }
        
            [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.Start))]
            internal class PlayerSpawnedEventPatch {
                public static PggModManager ModManager;
                [HarmonyPostfix]
                public static void Start(PlayerControl __instance) {
                    foreach ((PggMod _, Mod mod) in ModManager.TemporaryMods) mod.PlayerSpawned(__instance);
                }
            }
        
            [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.OnDestroy))]
            internal class PlayerDestroyedEventPatch {
                public static PggModManager ModManager;
                [HarmonyPostfix]
                public static void OnDestroy(PlayerControl __instance) {
                    foreach ((PggMod _, Mod mod) in ModManager.TemporaryMods) mod.PlayerDestroyed(__instance);
                }
            }
        }
    }
}