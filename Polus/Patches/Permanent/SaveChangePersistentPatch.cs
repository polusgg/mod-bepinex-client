using System.IO;
using HarmonyLib;
using Polus.Mods.Patching;
using UnityEngine;

namespace Polus.Patches.Permanent {
    [HarmonyPatch(typeof(PlatformPaths), nameof(PlatformPaths.persistentDataPath), MethodType.Getter)]
    public class SaveChangePersistentPatch {
        public static string Location => Path.Combine(Application.persistentDataPath, "polus");
        [PermanentPatch]
        [HarmonyPostfix]
        public static void PersistentDataPath(out string __result) {
            if (!Directory.Exists(Location))
                Directory.CreateDirectory(Location);
            __result = Location;
        } 
    }
}