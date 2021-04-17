using System;
using HarmonyLib;
using PolusGG.Extensions;
using PolusGG.Mods.Patching;
using UnityEngine.SceneManagement;

namespace PolusGG.Patches.Permanent {
    [HarmonyPatch(typeof(RegionMenu), nameof(RegionMenu.OnEnable))]
    public static class RegionMenuOnEnablePatch {
        [HarmonyPostfix]
        [PermanentPatch]
        public static void Postfix(RegionMenu __instance) {
            foreach (PoolableBehavior regionButton in __instance.ButtonPool.activeChildren) {
                ServerListButton button = regionButton.Cast<ServerListButton>();
                button.SetSelected(DestroyableSingleton<ServerManager>.Instance.CurrentRegion.Name == button.Text.text);
                button.Button.OnClick.AddListener((Action) (() => {
                    bool original = PogusPlugin.ModManager.AllPatched;
                    if (button.Text.text.Contains("Polus")) {
                        if (!PogusPlugin.ModManager.AllPatched) PogusPlugin.ModManager.PatchMods();
                    } else
                        PogusPlugin.ModManager.UnpatchMods();

                    PogusPlugin.Logger.LogInfo(
                        $"IsPatched = {PogusPlugin.ModManager.AllPatched}, original = {original}");

                    if (original != PogusPlugin.ModManager.AllPatched) {
                        int sceneId = SceneManager.GetActiveScene().buildIndex;
                        SceneManager.LoadScene(sceneId);
                    }
                }));
            }
        }
    }
}