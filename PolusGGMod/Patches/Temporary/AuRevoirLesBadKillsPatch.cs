using HarmonyLib;

namespace PolusGG.Patches.Temporary {
    [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.MurderPlayer))]
    public class AuRevoirLesBadKillsPatch {
        public static void SetImpostor() {
            
        }
        public static void UnsetImpostor() {
            
        }
    }
}