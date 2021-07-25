using System.Reflection;
using HarmonyLib;
using Polus.Mods.Patching;
using TMPro;

namespace Polus.Patches.Permanent {
    [HarmonyPatch(typeof(VersionShower), nameof(VersionShower.Start))]
    public class StereotypicalClientModderVersionShowerPatch {
        [PermanentPatch]
        [HarmonyPostfix]
        public static void Start(VersionShower __instance) {
            __instance.text.alignment = TextAlignmentOptions.TopLeft;
            __instance.text.text = "<color=#FF7E7E>Among Us</color> " + __instance.text.text;
            __instance.text.text +=
                $"\n<color=#B77EFF>Polus.gg</color> v{Assembly.GetExecutingAssembly().GetName().Version}s";
            __instance.text.text += $"\n<size=75%>{(PogusPlugin.ModManager.AllPatched ? "<color=#0A9D34>Successfully loaded!" : "<color=#FF7E7E>Failed to load!")}</color></size>";
        }
    }
}