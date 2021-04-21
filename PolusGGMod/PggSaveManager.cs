using System.Collections.Generic;
using System.IO;
using PolusGG.Enums;

namespace PolusGG {
    public static class PggSaveManager {
        // public static Dictionary<string, GameOption> GameOptions = new();
        private static bool _initiallyLoaded;
        private static string _fontName = "Arial";

        private static readonly Stack<long> startStack = new();

        public static string FontName {
            get {
                LoadSave();
                return _fontName;
            }
        }

        public static void LoadSave(bool noOverwrite = true) {
            if (_initiallyLoaded && noOverwrite) return;
            string path = Path.Combine(PlatformPaths.persistentDataPath, PggConstants.SaveFileName);
            using FileStream stream = File.OpenRead(path);
            using BinaryReader reader = new(stream);
            Deserialize(reader);

            _initiallyLoaded = true;
        }

        public static void SaveSave() {
            string path = Path.Combine(PlatformPaths.persistentDataPath, PggConstants.SaveFileName);
            using FileStream stream = File.OpenRead(path);
            using BinaryWriter writer = new(stream);
            Serialize(writer);
        }

        private static void Deserialize(BinaryReader reader) {
            while (reader.BaseStream.Position < reader.BaseStream.Length) {
                ushort length = reader.ReadUInt16();
                byte type = reader.ReadByte();
                switch ((SaveValues) type) {
                    case SaveValues.FontName: {
                        _fontName = reader.ReadString();
                        break;
                    }
                    default: {
                        PogusPlugin.Logger.LogWarning("Attempted to load invalid save option");
                        reader.ReadBytes(length);
                        break;
                    }
                }
            }
        }

        private static void Serialize(BinaryWriter writer) {
            StartMessage(writer, SaveValues.FontName);
            writer.Write(_fontName);
            EndMessage(writer);
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
            writer.Write((ushort) lengthPosition - currentPosition - 1);
            writer.BaseStream.Seek(currentPosition, SeekOrigin.Begin);
        }
    }
}