using System;
using System.Collections.Generic;
using HarmonyLib;
using Hazel;
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
        public OptionType Type { get; set; }
        public IGameOptionValue Value { get; set; }
    }

    public interface IGameOptionValue {}

    public class BooleanValue : IGameOptionValue
    {
        public BooleanValue(bool value)
        {
            Value = value;
        }

        public bool Value { get; }
    }

    public class EnumValue : IGameOptionValue
    {
        public EnumValue(uint optionIndex, string[] values)
        {
            OptionIndex = optionIndex;
            Values = values;
        }

        public uint OptionIndex { get; }
        public string[] Values { get; }

        public static EnumValue ConstructEnumValue(uint optionIndex, uint stringLength, MessageReader reader)
        {
            List<string> strings = new List<string>();
            for (int i = 0; i < stringLength; i++)
            {
                strings.Add(reader.ReadString());
            }

            return new EnumValue(optionIndex, strings.ToArray());
        }
    }

    public class FloatValue : IGameOptionValue
    {
        public FloatValue(float value, float step, float upper, float lower)
        {
            Value = value;
            Step = step;
            Upper = upper;
            Lower = lower;
        }

        public float Value { get; }
        public float Step { get; }
        public float Upper { get; }
        public float Lower { get; }
    }

    public interface OptionValue {}

    public class NumberValue : OptionValue {
        public 
    }
}