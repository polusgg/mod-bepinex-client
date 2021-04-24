using HarmonyLib;
using PolusGG.Mods.Patching;
using TMPro;

namespace PolusGG.Patches.Temporary {
#if DEBUG
    [HarmonyPatch(typeof(TextMeshPro), nameof(TextMeshPro.SetVerticesDirty))]
    public class ComicSanaes {
        [PermanentPatch]
        [HarmonyPrefix]
        public static void Lmao(TextMeshPro __instance) {
            // __instance.outlineColor = Color.black;
            // __instance.SetOutlineThickness(0.1f);
            // __instance.font = PogusPlugin.font;
            // __instance.LoadFontAsset();
        }
    }
#endif
}