using HarmonyLib;

namespace PolusGG.Patches.Temporary {
    [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.MurderPlayer))]
    public class AuRevoirLesBadKillsPatch {
        [HarmonyPrefix]
        public static void SetImpostor(PlayerControl __instance, ref bool __state) {
            __state = !__instance.Data.IsImpostor;
            if (__state) __instance.Data.IsImpostor = true;
        }

        [HarmonyPostfix]
        public static void UnsetImpostor(PlayerControl __instance, ref bool __state) {
            if (__state) __instance.Data.IsImpostor = false;
        }
    }
}