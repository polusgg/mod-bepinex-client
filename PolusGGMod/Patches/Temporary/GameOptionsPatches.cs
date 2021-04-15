using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using Hazel;
using PolusGG.Enums;
using PolusGG.Extensions;
using UnhollowerBaseLib;
using UnityEngine;
using Object = UnityEngine.Object;

namespace PolusGG.Patches.Temporary {
    public static class GameOptionsPatches {
        public static Dictionary<string, GameOption> Options = new();
        private static KeyValueOption enumOption;
        private static NumberOption numbOption;
        private static ToggleOption boolOption;

        [HarmonyPatch(typeof(GameSettingMenu), nameof(GameSettingMenu.OnEnable))]
        public class OnEnablePatch {
            [HarmonyPrefix]
            public static bool OnEnable(GameSettingMenu __instance) {
                if (enumOption == null) {
                    enumOption = Object.Instantiate(__instance.AllItems[1].gameObject, __instance.transform)
                        .GetComponent<KeyValueOption>().DontDestroy();
                }

                if (numbOption == null) {
                    numbOption = Object.Instantiate(__instance.AllItems[2].gameObject, __instance.transform)
                        .GetComponent<NumberOption>().DontDestroy();
                }

                if (boolOption == null) {
                    boolOption = Object.Instantiate(__instance.AllItems[3].gameObject, __instance.transform)
                        .GetComponent<ToggleOption>().DontDestroy();
                }

                List<OptionBehaviour> optionBehaviours = new();
                foreach ((string title, GameOption gameOption) in Options) {
                    switch (gameOption.Type) {
                        case OptionType.Number: {
                            FloatValue value = (FloatValue) gameOption.Value;
                            NumberOption option =
                                Object.Instantiate(numbOption, new Vector3(0, 0, -10), new Quaternion());
                            option.Increment = value.Step;
                            option.ValidRange = new FloatRange(value.Lower, value.Upper);
                            option.Value = value.Value;
                            option.TitleText.text = title;
                            optionBehaviours.Add(option);
                            break;
                        }
                        case OptionType.Boolean: {
                            BooleanValue value = (BooleanValue) gameOption.Value;
                            ToggleOption option =
                                Object.Instantiate(boolOption, new Vector3(0, 0, -10), new Quaternion());
                            option.CheckMark.enabled = value.Value;
                            option.TitleText.text = title;
                            optionBehaviours.Add(option);
                            break;
                        }
                        case OptionType.Enum: {
                            EnumValue value = (EnumValue) gameOption.Value;
                            KeyValueOption option =
                                Object.Instantiate(enumOption, new Vector3(0, 0, -10), new Quaternion());
                            option.Values =
                                new Il2CppSystem.Collections.Generic.List<
                                    Il2CppSystem.Collections.Generic.KeyValuePair<string, int>>();
                            int i = 0;
                            foreach (string valueValue in value.Values)
                                option.Values.Add(
                                    new Il2CppSystem.Collections.Generic.KeyValuePair<string, int>(valueValue, i++));
                            option.Selected = i;
                            option.ValueText.text = value.Values[value.OptionIndex];
                            option.TitleText.text = title;
                            optionBehaviours.Add(option);
                            break;
                        }
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                }

                __instance.HideForOnline = new Il2CppReferenceArray<Transform>(0);
                foreach (Transform transforms in __instance.AllItems) {
                    Object.Destroy(transforms.gameObject);
                }
                __instance.AllItems =
                    new Il2CppReferenceArray<Transform>(optionBehaviours.Select(x => x.transform).ToArray());

                return true;
            }
        }
    }

    public class GameOption {
        public OptionType Type { get; set; }
        public IGameOptionValue Value { get; set; }
    }

    public interface IGameOptionValue { }

    public class BooleanValue : IGameOptionValue {
        public BooleanValue(bool value) {
            Value = value;
        }

        public bool Value { get; }
    }

    public class EnumValue : IGameOptionValue {
        public EnumValue(uint optionIndex, string[] values) {
            OptionIndex = optionIndex;
            Values = values;
        }

        public uint OptionIndex { get; }
        public string[] Values { get; }

        public static EnumValue ConstructEnumValue(uint optionIndex, uint stringLength, MessageReader reader) {
            List<string> strings = new();
            for (int i = 0; i < stringLength; i++) {
                strings.Add(reader.ReadString());
            }

            return new EnumValue(optionIndex, strings.ToArray());
        }
    }

    public class FloatValue : IGameOptionValue {
        public FloatValue(float value, float step, float upper, float lower) {
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
}