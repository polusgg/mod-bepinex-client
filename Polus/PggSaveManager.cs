using System.Collections.Generic;
using System.IO;
using MonoMod.Utils;
using Polus.Enums;
using Polus.Extensions;
using Polus.Utils;

namespace Polus {
    public static class PggSaveManager {
        // public static Dictionary<string, GameOption> GameOptions = new();
        private static string SavePath => Path.Combine(PlatformPaths.persistentDataPath, PggConstants.SaveFileName);
        private static bool _initiallyLoaded;
        private static string _fontName = "Arial";
        private static byte _currentRegion;

        private static readonly Stack<long> startStack = new();

        public static string FontName {
            get {
                LoadSave();
                return _fontName;
            }
        }

        public static byte CurrentRegion {
            get {
                LoadSave();
                return _currentRegion;
            }
            set {
                LoadSave();
                _currentRegion = value;
                SaveSave();
            }
        }

        public static void LoadSave(bool noOverwrite = true) {
            if (_initiallyLoaded && noOverwrite) return;

            _initiallyLoaded = true;
            if (File.Exists(SavePath)) {
                CatchHelper.TryCatch(() => {
                    using FileStream stream = File.OpenRead(SavePath);
                    using BinaryReader reader = new(stream);
                    Deserialize(reader);
                });
            }
        }

        public static void SaveSave() {
            using FileStream stream = File.OpenWrite(SavePath);
            using BinaryWriter writer = new(stream);
            Serialize(writer);
        }

        private static void Deserialize(BinaryReader reader) {
            while (reader.BaseStream.Position < reader.BaseStream.Length) {
                ushort length = reader.ReadUInt16();
                byte type = reader.ReadByte();
                switch ((SaveValues) type) {
                    case SaveValues.FontName: {
                        _fontName = reader.ReadNullTerminatedString();
                        break;
                    }
                    case SaveValues.CurrentRegion: {
                        _currentRegion = reader.ReadByte();
                        break;
                    }
                    default: {
                        "Attempted to load invalid save option".Log(comment: "");
                        reader.ReadBytes(length);
                        break;
                    }
                }
            }
            $"Loaded {SavePath}".Log();
        }

        private static void Serialize(BinaryWriter writer) {
            StartMessage(writer, SaveValues.FontName);
            writer.WriteNullTerminatedString(_fontName);
            EndMessage(writer);
            StartMessage(writer, SaveValues.CurrentRegion);
            writer.Write(_currentRegion);
            EndMessage(writer);
            $"Saved {SavePath}".Log();
        }

        private static void StartMessage(BinaryWriter writer, SaveValues type) {
            startStack.Push(writer.BaseStream.Position);
            writer.Write((ushort) 0);
            writer.Write((byte) type);
        }

        private static void EndMessage(BinaryWriter writer) {
            long currentPosition = writer.BaseStream.Position;
            long lengthPosition = startStack.Pop();
            writer.BaseStream.Seek(lengthPosition, SeekOrigin.Begin);
            writer.Write((ushort) (currentPosition - lengthPosition - 3));
            writer.BaseStream.Seek(currentPosition, SeekOrigin.Begin);
        }
    }
}