using HarmonyLib;

namespace PolusGG.Patches.Temporary {
    [HarmonyPatch(typeof(VersionShower), nameof(VersionShower.Start))]
    public class StereotypicalClientModderVersionShowerPatch {
        [HarmonyPostfix]
        public static void Start(VersionShower __instance) {
            #if DEBUG
            __instance.text.Text += "\n\n  [A80100FF]Sus mod v69[] ([0007AAFF]by Impsustor32[])";
            #else
            __instance.text.Text += "\n\n  [A80100FF]Polus.gg Loaded[]";
            #endif
        }
    }
}