using System;
using HarmonyLib;
using Polus.Extensions;
using TMPro;
using UnityEngine;

namespace Polus.Patches.Temporary {
    public static class SetHudVisibilityPatches {
        public static bool VentButtonEnabled = true;
        public static bool SabotageButtonEnabled = true;
        public static bool UseButtonEnabled = true;
        public static bool MeetingButtonEnabled = true;
        public static bool AdminTableEnabled = true;
        public static void Reset() {
            HudShowMapPatch.DoorsEnabled = true;
            HudShowMapPatch.SabotagesEnabled = true;
            SabotageButtonEnabled = true;
            UseButtonEnabled = true;
            VentButtonEnabled = true;
            MeetingButtonEnabled = true;
            AdminTableEnabled = true;
            TaskPanelUpdatePatch.Enabled = true;
            ReportButtonDisablePatch.Enabled = true;
        }

        [HarmonyPatch(typeof(Vent), nameof(Vent.SetOutline))]
        public static class VentDisablePatch {
            [HarmonyPrefix]
            public static void Prefix(Vent __instance, [HarmonyArgument(0)] ref bool on, [HarmonyArgument(1)] ref bool mainTarget) {
                if (on && !VentButtonEnabled) {
                    on = false;
                    mainTarget = false;
                }
            }
        }

        [HarmonyPatch(typeof(InfectedOverlay), nameof(InfectedOverlay.FixedUpdate))]
        public static class HudShowMapPatch {
            public static bool DoorsEnabled = true;
            public static bool SabotagesEnabled = true;

            [HarmonyPostfix]
            public static void Postfix(InfectedOverlay __instance) {
                foreach (ButtonBehavior button in __instance.allButtons) {
                    button.gameObject.SetActive(button.gameObject.name is "closeDoors" or "Doors" ? DoorsEnabled : SabotagesEnabled);
                }
            }
        }

        [HarmonyPatch(typeof(UseButtonManager), nameof(UseButtonManager.SetTarget))]
        public static class UseButtonTargetPatch {
            [HarmonyPrefix]
            public static bool Prefix(UseButtonManager __instance, IUsable target) {
                __instance.currentTarget = target;
                __instance.RefreshButtons();
                if (target != null) // Has target? yes
                {
                    switch (target.UseIcon) {
                        // Is target a vent? yes
                        case ImageNames.VentButton: {
                            if (VentButtonEnabled) // ventButtonEnabled? yes
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
                            if (AdminTableEnabled) // adminTableEnabled? yes
                            {
                                DisplayButton(__instance, true, target.UseIcon, target.PercentCool);
                                return false;
                            }

                            break;
                        default: {
                            if (target.TryCast<SystemConsole>() != null &&
                                target.TryCast<SystemConsole>()?.MinigamePrefab.TryCast<EmergencyMinigame>() != null &&
                                !MeetingButtonEnabled) {
                                DisplayButton(__instance, false, ImageNames.UseButton);
                            } else {
                                DisplayButton(__instance, UseButtonEnabled, target.UseIcon, target.PercentCool);
                            }

                            return false;
                        }
                    }
                } // Has target? no

                PlayerControl localPlayer = PlayerControl.LocalPlayer;
                if (((localPlayer != null) ? localPlayer.Data : null) != null && PlayerControl.LocalPlayer.Data.IsImpostor && PlayerControl.LocalPlayer.CanMove && SabotageButtonEnabled) {
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
                if (!__instance.isActiveAndEnabled) {
                    return false;
                }

                if (!PlayerControl.LocalPlayer) {
                    return false;
                }

                GameData.PlayerInfo data = PlayerControl.LocalPlayer.Data;
                if (__instance.currentTarget != null) {
                    if (__instance.currentTarget.UseIcon == ImageNames.VentButton && VentButtonEnabled) {
                        PlayerControl.LocalPlayer.UseClosest();
                        return false;
                    }

                    if (__instance.currentTarget.UseIcon == ImageNames.VentButton && !VentButtonEnabled) {
                        HudManager.Instance.ShowMap(new Action<MapBehaviour>(m => m.ShowInfectedMap()));
                        return false;
                    }

                    if (__instance.currentTarget.UseIcon != ImageNames.VentButton && UseButtonEnabled) {
                        if (__instance.currentTarget.TryCast<SystemConsole>() != null &&
                            __instance.currentTarget.TryCast<SystemConsole>()?.MinigamePrefab.TryCast<EmergencyMinigame>() != null &&
                            !MeetingButtonEnabled) {
                            return false;
                        }

                        PlayerControl.LocalPlayer.UseClosest();
                        return false;
                    }

                    return false;
                }

                if (data != null && data.IsImpostor && SabotageButtonEnabled) {
                    HudManager.Instance.ShowMap(new Action<MapBehaviour>(m => m.ShowInfectedMap()));
                }

                return false;
            }
        }

        [HarmonyPatch(typeof(SystemConsole), nameof(SystemConsole.SetOutline))]
        public static class MeetingConsoleOutlinePatch {
            [HarmonyPrefix]
            public static bool Prefix(SystemConsole __instance) {
                if (__instance.MinigamePrefab.TryCast<EmergencyMinigame>() != null && !MeetingButtonEnabled && __instance.Image) {
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
            public static bool Prefix(MapConsole __instance) {
                if (!AdminTableEnabled && __instance.Image) {
                    __instance.Image.material.SetFloat("_Outline", 0f);
                    __instance.Image.material.SetColor("_OutlineColor", Color.white);
                    __instance.Image.material.SetColor("_AddColor", Color.clear);
                    return false;
                }

                return true;
            }
        }

        [HarmonyPatch(typeof(MapConsole), nameof(MapConsole.Use))]
        public static class AdminTableUsePatch {
            [HarmonyPrefix]
            public static bool Use(MapConsole __instance) {
                return AdminTableEnabled;
            }
        }

        [HarmonyPatch(typeof(MapConsole), nameof(MapConsole.CanUse))]
        public static class AdminTableCanUsePatch {
            [HarmonyPrefix]
            public static bool CanUse(MapConsole __instance, [HarmonyArgument(1)] out bool canUse, [HarmonyArgument(1)] out bool couldUse) {
                canUse = couldUse = false;

                return AdminTableEnabled || !__instance.Image;
            }
        }

        [HarmonyPatch(typeof(SystemConsole), nameof(SystemConsole.CanUse))]
        public static class MeetingConsoleUsePatch {
            [HarmonyPrefix]
            public static bool CanUse(SystemConsole __instance, [HarmonyArgument(1)] out bool canUse, [HarmonyArgument(1)] out bool couldUse) {
                canUse = couldUse = false;

                return __instance.MinigamePrefab.TryCast<EmergencyMinigame>() == null || MeetingButtonEnabled || !__instance.Image;
            }
        }

        [HarmonyPatch(typeof(Vent), nameof(Vent.Use))]
        public static class VentUsePatch {
            [HarmonyPrefix]
            public static bool Use(Vent __instance) {
                return VentButtonEnabled;
            }
        }

        [HarmonyPatch(typeof(TaskPanelBehaviour), nameof(TaskPanelBehaviour.Update))]
        public static class TaskPanelUpdatePatch {
            public static bool Enabled = true;

            [HarmonyPrefix]
            public static bool Prefix(TaskPanelBehaviour __instance) {
                __instance.background.enabled = Enabled;
                __instance.tab.enabled = Enabled;
                __instance.TaskText.enabled = Enabled;
                try {
                    __instance.tab.GetComponentInChildren<TextMeshPro>().enabled = Enabled;
                } catch (Exception e) {
                    PogusPlugin.Logger.LogWarning(e);
                }

                return Enabled;
            }
        }

        [HarmonyPatch(typeof(ProgressTracker), nameof(ProgressTracker.FixedUpdate))]
        public static class ProgressTrackerUpdatePatch {
            public static bool Enabled = true;

            [HarmonyPrefix]
            public static bool Prefix(ProgressTracker __instance) {
                var renderers = __instance.GetComponentsInChildren<MeshRenderer>();
                foreach (var rend in renderers) rend.enabled = Enabled;
                var srenderers = __instance.GetComponentsInChildren<SpriteRenderer>();
                foreach (var rend in srenderers) rend.enabled = Enabled;

                return Enabled;
            }
        }

        [HarmonyPatch(typeof(HudManager), nameof(HudManager.Update))]
        public static class ReportButtonDisablePatch {
            public static bool Enabled = true;

            [HarmonyPostfix]
            public static void Postfix(HudManager __instance) {
                if (__instance.ReportButton == null) return;
                if (__instance.ReportButton.gameObject != null && __instance.ReportButton.gameObject.active) __instance.ReportButton.gameObject.SetActive(Enabled);
            }
        }
    }
}