using System;
using Hazel;
using UnityEngine;

namespace Polus.Extensions {
    public static class ReaderExtensions {
        public static Color32 ReadColor(this MessageReader reader) => new(reader.ReadByte(), reader.ReadByte(), reader.ReadByte(), reader.ReadByte());

        public static MessageReader Clone(this MessageReader reader) {
            reader.Position -= 3;
            return reader.ReadMessageAsNewBuffer();
        }
    }
}