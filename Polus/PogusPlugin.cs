using System;
using System.IO;
using System.Linq;
using System.Reflection;
using BepInEx;
using BepInEx.IL2CPP;
using BepInEx.Logging;
using HarmonyLib;
using Polus.Extensions;
using Polus.Patches.Permanent;
using Polus.Resources;
using Polus.Utils;
using TMPro;
using UnityEngine;
using Debug = System.Diagnostics.Debug;

namespace Polus {
    [BepInPlugin(Id, "Polus.gg", "0.69")]
    public class PogusPlugin : BasePlugin {
        public const string Id = "gg.polus.bepismod";
        public static ManualLogSource Logger;

        public static PggMod PermanentMod = new PermanentPggMod();
        public static PggModManager ModManager;
        public static PggObjectManager ObjectManager;
        public static PggCache Cache = new();

        private static AssetBundle _bundle;

        public static TMP_FontAsset font;

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
            ObjectManager = new PggObjectManager();

            try {
                if (File.Exists(PggConstants.CacheLocation)) {
                    using FileStream stream = PggCache.GetFileStream(PggConstants.CacheLocation, FileMode.Open, FileAccess.Read, FileShare.None);
                    Cache.Deserialize(new BinaryReader(stream));
                }
                PermanentMod.LoadPatches("gg.polus.permanent",
                    Assembly.GetExecutingAssembly().GetTypes()
                        .Where(x => x.GetCustomAttribute(typeof(HarmonyPatch)) != null).ToArray());
                PermanentMod.Patch();
                ModManager = new PggModManager(Log);
                ModManager.LoadMods();
            } catch (Exception e) {
                Log.LogFatal("Failed to load!");
                Log.LogFatal(e);
                throw;
            }

            // font = Bundle.LoadAsset("Assets/Fonts/AmongUsButton2-Regular SDF.asset").Cast<TMP_FontAsset>();
            font = Bundle.LoadAsset("Assets/Fonts/ComicSansMs3 SDF.asset").Cast<TMP_FontAsset>();
            // font = Bundle.LoadAsset("Assets/Fonts/Inter-SemiBold SDF.asset").Cast<TMP_FontAsset>();
            // FontMwenuwuPatches.Load();
            CatchHelper.TryCatch(CreditsMainMenuPatches.Load);
            
            ModManager.PostLoad = true;

            "".Log(5);
            "Among Ass".Log();
            "".Log(5);
        }

        public override bool Unload() {
            "Unload is never used".Log();
            return base.Unload();
        }
    }
}