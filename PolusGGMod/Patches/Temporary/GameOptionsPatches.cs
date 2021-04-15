using System;
using System.Collections.Generic;
using HarmonyLib;
using PolusGG.Enums;
using UnhollowerBaseLib;
using UnityEngine;
using Object = UnityEngine.Object;

namespace PolusGG.Patches.Temporary {
    public static class GameOptionsPatches {
        public static Dictionary<string, GameOption> Options = new();
        [HarmonyPatch(typeof(GameSettingMenu), nameof(GameSettingMenu.OnEnable))]
        public class OnEnablePatch {
            [HarmonyPrefix]
            public static bool OnEnable(GameSettingMenu __instance) {
                KeyValueOption enumOption = Object.Instantiate(__instance.AllItems[1].gameObject, __instance.transform).GetComponent<KeyValueOption>();
                NumberOption numbOption = Object.Instantiate(__instance.AllItems[2].gameObject, __instance.transform).GetComponent<NumberOption>();
                ToggleOption boolOption = Object.Instantiate(__instance.AllItems[3].gameObject, __instance.transform).GetComponent<ToggleOption>();
                List<OptionBehaviour> optionBehaviours;
                foreach ((string title, GameOption gameOption) in Options) {
                    switch (gameOption.Type) {
                        case OptionType.Number:
                            numbOption.Increment = ;
                            numbOption.TitleText.text = title;
                            break;
                        case OptionType.Boolean:
                            break;
                        case OptionType.Enum:
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                }
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

    public interface OptionValue {}

    public class NumberValue : OptionValue {
        public 
    }
}