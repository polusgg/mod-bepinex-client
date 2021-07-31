using System;
using HarmonyLib;
using Polus.Extensions;
using TMPro;
using UnityEngine;
using static Polus.Patches.Temporary.UseButtonTargetPatch;

namespace Polus.Patches.Temporary
{
    [HarmonyPatch(typeof(HudManager), nameof(HudManager.Start))]
    public static class HudStartPatch {
        [HarmonyPrefix]
        public static void Prefix(HudManager __instance) {
            HudShowMapPatch.doorsEnabled = true;
            HudShowMapPatch.sabotagesEnabled = true;
            sabotageButtonEnabled = true;
            useButtonEnabled = true;
            ventButtonEnabled = true;
            TaskPanelUpdatePatch.enabled = true;
            ReportButtonDisablePatch.enabled = true;
        }
    }

    [HarmonyPatch(typeof(Vent), nameof(Vent.SetOutline))]
    public static class VentDisablePatch {
        [HarmonyPrefix]
        public static void Prefix(Vent __instance, [HarmonyArgument(0)] ref bool on, [HarmonyArgument(1)] ref bool mainTarget) {
            if (on && !ventButtonEnabled)
            {
                on = false;
                mainTarget = false;
            }
        }
    }
    
    [HarmonyPatch(typeof(InfectedOverlay), nameof(InfectedOverlay.FixedUpdate))]
    public static class HudShowMapPatch {
        public static bool doorsEnabled = true;
        public static bool sabotagesEnabled = true;
        [HarmonyPostfix]
        public static void Postfix(InfectedOverlay __instance) {
            foreach (var button in __instance.allButtons) {
                if (button.gameObject.name == "closeDoors" || button.gameObject.name == "Doors") {
                    button.gameObject.SetActive(doorsEnabled);
                }
                else {
                    button.gameObject.SetActive(sabotagesEnabled);
                }
            }
        }
    }

    [HarmonyPatch(typeof(UseButtonManager), nameof(UseButtonManager.SetTarget))]
    public static class UseButtonTargetPatch {
        public static bool ventButtonEnabled = true;
        public static bool sabotageButtonEnabled = true;
        public static bool useButtonEnabled = true;
        [HarmonyPrefix]
        public static bool Prefix(UseButtonManager __instance, IUsable target) {
            __instance.currentTarget = target;
            __instance.RefreshButtons();
            if (target != null) // Has target? yes
            {
                if(target.UseIcon == ImageNames.VentButton) // Is target a vent? yes
                {
                    if (ventButtonEnabled) // ventButtonEnabled? yes
                    {
                        DisplayButton(__instance, true, target.UseIcon, target.PercentCool);
                        return false;
                    }
                    // ventButtonEnabled? no
                    // fall through
                }
                else // Is target a vent? no
                {
                    DisplayButton(__instance, useButtonEnabled, target.UseIcon, target.PercentCool);
                    return false;
                }
            }// Has target? no
            PlayerControl localPlayer = PlayerControl.LocalPlayer;
            if (((localPlayer != null) ? localPlayer.Data : null) != null && PlayerControl.LocalPlayer.Data.IsImpostor && PlayerControl.LocalPlayer.CanMove && sabotageButtonEnabled)
            {
                DisplayButton(__instance, true, ImageNames.SabotageButton);
                return false;
            }
            DisplayButton(__instance, false, ImageNames.UseButton);
            return false;
        }

        public static void DisplayButton(UseButtonManager instance, bool enabled, ImageNames image, float percent = 0f) {
            instance.currentButtonShown = instance.otherButtons[image];
            //CooldownHelpers.SetCooldownNormalizedUvs(instance.currentButtonShown.graphic);
            instance.currentButtonShown.graphic.color = enabled ? UseButtonManager.EnabledColor : UseButtonManager.DisabledColor;
            instance.currentButtonShown.text.color = enabled ? UseButtonManager.EnabledColor : UseButtonManager.DisabledColor;
            instance.currentButtonShown.Show(percent);
        }
    }

    [HarmonyPatch(typeof(UseButtonManager), nameof(UseButtonManager.DoClick))]
    public static class UseButtonClickPatch {
        [HarmonyPrefix]
        public static bool Prefix(UseButtonManager __instance) {
            if (__instance == null) return false;
            if (!__instance.isActiveAndEnabled)
            {
                return false;
            }
            if (!PlayerControl.LocalPlayer)
            {
                return false;
            }
            GameData.PlayerInfo data = PlayerControl.LocalPlayer.Data;
            if (__instance.currentTarget != null)
            {
                if (__instance.currentTarget.UseIcon == ImageNames.VentButton && ventButtonEnabled)
                {
                    PlayerControl.LocalPlayer.UseClosest();
                    return false;
                }

                if (__instance.currentTarget.UseIcon == ImageNames.VentButton && !ventButtonEnabled)
                {
                    HudManager.Instance.ShowMap(new Action<MapBehaviour>(m => m.ShowInfectedMap()));
                    return false;
                }

                if (__instance.currentTarget.UseIcon != ImageNames.VentButton && useButtonEnabled)
                {
                    PlayerControl.LocalPlayer.UseClosest();
                    return false;
                }

                return false;
            }
            if (data != null && data.IsImpostor && sabotageButtonEnabled)
            {
                HudManager.Instance.ShowMap(new Action<MapBehaviour>(m => m.ShowInfectedMap()));
            }
            return false;
        }
    }

    [HarmonyPatch(typeof(TaskPanelBehaviour), nameof(TaskPanelBehaviour.Update))]
    public static class TaskPanelUpdatePatch {
        public static bool enabled = true;
        [HarmonyPrefix]
        public static bool Prefix(TaskPanelBehaviour __instance) {
            __instance.background.enabled = enabled;
            __instance.tab.enabled = enabled;
            __instance.TaskText.enabled = enabled;
            try
            {
                __instance.tab.GetComponentInChildren<TextMeshPro>().enabled = enabled;
            }
            catch (Exception e)
            {
                PogusPlugin.Logger.LogWarning(e);
            }
            return enabled;
        }
    }

    [HarmonyPatch(typeof(ProgressTracker), nameof(ProgressTracker.FixedUpdate))]
    public static class ProgressTrackerUpdatePatch {
        public static bool enabled = true;
        [HarmonyPrefix]
        public static bool Prefix(ProgressTracker __instance) {
            var renderers = __instance.GetComponentsInChildren<MeshRenderer>();
            foreach (var rend in renderers) rend.enabled = enabled;
            var srenderers = __instance.GetComponentsInChildren<SpriteRenderer>();
            foreach (var rend in srenderers) rend.enabled = enabled;

            return enabled;
        }
    }

    [HarmonyPatch(typeof(HudManager), nameof(HudManager.Update))]
    public static class ReportButtonDisablePatch {
        public static bool enabled = true;

        [HarmonyPostfix]
        public static void Postfix(HudManager __instance) {
            if (__instance.ReportButton == null) return;
            if (__instance.ReportButton.gameObject != null && __instance.ReportButton.gameObject.active) __instance.ReportButton.gameObject.SetActive(enabled);
        }
    }
}