using Hazel;
using UnityEngine;

namespace Polus.Extensions {
    public static class ReaderExtensions {
        public static Color32 ReadColor(this MessageReader reader) => new(reader.ReadByte(), reader.ReadByte(), reader.ReadByte(), reader.ReadByte());

        public static MessageReader Clone(this MessageReader reader) {
            byte[] data = reader.ReadBytes(reader.BytesRemaining);
            MessageReader second = new() {
                Buffer = data,
                Offset = 0,
                Position = 0
            };
            return second;
        }
    }
}