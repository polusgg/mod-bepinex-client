using HarmonyLib;
using PolusGG.Extensions;
using PolusGG.Mods.Patching;
using TMPro;
using UnityEngine;

namespace PolusGG.Patches.Temporary {
    [HarmonyPatch(typeof(TextMeshPro), nameof(TextMeshPro.InternalUpdate))]
    public class ComicSanaes {
        [PermanentPatch]
        [HarmonyPrefix]
        public static void Lmao(TextMeshPro __instance) {
            __instance.color = Color.black;
            __instance.SetOutlineThickness(0.4f);
            __instance.font = PogusPlugin.font;
            // __instance.LoadFontAsset();
        }
    }
}