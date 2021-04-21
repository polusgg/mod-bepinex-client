using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using HarmonyLib;
using Hazel;
using PolusGG.Enums;
using PolusGG.Extensions;
using TMPro;
using UnhollowerBaseLib;
using UnityEngine;
using Object = UnityEngine.Object;

namespace PolusGG.Patches.Temporary {
    public static class GameOptionsPatches {
        public static Dictionary<string, List<GameOption>> Categories = new();
        public static Dictionary<string, GameOption> OptionMap = new();

        private static TextMeshPro groupTitle;
        private static KeyValueOption enumOption;
        private static NumberOption numbOption;
        private static ToggleOption boolOption;

        public static void UpdateHudString() {
            HudManager.Instance.GameSettings.text = PlayerControl.GameOptions.ToHudString(69);
        }

        [HarmonyPatch(typeof(GameSettingMenu), nameof(GameSettingMenu.OnEnable))]
        public class OnEnablePatch {
            [HarmonyPrefix]
            public static bool OnEnable(GameSettingMenu __instance) {
                if (!CustomPlayerMenu.Instance || CustomPlayerMenu.Instance.selectedTab != 4) return true;
                if (groupTitle == null) {
                    groupTitle = Object.Instantiate(__instance.AllItems[3].GetComponentInChildren<TextMeshPro>(), __instance.transform)
                        .DontDestroy();
                    groupTitle.gameObject.active = false;
                    groupTitle.name = "CategoryTitlePrefab";
                }

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

                List<Transform> options = new();
                foreach ((string categoryTitle, List<GameOption> gameOptions) in Categories) {
                    if (categoryTitle != "") {
                        TextMeshPro newTitle = Object.Instantiate(groupTitle, new Vector3(0, 0, -10), new Quaternion());
                        newTitle.transform.parent = groupTitle.transform.parent;
                        newTitle.text = categoryTitle;
                        options.Add(newTitle.transform);
                    }

                    foreach (GameOption gameOption in gameOptions) {
                        switch (gameOption.Type) {
                            case OptionType.Number: {
                                FloatValue value = (FloatValue) gameOption.Value;
                                NumberOption option =
                                    Object.Instantiate(numbOption, new Vector3(0, 0, -10), new Quaternion());
                                option.transform.parent = numbOption.transform.parent;
                                option.name = gameOption.Title;
                                option.Increment = value.Step;
                                option.ValidRange = new FloatRange(value.Lower, value.Upper);
                                option.Value = value.Value;
                                option.TitleText.text = gameOption.Title;
                                option.FormatString = value.FormatString;
                                option.ZeroIsInfinity = value.IsInfinity;
                                option.OnValueChanged = new Action<OptionBehaviour>(HandleNumberChanged);
                                options.Add(option.transform);
                                break;
                            }
                            case OptionType.Boolean: {
                                BooleanValue value = (BooleanValue) gameOption.Value;
                                ToggleOption option =
                                    Object.Instantiate(boolOption, new Vector3(0, 0, -10), new Quaternion());
                                option.transform.parent = boolOption.transform.parent;
                                option.name = gameOption.Title;
                                option.CheckMark.enabled = value.Value;
                                option.TitleText.text = gameOption.Title;
                                option.OnValueChanged = new Action<OptionBehaviour>(HandleToggleChanged);
                                options.Add(option.transform);
                                break;
                            }
                            case OptionType.Enum: {
                                EnumValue value = (EnumValue) gameOption.Value;
                                KeyValueOption option =
                                    Object.Instantiate(enumOption, new Vector3(0, 0, -10), new Quaternion());
                                option.transform.parent = enumOption.transform.parent;
                                option.name = gameOption.Title;
                                option.Values =
                                    new Il2CppSystem.Collections.Generic.List<
                                        Il2CppSystem.Collections.Generic.KeyValuePair<string, int>>();
                                for (int i = 0; i < value.Values.Length; i++)
                                    option.Values.Add(new Il2CppSystem.Collections.Generic.KeyValuePair<string, int> {
                                        key = value.Values[i],
                                        value = i
                                    });

                                option.Selected = (int) value.OptionIndex;
                                option.ValueText.text = value.Values[value.OptionIndex];
                                option.TitleText.text = gameOption.Title;
                                option.OnValueChanged = new Action<OptionBehaviour>(HandleStringChanged);
                                options.Add(option.transform);
                                break;
                            }
                            default:
                                throw new ArgumentOutOfRangeException();
                        }
                    }
                }


                __instance.HideForOnline = new Il2CppReferenceArray<Transform>(0);
                foreach (Transform transforms in __instance.AllItems) Object.Destroy(transforms.gameObject);

                __instance.AllItems = options.ToArray();

                int index = 0;
                foreach (Transform transforms in __instance.AllItems) {
                    transforms.localPosition = new Vector3(0, __instance.YStart - index++ * __instance.YOffset, -1);
                    transforms.gameObject.SetActive(true);
                }

                __instance.GetComponent<Scroller>().YBounds.max =
                    index * __instance.YOffset - 2f * __instance.YStart - 0.1f;

                return true;
            }

            public static void HandleToggleChanged(OptionBehaviour toggleBehaviour) {
                UpdateHudString();
                ToggleOption toggle = toggleBehaviour.Cast<ToggleOption>();
                BooleanValue value = (BooleanValue) OptionMap[toggle.TitleText.text].Value;
                value.Value = toggle.CheckMark.enabled;
                MessageWriter writer = MessageWriter.Get(SendOption.Reliable);
                writer.StartMessage((byte) PolusRootPackets.SetGameOption);
                writer.Write(toggle.TitleText.text);
                writer.Write((byte) 1);
                writer.Write(toggle.CheckMark.enabled);
                PolusMod.EndSend(writer);
            }

            public static void HandleNumberChanged(OptionBehaviour toggleBehaviour) {
                UpdateHudString();
                NumberOption toggle = toggleBehaviour.Cast<NumberOption>();
                FloatValue value = (FloatValue) OptionMap[toggle.TitleText.text].Value;
                value.Value = (uint) toggle.Value;
                MessageWriter writer = MessageWriter.Get(SendOption.Reliable);
                writer.StartMessage((byte) PolusRootPackets.SetGameOption);
                writer.Write(toggle.TitleText.text);
                writer.Write((byte) 0);
                writer.Write(toggle.Value);
                // just for the fans (not used on server, just to avoid server crashes)
                writer.Write(toggle.Increment);
                writer.Write(toggle.ValidRange.min);
                writer.Write(toggle.ValidRange.max);
                writer.Write(toggle.ZeroIsInfinity);
                writer.Write(toggle.FormatString);
                PolusMod.EndSend(writer);
            }

            public static void HandleStringChanged(OptionBehaviour toggleBehaviour) {
                UpdateHudString();
                KeyValueOption toggle = toggleBehaviour.Cast<KeyValueOption>();
                EnumValue value = (EnumValue) OptionMap[toggle.TitleText.text].Value;
                value.OptionIndex = (uint) toggle.Selected;
                MessageWriter writer = MessageWriter.Get(SendOption.Reliable);
                writer.StartMessage((byte) PolusRootPackets.SetGameOption);
                writer.Write(toggle.TitleText.text);
                writer.Write((byte) OptionType.Enum);
                writer.WritePacked(toggle.Selected);
                // just for the fans (not used on server, just to avoid server crashes)
                foreach (Il2CppSystem.Collections.Generic.KeyValuePair<string, int> keyValuePair in toggle.Values)
                    writer.Write(keyValuePair.key);

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

        [HarmonyPatch(typeof(NumberOption), nameof(NumberOption.FixedUpdate))]
        public class NumberButtonFixedUpdatePatch {
            [HarmonyPrefix]
            public static bool Prefix(NumberOption __instance) {
                if (Math.Abs(__instance.oldValue - __instance.Value) > 0.001f) {
                    __instance.oldValue = __instance.Value;
                    __instance.ValueText.text = string.Format(__instance.FormatString, __instance.Value);
                }

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
            public static bool Prefix(KeyValueOption __instance) {
                if (__instance.oldValue != __instance.Selected) {
                    __instance.oldValue = __instance.Selected;
                    __instance.ValueText.text =
                        ((EnumValue) OptionMap[__instance.TitleText.text].Value).Values[
                            __instance.Selected];
                }

                return false;
            }
        }

        [HarmonyPatch(typeof(KeyValueOption), nameof(KeyValueOption.GetInt))]
        public class StringButtonIncreasePatch {
            [HarmonyPrefix]
            public static bool Prefix(KeyValueOption __instance, out int __result) {
                __result = 0;
                return false;
            }
        }
    }

    public class GameOption {
        public string Title { get; set; }
        public OptionType Type { get; set; }
        public IGameOptionValue Value { get; set; }
    }

    public interface IGameOptionValue { }

    public class BooleanValue : IGameOptionValue {
        public bool Value;

        public BooleanValue(bool value) {
            Value = value;
        }
    }

    public class EnumValue : IGameOptionValue {
        public uint OptionIndex;
        public string[] Values;

        public EnumValue(uint optionIndex, string[] values) {
            OptionIndex = optionIndex;
            Values = values;
        }

        public static EnumValue ConstructEnumValue(MessageReader reader) {
            List<string> strings = new();
            uint current = reader.ReadPackedUInt32();
            while (reader.Position < reader.Length) strings.Add(reader.ReadString());

            return new EnumValue(current, strings.ToArray());
        }
    }

    public class FloatValue : IGameOptionValue {
        public string FormatString;
        public bool IsInfinity;
        public float Lower;
        public float Step;
        public float Upper;

        public float Value;

        public FloatValue(float value, float step, float lower, float upper, bool isInfinity, string formatString) {
            Value = value;
            Step = step;
            Lower = lower;
            Upper = upper;
            IsInfinity = isInfinity;
            FormatString = formatString;
        }
    }

    public class TitleOption : MonoBehaviour {
        public TextMeshPro Title;
        public TitleOption(IntPtr ptr) : base(ptr) { }
    }

    [HarmonyPatch(typeof(GameOptionsData), nameof(GameOptionsData.ToHudString))]
    public class HudStringPatch {
        [HarmonyPrefix]
        public static bool ToHudString(out string __result) {
            __result = "Game Settings:\n";

            foreach ((string categoryTitle, List<GameOption> options) in GameOptionsPatches.Categories) {
                if (categoryTitle != "") __result += $"{categoryTitle}\n";
                foreach (GameOption option in options) {
                    if (categoryTitle != "") __result += "  ";
                    __result += $"{option.Title}: ";
                    __result += option.Type switch {
                        OptionType.Number => string.Format(((FloatValue) option.Value).FormatString,
                            ((FloatValue) option.Value).Value),
                        OptionType.Boolean => ((BooleanValue) option.Value).Value ? "On" : "Off",
                        OptionType.Enum => ((EnumValue) option.Value).Values[((EnumValue) option.Value).OptionIndex],
                        _ => throw new ArgumentOutOfRangeException()
                    };
                    __result += '\n';
                }
            }

            return false;
        }
    }
}