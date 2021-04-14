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
                // if (button.Text.Text.Contains("Polus") && PogusPlugin.ModManager.AllPatched) {
                //     button.Text.Text += " (Patched)";
                // }
                PogusPlugin.ModManager.AllPatched.Log(1, "how are you doing today");
                button.Button.OnClick.AddListener((Action) (() => {
                    button.Text.text.Log(1, "i'm not so great rn");
                    PogusPlugin.ModManager.AllPatched.Log(1, "might be because you're reading my code as we speak");
                    bool original = PogusPlugin.ModManager.AllPatched;
                    if (button.Text.text.Contains("Polus")) {
                        if (!PogusPlugin.ModManager.AllPatched) PogusPlugin.ModManager.PatchMods();
                    } else
                        PogusPlugin.ModManager.UnpatchMods();

                    PogusPlugin.Logger.LogInfo(
                        $"IsPatched = {PogusPlugin.ModManager.AllPatched}, original = {original}");

                    if (original != PogusPlugin.ModManager.AllPatched) {
                        //todo might need an update when ported to latest with addressables
                        int sceneId = SceneManager.GetActiveScene().buildIndex;
                        SceneManager.LoadScene(sceneId);
                    }
                }));
            }
        }
    }
}