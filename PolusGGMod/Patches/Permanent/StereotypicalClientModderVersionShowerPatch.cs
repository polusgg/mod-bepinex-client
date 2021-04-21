using System.Reflection;
using HarmonyLib;
using PolusGG.Mods.Patching;
using TMPro;

namespace PolusGG.Patches.Permanent {
    [HarmonyPatch(typeof(VersionShower), nameof(VersionShower.Start))]
    public class StereotypicalClientModderVersionShowerPatch {
        [PermanentPatch]
        [HarmonyPostfix]
        public static void Start(VersionShower __instance) {
            __instance.text.alignment = TextAlignmentOptions.TopLeft;
            __instance.text.text = "<color=#FF7E7E>Among Us</color> " + __instance.text.text;
            __instance.text.text +=
                $"\n<color=#B77EFF>Polus.gg</color> v{Assembly.GetExecutingAssembly().GetName().Version}s ({(PogusPlugin.ModManager.AllPatched ? "<color=#0A9D34>✔" : "<color=#FF7E7E>✖")}</color>)";
        }
    }
}