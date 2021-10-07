using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using BepInEx;
using BepInEx.IL2CPP;
using BepInEx.Logging;
using HarmonyLib;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Polus.Extensions;
using Polus.Net.Objects;
using Polus.Patches.Permanent;
using Polus.Resources;
using Polus.Utils;
using TMPro;
using UnhollowerBaseLib;
using UnityEngine;
using Debug = System.Diagnostics.Debug;
using Environment = Il2CppSystem.Environment;

namespace Polus {
    [BepInPlugin(Id, "Polus.gg", "0.69")]
    public class PogusPlugin : BasePlugin {
        public const string Id = "gg.polus.bepismod";
        public static ManualLogSource Logger;

        public static PggMod PermanentMod = new PermanentPggMod();
        public static PggModManager ModManager;
        public static IObjectManager ObjectManager;
        public static ICache Cache = new PggCache();

        private static AssetBundle _bundle;

        public static TMP_FontAsset font;
        public static TMP_SpriteAsset spriteSheet;

        public static int? Revision;
        public static int LauncherBuild;

        public static AssetBundle Bundle {
            get {
                if (_bundle == null) {
                    Stream strm = Assembly.GetExecutingAssembly().GetManifestResourceStream("Polus.bepinexresources");
                    Debug.Assert(strm != null, nameof(strm) + " != null");
                    byte[] ba = new byte[strm.Length];
                    strm.Read(ba, 0, ba.Length);
                    _bundle = AssetBundle.LoadFromMemory(ba);
                }

                return _bundle;
            }
        }

        // public class BelpinexPno : InnerNetObject {
        //     static BelpinexPno() {
        //         ClassInjector.RegisterTypeInIl2Cpp<BelpinexPno>();
        //     }
        //     public BelpinexPno(IntPtr ptr) : base(ptr) {}
        //
        //     private void Start() {
        //         throw new NotImplementedException();
        //     }
        // }

        public override void Load() {
            Logger = Log;
            
            BetterErrorPatches.DebugLogTrace.Initialize();

            try {
                throw new Exception("Hello world!");
            } catch (Exception ex) {
                ex.Log(comment: "Hello!");
            }

            try {
                CatchHelper.Init();

                CatchHelper.TryCatch(() => {
                    if (File.Exists("modpackage.manifest.json")) {
                        //todo move to a version management class and periodically check for updates :)
                        ModPackage modPackage = JsonConvert.DeserializeObject<ModPackage>(File.ReadAllText("modpackage.manifest.json"));
                        if (modPackage != null) {
                            Revision = modPackage.Version;
                        }
                    }
                });

                ObjectManager = new PggObjectManager();

                try {
                    LauncherInfo launcherInfo = JsonConvert.DeserializeObject<LauncherInfo>(File.ReadAllText("launcher.info.json"));
                    if (launcherInfo != null) {
                        LauncherBuild = launcherInfo.Version;
                    }
                } catch {
                    // #if RELEASE
                    // CrashAndBurn.Die("Unable to get launcher version!");
                    // #endif
                    LauncherBuild = 0;
                }

                CatchHelper.TryCatch(() => {
                    if (File.Exists(PggConstants.CacheLocation)) {
                        using FileStream stream = PggCache.GetFileStream(PggConstants.CacheLocation, FileMode.Open, FileAccess.Read, FileShare.None);
                        ((PggCache) Cache).Deserialize(new BinaryReader(stream));
                    }

                    PermanentMod.LoadPatches("gg.polus.permanent",
                        Assembly.GetExecutingAssembly().GetTypes()
                            .Where(x => x.GetCustomAttribute(typeof(HarmonyPatch)) != null).ToArray());
                    PermanentMod.Patch();
                    ModManager = new PggModManager(Log);
                    ModManager.LoadMods();
                });

                // font = Bundle.LoadAsset("Assets/Fonts/AmongUsButton2-Regular SDF.asset").Cast<TMP_FontAsset>();
                font = Bundle.LoadAsset("Assets/Fonts/ComicSansMs3 SDF.asset").Cast<TMP_FontAsset>();
                spriteSheet = Bundle.LoadAsset("Assets/Mods/Emojis/Emotes.asset").Cast<TMP_SpriteAsset>();
                TMP_Settings.instance.m_defaultSpriteAsset = spriteSheet;
                // font = Bundle.LoadAsset("Assets/Fonts/Inter-SemiBold SDF.asset").Cast<TMP_FontAsset>();
                // FontMwenuwuPatches.Load();
                CatchHelper.TryCatch(CreditsMainMenuPatches.Load);

                CosmeticsWindowButton.Load();

                ModManager.PostLoad = true;

                "This is the end of PogusPlugin.Load()".Log(1);
            } catch (Exception ex) {
                
                CrashAndBurn.Die(ex);
            }
        }

        public override bool Unload() {
            "Unload is never used".Log();
            return base.Unload();
        }

        public class ModPackage {
            [JsonProperty("version")] public int Version;
        }

        public class LauncherInfo {
            [JsonProperty("version")] public int Version;
        }
    }
}