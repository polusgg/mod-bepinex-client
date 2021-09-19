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
            meetingButtonEnabled = true;
            adminTableEnabled = true;
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
        public static bool meetingButtonEnabled = true;
        public static bool adminTableEnabled = true;
        [HarmonyPrefix]
        public static bool Prefix(UseButtonManager __instance, IUsable target) {
            __instance.currentTarget = target;
            __instance.RefreshButtons();
            if (target != null) // Has target? yes
            {
                switch (target.UseIcon) {
                    // Is target a vent? yes
                    case ImageNames.VentButton: {
                        if (ventButtonEnabled) // ventButtonEnabled? yes
                        {
                            DisplayButton(__instance, true, target.UseIcon, target.PercentCool);
                            return false;
                        }
                        // ventButtonEnabled? no
                        // fall through
                        break;
                    }
                    case ImageNames.AdminMapButton:
                    case ImageNames.MIRAAdminButton:
                    case ImageNames.PolusAdminButton:
                    case ImageNames.AirshipAdminButton:
                        if (adminTableEnabled) // adminTableEnabled? yes
                        {
                            DisplayButton(__instance, true, target.UseIcon, target.PercentCool);
                            return false;
                        }
                        break;
                    default: {
                        if (target.TryCast<SystemConsole>() != null &&
                            target.TryCast<SystemConsole>()?.MinigamePrefab.TryCast<EmergencyMinigame>() != null &&
                            !meetingButtonEnabled)
                        {
                            DisplayButton(__instance, false, ImageNames.UseButton);
                        }
                        else
                        {
                            DisplayButton(__instance, useButtonEnabled, target.UseIcon, target.PercentCool);
                        }
                        return false;
                    }
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
                    if (__instance.currentTarget.TryCast<SystemConsole>() != null &&
                        __instance.currentTarget.TryCast<SystemConsole>()?.MinigamePrefab.TryCast<EmergencyMinigame>() != null &&
                        !meetingButtonEnabled)
                    {
                        return false;
                    }
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

    [HarmonyPatch(typeof(SystemConsole), nameof(SystemConsole.SetOutline))]
    public static class MeetingConsoleOutlinePatch {
        [HarmonyPrefix]
        public static bool Prefix(SystemConsole __instance) {
            if (__instance.MinigamePrefab.TryCast<EmergencyMinigame>() != null && !meetingButtonEnabled && __instance.Image)
            {
                __instance.Image.material.SetFloat("_Outline", 0f);
                __instance.Image.material.SetColor("_OutlineColor", Color.white);
                __instance.Image.material.SetColor("_AddColor", Color.clear);
                return false;
            }

            return true;
        }
    }
    
    [HarmonyPatch(typeof(MapConsole), nameof(MapConsole.SetOutline))]
    public static class AdminTableOutlinePatch {
        [HarmonyPrefix]
        public static bool Prefix(MapConsole __instance)
        {
            if (!adminTableEnabled && __instance.Image)
            {
                __instance.Image.material.SetFloat("_Outline", 0f);
                __instance.Image.material.SetColor("_OutlineColor", Color.white);
                __instance.Image.material.SetColor("_AddColor", Color.clear);
                return false;
            }

            return true;
        }
    }

    [HarmonyPatch(typeof(MapConsole), nameof(MapConsole.Use))]
    public static class AdminTableUsePatch
    {
        [HarmonyPrefix]
        public static bool Use(MapConsole __instance)
        {
            if (!adminTableEnabled)
                return false;

            return true;
        }
    }

    [HarmonyPatch(typeof(MapConsole), nameof(MapConsole.CanUse))]
    public static class AdminTableCanUsePatch
    {
        [HarmonyPrefix]
        public static bool CanUse(MapConsole __instance, [HarmonyArgument(1)] out bool canUse, [HarmonyArgument(1)] out bool couldUse)
        {
            canUse = couldUse = false;

            if (!adminTableEnabled && __instance.Image)
                return false;

            return true;
        }
    }

    [HarmonyPatch(typeof(SystemConsole), nameof(SystemConsole.CanUse))]
    public static class MeetingConsoleUsePatch {
        [HarmonyPrefix]
        public static bool CanUse(SystemConsole __instance, [HarmonyArgument(1)] out bool canUse, [HarmonyArgument(1)] out bool couldUse) {
            canUse = couldUse = false;

            if (__instance.MinigamePrefab.TryCast<EmergencyMinigame>() != null && !meetingButtonEnabled && __instance.Image)
                return false;

            return true;
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