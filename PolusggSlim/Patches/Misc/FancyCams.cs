using HarmonyLib;
using UnityEngine;

namespace PolusggSlim.Patches.Misc
{
    public static class FancyCams
    {
        private static bool Enabled;
        
        [HarmonyPatch(typeof(SurveillanceMinigame), nameof(SurveillanceMinigame.Begin))]
        public static class SurveillanceMinigame_Begin {
            public static void Prefix() => Enabled = true;

            public static void Postfix() => Enabled = false;
        }

        [HarmonyPatch(typeof(RenderTexture), 
           nameof(RenderTexture.GetTemporary), 
           typeof(int), typeof(int), typeof(int), typeof(RenderTextureFormat))]
        public static class RenderTexture_GetTemporary {
            public static void Prefix([HarmonyArgument(0)] ref int width, [HarmonyArgument(1)] ref int height) {
                if (!Enabled) 
                    return;
                
                if (Screen.width > Screen.height) {
                    height = (int) (height / (float) width * Screen.width);
                    width = Screen.width;
                } else {
                    height = Screen.height;
                    width = (int) (width / (float) height * Screen.height);
                }
            }
        }
    }
}