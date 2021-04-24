using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using HarmonyLib;
using Hazel;
using PolusGG.Enums;
using PolusGG.Extensions;
using PolusGG.Utils;
using TMPro;
using UnhollowerBaseLib;
using UnityEngine;
using Object = UnityEngine.Object;

namespace PolusGG.Patches.Temporary {
    public static class GameOptionsPatches {
        public static Dictionary<string, List<GameOption>> Categories = new();
        public static Dictionary<string, GameOption> OptionMap = new();
        private static object lockable = new();

        private static TextMeshPro groupTitle;
        private static KeyValueOption enumOption;
        private static NumberOption numbOption;
        private static ToggleOption boolOption;

        public static void UpdateHudString() {
            "L".Log(3);
            CatchHelper.TryCatch(() => HudManager.Instance.GameSettings.text = PlayerControl.GameOptions.ToHudString(69));
        }

        [HarmonyPatch(typeof(GameSettingMenu), nameof(GameSettingMenu.OnEnable))]
        public class OnEnablePatch {
            [HarmonyPrefix]
            public static bool OnEnable(GameSettingMenu __instance) {
                lock (lockable) {
                    if (!CustomPlayerMenu.Instance || CustomPlayerMenu.Instance.selectedTab != 4) return true;
                    if (groupTitle == null) {
                        groupTitle = Object.Instantiate(__instance.AllItems[3].GetComponentInChildren<TextMeshPro>())
                            .DontDestroy();
                        groupTitle.gameObject.active = false;
                        groupTitle.name = "CategoryTitlePrefab";
                    }

                    if (enumOption == null) {
                        enumOption = Object.Instantiate(__instance.AllItems[1].gameObject)
                            .GetComponent<KeyValueOption>().DontDestroy();
                        enumOption.gameObject.active = false;
                        enumOption.name = "EnumOptionPrefab";
                    }

                    if (numbOption == null) {
                        numbOption = Object.Instantiate(__instance.AllItems[2].gameObject)
                            .GetComponent<NumberOption>().DontDestroy();
                        numbOption.gameObject.active = false;
                        numbOption.name = "NumberOptionPrefab";
                    }

                    if (boolOption == null) {
                        boolOption = Object.Instantiate(__instance.AllItems[3].gameObject)
                            .GetComponent<ToggleOption>().DontDestroy();
                        boolOption.gameObject.active = false;
                        boolOption.name = "BooleanOptionPrefab";
                    }

                    List<Transform> options = new();
                    foreach ((string categoryTitle, List<GameOption> gameOptions) in Categories) {
                        if (categoryTitle != "") {
                            TextMeshPro newTitle =
                                Object.Instantiate(groupTitle, new Vector3(0, 0, -10), new Quaternion());
                            newTitle.text = categoryTitle;
                            options.Add(newTitle.transform);
                        }

                        foreach (GameOption gameOption in gameOptions) {
                            switch (gameOption.Type) {
                                case OptionType.Number: {
                                    FloatValue value = (FloatValue) gameOption.Value;
                                    NumberOption option =
                                        Object.Instantiate(numbOption, new Vector3(0, 0, -10), new Quaternion());
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
                                    $"Adding {gameOption.Title}".Log();
                                    option.OnValueChanged = new Action<OptionBehaviour>(HandleStringChanged);
                                    $"Added {gameOption.Title}".Log();
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

                    Scroller scroller = __instance.GetComponent<Scroller>();
                    int index = 0;
                    foreach (Transform transforms in __instance.AllItems) {
                        transforms.SetParent(scroller.Inner);
                        transforms.localPosition = new Vector3(0, __instance.YStart - index++ * __instance.YOffset, -1);
                        transforms.gameObject.SetActive(true);
                    }

                    scroller.YBounds.max =
                        index * __instance.YOffset - 2f * __instance.YStart - 0.5f;
                }

                return true;
            }

            public static void HandleToggleChanged(OptionBehaviour toggleBehaviour) {
                "LLLLLL".Log(4);
                UpdateHudString();
                ToggleOption toggle = toggleBehaviour.Cast<ToggleOption>();
                GameOption gameOption = OptionMap[toggle.TitleText.text];
                BooleanValue value = (BooleanValue) gameOption.Value;
                value.Value = toggle.CheckMark.enabled;
                MessageWriter writer = MessageWriter.Get(SendOption.Reliable);
                writer.StartMessage((byte) PolusRootPackets.SetGameOption);
                // TODO do this when serverside sequence id handling
                writer.Write((ushort)0);
                writer.Write(gameOption.CategoryName);
                writer.Write(toggle.TitleText.text);
                writer.Write((byte) 1);
                writer.Write(toggle.CheckMark.enabled);
                PolusMod.EndSend(writer);
            }

            public static void HandleNumberChanged(OptionBehaviour toggleBehaviour) {
                "LLLLLLL2".Log(4);
                UpdateHudString();
                NumberOption toggle = toggleBehaviour.Cast<NumberOption>();
                GameOption gameOption = OptionMap[toggle.TitleText.text];
                FloatValue value = (FloatValue) gameOption.Value;
                value.Value = (uint) toggle.Value;
                MessageWriter writer = MessageWriter.Get(SendOption.Reliable);
                writer.StartMessage((byte) PolusRootPackets.SetGameOption);
                writer.Write((ushort)0);
                writer.Write(gameOption.CategoryName);
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
                "LLLLLL3".Log(4);
                UpdateHudString();
                KeyValueOption toggle = toggleBehaviour.Cast<KeyValueOption>();
                GameOption gameOption = OptionMap[toggle.TitleText.text];
                EnumValue value = (EnumValue) gameOption.Value;
                value.OptionIndex = (uint) toggle.Selected;
                MessageWriter writer = MessageWriter.Get(SendOption.Reliable);
                writer.StartMessage((byte) PolusRootPackets.SetGameOption);
                writer.Write((ushort)0);
                writer.Write(gameOption.CategoryName);
                writer.Write(toggle.TitleText.text);
                writer.Write((byte) OptionType.Enum);
                writer.WritePacked(toggle.Selected);
                // just for the fans (not used on server, just to avoid server crashes)
                foreach (Il2CppSystem.Collections.Generic.KeyValuePair<string, int> keyValuePair in toggle.Values)
                    writer.Write(keyValuePair.key);

                PolusMod.EndSend(writer);
            }

            private static bool _hasStarted;
            private static ushort _nextSequenceReceived;
            private static SortedList<ushort, GameOptionPacket> _packetQueue = new();

            public static void Reset() {
                _packetQueue = new SortedList<ushort, GameOptionPacket>();
                _hasStarted = false;
                _nextSequenceReceived = 0;
                Categories = new Dictionary<string, List<GameOption>>();
                OptionMap = new Dictionary<string, GameOption>();
            }

            public static void ReceivedGameOptionPacket(GameOptionPacket packet) {
                ushort sequenceId = packet.SequenceId;
                if (sequenceId != _nextSequenceReceived && NetHelpers.SidGreaterThan(sequenceId, _nextSequenceReceived) && !_hasStarted) {
                    $"Got {sequenceId}, waiting for ${_nextSequenceReceived}, {_packetQueue.Count} in queue right now".Log();
                    _packetQueue.Add(sequenceId, packet);
                } else {
                    ushort lastId = sequenceId;
                    CatchHelper.TryCatch(() => HandlePacket(packet));
                    if (!_hasStarted) {
                        _nextSequenceReceived = (ushort) (sequenceId + 1);
                        _hasStarted = true;
                        return;
                    }
                    lock(_packetQueue) while (_packetQueue.Count != 0) {
                        GameOptionPacket packet2 = _packetQueue.Values[0];
                        CatchHelper.TryCatch(() => HandlePacket(packet2));
                        _packetQueue.RemoveAt(0);
                        if (_packetQueue.Count == 0) lastId = packet2.SequenceId;
                    }

                    _nextSequenceReceived = (ushort) (lastId + 1);
                    $"Handled all packets up to seqid {_nextSequenceReceived}".Log();
                }
            }

            private static void HandlePacket(GameOptionPacket packet) {
                lock (lockable) {       
                    MessageReader reader = packet.Reader;
                    switch (packet.Type) {
                        case OptionPacketType.DeleteOption: {
                            string name = reader.ReadString();
                            List<GameOption> category = Categories.First(x => x.Value.Any(x => x.Title == name)).Value;
                            category.RemoveAll(x => x.Title == name);
                            OptionMap.Remove(name);
                            if (category.Count == 0) Categories.Remove(Categories.First(y => y.Value == category).Key);

                            PolusMod.Instance.DirtyOptions();
                            break;
                        }
                        case OptionPacketType.SetOption: {
                            string cat = reader.ReadString();
                            string name = reader.ReadString();
                            OptionType optionType = (OptionType) reader.ReadByte();

                            PolusMod.Instance.DirtyOptions();

                            if (!Categories.ContainsKey(cat))
                                Categories.Add(cat, new List<GameOption>());
                            List<GameOption> category = Categories[cat];

                            if (category.Any(x => x.Title == name)) {
                                category.Find(x => x.Title == name).Value = optionType switch {
                                    OptionType.Boolean => new BooleanValue(reader.ReadBoolean()),
                                    OptionType.Number => new FloatValue(reader.ReadSingle(), reader.ReadSingle(),
                                        reader.ReadSingle(), reader.ReadSingle(), reader.ReadBoolean(),
                                        reader.ReadString()),
                                    OptionType.Enum => EnumValue.ConstructEnumValue(reader),
                                    _ => throw new ArgumentOutOfRangeException()
                                };
                            } else {
                                GameOption option = new() {
                                    Title = name,
                                    Type = optionType,
                                    Value = optionType switch {
                                        OptionType.Boolean => new BooleanValue(reader.ReadBoolean()),
                                        OptionType.Number => new FloatValue(reader.ReadSingle(), reader.ReadSingle(),
                                            reader.ReadSingle(), reader.ReadSingle(), reader.ReadBoolean(),
                                            reader.ReadString()),
                                        OptionType.Enum => EnumValue.ConstructEnumValue(reader),
                                        _ => throw new ArgumentOutOfRangeException()
                                    },
                                    CategoryName = cat
                                };
                                OptionMap[name] = option;
                                category.Add(option);
                            }

                            break;
                        }
                    }
                }
            }
        }

        public class GameOptionPacket {
            public ushort SequenceId;
            public OptionPacketType Type;
            public MessageReader Reader;
        }

        [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.RpcSyncSettings))]
        public class ScrewYouRpcSyncSettings {
            [HarmonyPrefix]
            public static bool RpcSyncSettings() {
                return false;
            }
        }

        [HarmonyPatch(typeof(KeyValueOption), nameof(KeyValueOption.Increase))]
        public class S {
            [HarmonyPrefix]
            public static bool RpcSyncSettings(KeyValueOption __instance) {
                __instance.Selected = Mathf.Clamp(__instance.Selected + 1, 0, __instance.Values.Count - 1);
                OnEnablePatch.HandleStringChanged(__instance);
                return false;
            }
        }

        [HarmonyPatch(typeof(KeyValueOption), nameof(KeyValueOption.Decrease))]
        public class S2 {
            [HarmonyPrefix]
            public static bool RpcSyncSettings(KeyValueOption __instance) {
                __instance.Selected = Mathf.Clamp(__instance.Selected - 1, 0, __instance.Values.Count - 1);
                OnEnablePatch.HandleStringChanged(__instance);
                return false;
            }
        }

        [HarmonyPatch(typeof(NumberOption), nameof(NumberOption.Increase))]
        public class AmazonS3 {
            [HarmonyPrefix]
            public static bool RpcSyncSettings(NumberOption __instance) {
                __instance.Value = __instance.ValidRange.Clamp(__instance.Value + __instance.Increment);
                OnEnablePatch.HandleNumberChanged(__instance);
                return false;
            }
        }

        [HarmonyPatch(typeof(NumberOption), nameof(NumberOption.Decrease))]
        public class S4 {
            [HarmonyPrefix]
            public static bool RpcSyncSettings(NumberOption __instance) {
                __instance.Value = __instance.ValidRange.Clamp(__instance.Value + __instance.Increment);
                OnEnablePatch.HandleNumberChanged(__instance);
                return false;
            }
        }

        [HarmonyPatch(typeof(ToggleOption), nameof(ToggleOption.Toggle))]
        public class S5 {
            [HarmonyPrefix]
            public static bool RpcSyncSettings(ToggleOption __instance) {
                __instance.CheckMark.enabled = !__instance.CheckMark.enabled;
                OnEnablePatch.HandleNumberChanged(__instance);
                return false;
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
        public string CategoryName { get; set; }
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

            string oute = "";

            foreach ((string categoryTitle, List<GameOption> options) in GameOptionsPatches.Categories) {
                CatchHelper.TryCatch(() => {
                    string output = "";
                    if (categoryTitle != "") output += $"{categoryTitle}\n";
                    foreach (GameOption option in options) {
                        if (categoryTitle != "") output += "  ";
                        output += $"{option.Title}: ";
                        output += option.Type switch {
                            OptionType.Number => string.Format(((FloatValue) option.Value).FormatString,
                                ((FloatValue) option.Value).Value),
                            OptionType.Boolean => ((BooleanValue) option.Value).Value ? "On" : "Off",
                            OptionType.Enum => ((EnumValue) option.Value).Values
                                [((EnumValue) option.Value).OptionIndex],
                            _ => throw new ArgumentOutOfRangeException()
                        };
                        output += '\n';
                    }

                    oute += output;
                });
            }

            __result = oute;

            return false;
        }
    }
}