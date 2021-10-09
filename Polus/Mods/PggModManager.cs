using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using BepInEx.Logging;
using HarmonyLib;
using Hazel;
using Polus.Behaviours;
using Polus.Enums;
using Polus.Extensions;
using Polus.Mods;
using Polus.Patches.Permanent;
using UnhollowerBaseLib;
using UnhollowerRuntimeLib;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Polus {
    public class PggModManager {
        private static readonly string[] OnlineScenes = {GameScenes.EndGame, GameScenes.OnlineGame};
        public bool AllPatched;
        public ManualLogSource Logger;
        public bool PostLoad;
        public readonly HashSet<(PggMod, Mod)> TemporaryMods = new();
        private bool _wasOnline;
        private string _previousSceneName;
        public static bool DebugEnabled = false;

        public PggModManager(ManualLogSource logger) {
            Logger = logger;
            GameObject gameObject = new("PolousModManager");
            gameObject.DontDestroy();
            gameObject.AddComponent<EventHandlerBehaviour>().ModManager = this;

            SceneManager.add_sceneLoaded(new Action<Scene, LoadSceneMode>((scene, mode) => {
                foreach ((_, Mod mod) in TemporaryMods) mod.SceneChanged(scene);

                if (!AllPatched) return;
                if (scene.name != GameScenes.OnlineGame) PogusPlugin.ObjectManager.EndedGame();
                if (OnlineScenes.Contains(scene.name) != _wasOnline) {
                    Logger.LogInfo(scene.name);
                    _wasOnline = OnlineScenes.Contains(scene.name);
                    foreach ((_, Mod mod) in TemporaryMods)
                        if (_wasOnline) mod.LobbyJoined();
                        else
                            mod.LobbyLeft();
                }
                
                if (_previousSceneName == GameScenes.OnlineGame && scene.name == GameScenes.EndGame) 
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
                AppDomain.CurrentDomain.GetAssemblies().SelectMany(a => a.GetTypes()).Where(t => t.BaseType == typeof(Mod));
            foreach (Type type in enumerable) {
                Mod mod;
                type.FullName.Log(comment: "Loading");
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
                // mod.Logger = Logger;
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

            ((PggObjectManager)PogusPlugin.ObjectManager).UnregisterAll();

            AllPatched = false;
        }

        public class EventHandlerBehaviour : MonoBehaviour {
            public PggModManager ModManager;
            private bool amHost;
            private bool amConnected;

            static EventHandlerBehaviour() {
                ClassInjector.RegisterTypeInIl2Cpp<EventHandlerBehaviour>();
            }

            private void Start() {
                PlayerSpawnedEventPatch.ModManager = ModManager;
                PlayerDestroyedEventPatch.ModManager = ModManager;
                GetConnectionDataPatch.ModManager = ModManager;
            }

            public EventHandlerBehaviour(IntPtr ptr) : base(ptr) { }

            private void Update() {
                foreach ((PggMod _, Mod mod) in ModManager.TemporaryMods) mod.Update();
                if (AmongUsClient.Instance && AmongUsClient.Instance.AmConnected && amHost != HostFixingPatches.AmHostDisable.AmHostReal) {
                    amHost = HostFixingPatches.AmHostDisable.AmHostReal;
                    if (amHost) foreach ((PggMod _, Mod mod) in ModManager.TemporaryMods) mod.BecameHost();
                    else foreach ((PggMod _, Mod mod) in ModManager.TemporaryMods) mod.LostHost();
                }

                if (AmongUsClient.Instance && amConnected != AmongUsClient.Instance.AmConnected) {
                    amConnected = (AmongUsClient.Instance && AmongUsClient.Instance.AmConnected);
                    if (amConnected) foreach ((PggMod _, Mod mod) in ModManager.TemporaryMods) mod.ConnectionEstablished();
                    else foreach ((PggMod _, Mod mod) in ModManager.TemporaryMods) mod.ConnectionDestroyed();
                }
            }

            private void FixedUpdate() {
                foreach ((PggMod _, Mod mod) in ModManager.TemporaryMods) mod.FixedUpdate();
            }

            private void OnGUI() {
                if (DebugEnabled) foreach ((PggMod _, Mod mod) in ModManager.TemporaryMods) mod.DebugGui();
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
        
            [HarmonyPatch(typeof(AmongUsClient), nameof(AmongUsClient.GetConnectionData))]
            internal class GetConnectionDataPatch {
                public static PggModManager ModManager;
                [HarmonyPrefix]
                public static bool GetConnectionData(AmongUsClient __instance, out Il2CppStructArray<byte> __result) {
                    MessageWriter writer = MessageWriter.Get(SendOption.None);
                    writer.Write(Constants.GetBroadcastVersion());
                    writer.Write(SaveManager.PlayerName);
                    writer.Write(SaveManager.LastLanguage);
                    writer.Write(0); // AuthManager.Instance.LastNonceReceived
                    writer.Write((byte)SaveManager.ChatModeType);
                    foreach (Mod mod in ModManager.TemporaryMods.Select(mods => mods.Item2)) {
                        if (mod.ProtocolId.HasValue) {
                            writer.StartMessage(mod.ProtocolId.Value);
                            mod.WriteExtraData(writer);
                            writer.EndMessage();    
                        }
                    }

                    __result = writer.ToByteArray(false);
                    writer.Recycle();

                    return false;
                }
            }
        }
    }
}