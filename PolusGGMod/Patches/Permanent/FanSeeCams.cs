using HarmonyLib;
using PolusGG.Mods.Patching;
using UnityEngine;

namespace PolusGG.Patches.Permanent {
    [HarmonyPatch(typeof(ShadowCamera), nameof(ShadowCamera.OnEnable))]
    public static class ShadowCameraOnEnablePatch {
        [PermanentPatch]
        [HarmonyPostfix]
        public static void Postfix(ShadowCamera __instance) {
            Camera shadowCamera = __instance.GetComponent<Camera>();

            int res = Mathf.Max(Screen.width, Screen.height);
            RenderTexture highResTexture = new(res, res, 0) {antiAliasing = 4};

            shadowCamera.targetTexture = highResTexture;
            shadowCamera.allowMSAA = true;
            GameObject.Find("ShadowQuad").GetComponent<MeshRenderer>().material.mainTexture = highResTexture;
        }
    }

    [HarmonyPatch(typeof(SurveillanceMinigame), nameof(SurveillanceMinigame.Begin))]
    public static class SurveillanceMinigameBeginPatch {
        [PermanentPatch]
        [HarmonyPrefix]
        public static void Prefix() {
            RenderTextureGetTemporaryPatch.Enabled = true;
        }

        [PermanentPatch]
        [HarmonyPostfix]
        public static void Postfix() {
            RenderTextureGetTemporaryPatch.Enabled = false;
        }
    }

    [HarmonyPatch(typeof(RenderTexture), nameof(RenderTexture.GetTemporary), typeof(int), typeof(int), typeof(int),
        typeof(RenderTextureFormat))]
    public static class RenderTextureGetTemporaryPatch {
        public static bool Enabled;

        [PermanentPatch]
        [HarmonyPrefix]
        public static void Prefix([HarmonyArgument(0)] ref int width, [HarmonyArgument(1)] ref int height) {
            if (!Enabled) return;
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