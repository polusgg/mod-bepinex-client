﻿using System.Linq;
using HarmonyLib;
using Polus.Behaviours.Inner;
using Polus.Enums;

namespace Polus.Patches.Temporary {
    [HarmonyPatch(typeof(HudManager), nameof(HudManager.SetHudActive))]
    public class PnoButtonVisibilityPatch {
        [HarmonyPostfix]
        public static void SetHudActive([HarmonyArgument(0)] bool active) {
            HudManager.Instance.KillButton.gameObject.SetActive(false);
            PolusClickBehaviour.SetLock(ButtonLocks.SetHudActive, !active);
        }
    }
}