using HarmonyLib;

namespace Polus.Patches.Temporary {
    [HarmonyPatch(typeof(PolusShipStatus), nameof(PolusShipStatus.SpawnPlayer))]
    public class SpawnPositionFixPatch {
        
    }
}