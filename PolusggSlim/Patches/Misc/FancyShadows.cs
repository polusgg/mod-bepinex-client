using System;
using UnityEngine;

namespace PolusggSlim.Patches.Misc
{
    public static class FancyShadows
    {
        // [HarmonyPatch(typeof(ShadowCamera), nameof(ShadowCamera.OnEnable))]
        public static class ShadowCamera_OnEnable
        {
            public static void Postfix(ShadowCamera __instance)
            {
                var camera = __instance.GetComponent<Camera>();

                var maxBound = Math.Max(Screen.width, Screen.height);
                var renderTex = new RenderTexture(maxBound, maxBound, 0)
                {
                    antiAliasing = 4
                };

                camera.targetTexture = renderTex;
                camera.allowMSAA = true;

                var shadowRend = DestroyableSingleton<HudManager>.Instance.ShadowQuad.GetComponent<MeshRenderer>();
                shadowRend.material.mainTexture = renderTex;
            }
        }
    }
}