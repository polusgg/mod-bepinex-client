using HarmonyLib;
using UnityEngine;

namespace Polus.Patches.Temporary {
    public class HudScrollPatches {
        [HarmonyPatch(typeof(HudManager), nameof(HudManager.Start))]
        public static class HudManagerStartPatch {
            [HarmonyPostfix]
            public static void Postfix(HudManager __instance) {
                BoxCollider2D boxCollider2D = __instance.GameSettings.gameObject.AddComponent<BoxCollider2D>();
                boxCollider2D.size = new Vector2(4, 40);
                Scroller scroller = __instance.GameSettings.gameObject.AddComponent<Scroller>();
                scroller.Colliders = new[] {boxCollider2D};
                scroller.allowY = true;
                scroller.YBounds = new FloatRange(2.9f, 0f);
                scroller.Inner = __instance.GameSettings.transform;

                // __instance.GameSettings.textLinkPrefab = AssetUtilities.GetAsset<TextLink>("TextLink");
            }
        }

        [HarmonyPatch(typeof(HudManager), nameof(HudManager.Update))]
        public static class HudManagerUpdatePatch {
            [HarmonyPostfix]
            public static void Postfix(HudManager __instance) {
                try {
                    if (!AmongUsClient.Instance.IsGameStarted) {
                        Scroller scroller = __instance.GameSettings.GetComponent<Scroller>();
                        if (scroller != null)
                            scroller.YBounds = new FloatRange(2.9f, 2.9f + __instance.GameSettings.renderedHeight);

                        bool wasEnabled = scroller.enabled;
                        scroller.enabled = !CustomPlayerMenu.Instance || Input.GetMouseButton(0) && wasEnabled;
                    }
                } catch {
                    // ignored
                }
            }
        }
    }
}