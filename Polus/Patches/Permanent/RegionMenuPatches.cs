using System;
using System.Linq;
using HarmonyLib;
using Polus.Extensions;
using Polus.Mods.Patching;
using Polus.ServerList;
using UnityEngine.SceneManagement;

namespace Polus.Patches.Permanent {
    [HarmonyPatch(typeof(RegionMenu), nameof(RegionMenu.OnEnable))]
    public static class RegionMenuOnEnablePatch {
        [HarmonyPrefix]
        [PermanentPatch]
        public static void Prefix(RegionMenu __instance) {
            "All regions".Log();
            foreach (IRegionInfo region in ServerManager.Instance.AvailableRegions) {
                region.Name.Log();
            }

            "snoiger llA".Log();
        }

        [HarmonyPostfix]
        [PermanentPatch]
        public static void Postfix(RegionMenu __instance) {
            foreach (PoolableBehavior regionButton in __instance.ButtonPool.activeChildren) {
                ServerListButton button = regionButton.Cast<ServerListButton>();
                button.SetSelected(DestroyableSingleton<ServerManager>.Instance.CurrentRegion.Name == button.Text.text);
                button.Button.OnClick.AddListener((Action) (() => {
                    PggSaveManager.CurrentRegion = (byte) global::Extensions.IndexOf(ServerManager.Instance.AvailableRegions, new Func<IRegionInfo, bool>(region => region.Name == button.Text.text));
                    
                    bool original = PogusPlugin.ModManager.AllPatched;
                    if (!PogusPlugin.ModManager.AllPatched) PogusPlugin.ModManager.PatchMods();

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