using System;
using System.Linq;
using HarmonyLib;

namespace PolusGGMod.Patches {
    [HarmonyPatch(typeof(RegionMenu), nameof(RegionMenu.OnEnable))]
    public static class RegionMenuOnEnablePatch {
        [HarmonyPostfix]
        [PermanentPatch]
        public static void Postfix(RegionMenu __instance) {
            foreach (PoolableBehavior regionButton in __instance.ButtonPool.activeChildren) {
                ChatLanguageButton button = regionButton.Cast<ChatLanguageButton>();
                button.SetSelected(DestroyableSingleton<ServerManager>.Instance.CurrentRegion.Name == button.Text.Text);
                button.Button.OnClick.AddListener((Action) (() => {
                    bool original = PogusPlugin.ModManager.AllPatched;
                    if (button.Text.Text == PggConstants.Region.Name) {
                        if (!PogusPlugin.ModManager.AllPatched) {
                            // PogusPlugin.ModManager.LoadMods();
                            PogusPlugin.ModManager.PatchMods(); //todo implement temporary patches
                        }
                    } else {
                        PogusPlugin.ModManager.UnpatchMods();
                        // PogusPlugin.ModManager.UnloadMods();
                    }

                    PogusPlugin.Logger.LogInfo(
                        $"IsPatched = {PogusPlugin.ModManager.AllPatched}, original = {original}");

                    if (original != PogusPlugin.ModManager.AllPatched) {
                        //todo might need an update when ported to latest with addressables
                        int sceneId = UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex;
                        UnityEngine.SceneManagement.SceneManager.LoadScene(sceneId);
                    }
                }));
            }
        }
    }
}