using HarmonyLib;
using Polus.Mods.Patching;
using TMPro;
using UnityEngine.UI;

namespace Polus.Patches.Temporary {
// #if DEBUG
    [HarmonyPatch(typeof(TextMeshPro), nameof(TextMeshPro.OnEnable))]
    public class ComicSanaes {
        [PermanentPatch]
        [HarmonyPrefix]
        public static void Lmao(TextMeshPro __instance) {
            // if (__instance.GetScriptClassName() != nameof(TextMeshPro)) return;
            // TextMeshPro text = __instance.Cast<TextMeshPro>();
            
            // __instance.outlineColor = Color.black;
            // __instance.SetOutlineThickness(0.1f);
            // __instance.font = PogusPlugin.font;
            // __instance.LoadFontAsset();

            __instance.spriteAsset = PogusPlugin.spriteSheet;
        }
    }
// #endif
}