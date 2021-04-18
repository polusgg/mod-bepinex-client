using HarmonyLib;
using PolusGG.Extensions;
using UnityEngine;

namespace PolusGG.Patches.Permanent {
    [HarmonyPatch(typeof(PlayerParticles), nameof(PlayerParticles))]
    public class SyncPlayersPatch {
        private static ObjectPoolBehavior globalPool;

        [HarmonyPrefix]
        public static void Start(PlayerParticles __instance) {
            if (globalPool == null) {
                // globalPool = Object.Instantiate(__instance.pool).DontDestroy();
                // globalPool.
            }
        }
    }
}