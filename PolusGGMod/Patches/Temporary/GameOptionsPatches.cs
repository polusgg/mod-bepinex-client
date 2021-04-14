using System.Collections.Generic;
using HarmonyLib;
using PolusGG.Enums;
using UnityEngine;

namespace PolusGG.Patches.Temporary {
    public static class GameOptionsPatches {
        public static Dictionary<string, GameOption> Options;
        [HarmonyPatch(typeof(GameSettingMenu), nameof(GameSettingMenu.OnEnable))]
        public class OnEnablePatch {
            [HarmonyPrefix]
            public static bool OnEnable(GameSettingMenu __instance) {
                GameObject enumoption = Object.Instantiate(__instance.AllItems[1].gameObject);
                GameObject numoption = Object.Instantiate(__instance.AllItems[1].gameObject);
                GameObject booloption = Object.Instantiate(__instance.AllItems[1].gameObject);
                return true;
            }
        }
    }

    public class GameOption {
        public OptionType Type;
        public object Value;
        public GameOption(OptionType type, object value) {
            Type = type;
            Value = value;
        }
    }
}