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
                    bool original = PogusPlugin.AllPatched;
                    if (button.Text.Text == PggConstants.Region.Name) {
                        if (!PogusPlugin.AllPatched)PogusPlugin.PatchMods();//todo implement temporary patches
                    } else PogusPlugin.UnpatchMods();

                    PogusPlugin.Logger.LogInfo($"IsPatched = {PogusPlugin.AllPatched}, original = {original}");

                    if (original != PogusPlugin.AllPatched) {
                        int sceneId = UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex;
                        UnityEngine.SceneManagement.SceneManager.LoadScene(sceneId);
                    }
                }));
            }
        }
    }
}