using System;
using System.IO;
using Hazel;
using UnhollowerBaseLib;
using UnityEngine;

namespace Polus.Extensions {
    public static class ReaderExtensions {
        public static Color32 ReadColor(this MessageReader reader) => new(reader.ReadByte(), reader.ReadByte(), reader.ReadByte(), reader.ReadByte());

        public static MessageReader Clone(this MessageReader reader) {
            reader.Position -= 3;
            return reader.ReadMessageAsNewBuffer();
        }

        public static Guid ReadGuid(this MessageReader reader) {
            byte[] guidData = reader.ReadBytes(16);
            return new Guid(guidData);
        }

        public static void WriteGuid(this MessageWriter writer, Guid guid) {
            writer.Write(guid.ToByteArray());
        }
    }
}