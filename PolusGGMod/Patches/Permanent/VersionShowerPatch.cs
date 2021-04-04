using HarmonyLib;
using PolusApi.Net;

namespace PolusGGMod.Patches {
    [HarmonyPatch(typeof(VersionShower), nameof(VersionShower.Start))]
    public class VersionShowerPatch {
        [PermanentPatch]
        [HarmonyPostfix]
        public static void Start() {
            IObjectManager.Instance.EndedGame();
        }
    }
}