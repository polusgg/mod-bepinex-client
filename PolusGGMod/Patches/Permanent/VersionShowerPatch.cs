using HarmonyLib;
using PolusGG.Net;

namespace PolusGG.Patches.Permanent {
    [HarmonyPatch(typeof(VersionShower), nameof(VersionShower.Start))]
    public class VersionShowerPatch {
        [PermanentPatch]
        [HarmonyPostfix]
        public static void Start() {
            IObjectManager.Instance.EndedGame();
        }
    }
}