using System;
using System.Linq;
using HarmonyLib;

namespace PolusGGMod.Patches {
    [HarmonyPatch(typeof(RegionMenu), nameof(RegionMenu.OnEnable))]
    public static class RegionMenuOnEnablePatch {
        [HarmonyPrefix]
        public static void Prefix() {
            bool hasModded = ServerManager.DefaultRegions.Any(x => x.Name == PggConstants.Region.Name);
            if (!hasModded) {
                ServerManager.DefaultRegions = ServerManager.DefaultRegions.Append(PggConstants.Region).ToArray();
            }
        }

        [HarmonyPostfix]
        [PermanentPatch]
        public static void Postfix(RegionMenu __instance) {
            foreach (PoolableBehavior regionButton in __instance.ButtonPool.activeChildren) {
                ChatLanguageButton button = regionButton.Cast<ChatLanguageButton>();
                button.SetSelected(DestroyableSingleton<ServerManager>.Instance.CurrentRegion.Name == button.Text.Text);
                button.Button.OnClick.AddListener((Action) (() => {
                    bool original = PggMod.IsPatched;
                    if (button.Text.Text == PggConstants.Region.Name && !PggMod.IsPatched) {
                        // PggMod.();//todo implement temporary patches
                    } // else PggMod.Unpatch();

                    PogusPlugin.Logger.LogInfo($"IsPatched = {PggMod.IsPatched}, original = {original}");

                    if (original != PggMod.IsPatched) {
                        int sceneId = UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex;
                        UnityEngine.SceneManagement.SceneManager.LoadScene(sceneId);
                    }
                }));
            }
        }
    }
}