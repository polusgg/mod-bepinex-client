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
                    enumOption.gameObject.active = false;
                    enumOption.name = "EnumOptionPrefab";
                }

                if (numbOption == null) {
                    numbOption = Object.Instantiate(__instance.AllItems[2].gameObject, __instance.transform)
                        .GetComponent<NumberOption>().DontDestroy();
                    numbOption.gameObject.active = false;
                    numbOption.name = "NumberOptionPrefab";
                }

                if (boolOption == null) {
                    boolOption = Object.Instantiate(__instance.AllItems[3].gameObject, __instance.transform)
                        .GetComponent<ToggleOption>().DontDestroy();
                    boolOption.gameObject.active = false;
                    boolOption.name = "BooleanOptionPrefab";
                }

                List<OptionBehaviour> optionBehaviours = new();
                foreach ((string title, GameOption gameOption) in Options) {
                    switch (gameOption.Type) {
                        case OptionType.Number: {
                            FloatValue value = (FloatValue) gameOption.Value;
                            NumberOption option =
                                Object.Instantiate(numbOption, new Vector3(0, 0, -10), new Quaternion());
                            option.transform.parent = numbOption.transform.parent;
                            option.name = title;
                            option.Increment = value.Step;
                            option.ValidRange = new FloatRange(value.Lower, value.Upper);
                            option.Value = value.Value;
                            option.TitleText.text = title;
                            option.OnValueChanged = new Action<OptionBehaviour>(HandleNumberChanged);
                            optionBehaviours.Add(option);
                            break;
                        }
                        case OptionType.Boolean: {
                            BooleanValue value = (BooleanValue) gameOption.Value;
                            ToggleOption option =
                                Object.Instantiate(boolOption, new Vector3(0, 0, -10), new Quaternion());
                            option.transform.parent = boolOption.transform.parent;
                            option.name = title;
                            option.CheckMark.enabled = value.Value;
                            option.TitleText.text = title;
                            option.OnValueChanged = new Action<OptionBehaviour>(HandleToggleChanged);
                            optionBehaviours.Add(option);
                            break;
                        }
                        case OptionType.Enum: {
                            EnumValue value = (EnumValue) gameOption.Value;
                            KeyValueOption option =
                                Object.Instantiate(enumOption, new Vector3(0, 0, -10), new Quaternion());
                            option.transform.parent = enumOption.transform.parent;
                            option.name = title;
                            option.Values =
                            new Il2CppSystem.Collections.Generic.List<
                            Il2CppSystem.Collections.Generic.KeyValuePair<string, int>>();
                            for (var i = 0; i < value.Values.Length; i++) {
                                option.Values.Add(null);
                            }
                            option.Selected = (int) value.OptionIndex;
                            option.ValueText.text = value.Values[value.OptionIndex];
                            option.TitleText.text = title;
                            option.OnValueChanged = new Action<OptionBehaviour>(HandleStringChanged);
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

                int index = 0;
                foreach (Transform transforms in __instance.AllItems) {
                    transforms.localPosition = new Vector3(0, __instance.YStart - index++ * __instance.YOffset, -1);
                    transforms.gameObject.SetActive(true);
                }

                return true;
            }

            public static void HandleToggleChanged(OptionBehaviour toggleBehaviour) {
                MessageWriter writer = MessageWriter.Get(SendOption.Reliable);
                writer.StartMessage((byte) PolusRootPackets.SetGameOption);
                ToggleOption toggle = toggleBehaviour.Cast<ToggleOption>();
                writer.Write(toggle.TitleText.text);
                writer.Write(toggle.CheckMark.enabled);
                PolusMod.EndSend(writer);
            }

            public static void HandleNumberChanged(OptionBehaviour toggleBehaviour) {
                MessageWriter writer = MessageWriter.Get(SendOption.Reliable);
                writer.StartMessage((byte) PolusRootPackets.SetGameOption);
                NumberOption toggle = toggleBehaviour.Cast<NumberOption>();
                writer.Write(toggle.TitleText.text);
                writer.Write(toggle.Value);
                // these ones are for the fans (not used on server, just to avoid server crashes)
                writer.Write(toggle.Increment);
                writer.Write(toggle.ValidRange.min);
                writer.Write(toggle.ValidRange.max);
                PolusMod.EndSend(writer);
            }

            public static void HandleStringChanged(OptionBehaviour toggleBehaviour) {
                MessageWriter writer = MessageWriter.Get(SendOption.Reliable);
                writer.StartMessage((byte) PolusRootPackets.SetGameOption);
                KeyValueOption toggle = toggleBehaviour.Cast<KeyValueOption>();
                writer.Write(toggle.TitleText.text);
                writer.Write(toggle.Values[(Index) toggle.Selected]
                    .Cast<Il2CppSystem.Collections.Generic.KeyValuePair<string, int>>().Key);
                foreach (Il2CppSystem.Collections.Generic.KeyValuePair<string, int> keyValuePair in toggle.Values) {
                    writer.Write(keyValuePair.Key);
                }

                PolusMod.EndSend(writer);
            }
        }

        [HarmonyPatch(typeof(ToggleOption), nameof(ToggleOption.OnEnable))]
        public class ToggleButtonDisableStartPatch {
            [HarmonyPrefix]
            public static bool Prefix() {
                return false;
            }
        }

        [HarmonyPatch(typeof(NumberOption), nameof(NumberOption.OnEnable))]
        public class NumberButtonDisableStartPatch {
            [HarmonyPrefix]
            public static bool Prefix() {
                return false;
            }
        }

        [HarmonyPatch(typeof(KeyValueOption), nameof(KeyValueOption.OnEnable))]
        public class StringButtonDisableStartPatch {
            [HarmonyPrefix]
            public static bool Prefix() {
                return false;
            }
        }

        [HarmonyPatch(typeof(KeyValueOption), nameof(KeyValueOption.FixedUpdate))]
        public class StringButtonUpdatePatch {
            [HarmonyPrefix]
            public static bool Prefix() {
                return false;
            }
        }

        [HarmonyPatch(typeof(KeyValueOption), nameof(KeyValueOption.Increase))]
        public class StringButtonIncreasePatch {
            [HarmonyPrefix]
            public static bool Prefix() {
                return false;
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

        public static EnumValue ConstructEnumValue(MessageReader reader) {
            List<string> strings = new();
            uint current = reader.ReadPackedUInt32();
            while (reader.Position < reader.Length) {
                strings.Add(reader.ReadString());
            }

            return new EnumValue(current, strings.ToArray());
        }
    }

    public class FloatValue : IGameOptionValue {
        public FloatValue(float value, float step, float lower, float upper) {
            Value = value;
            Step = step;
            Lower = lower;
            Upper = upper;
        }

        public float Value { get; }
        public float Step { get; }
        public float Lower { get; }
        public float Upper { get; }
    }
}