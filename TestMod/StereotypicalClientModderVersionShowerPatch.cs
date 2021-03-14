﻿using HarmonyLib;

namespace PolusGGMod.Patches {
    [HarmonyPatch(typeof(VersionShower), nameof(VersionShower.Start))]
    public class StereotypicalClientModderVersionShowerPatch {
        [HarmonyPostfix]
        public static void Start(VersionShower __instance) {
            __instance.text.Text += "\n\n  [A80100FF]Sus mod v2[] ([0007AAFF]by Impsustor32[])";
        }
    }
}