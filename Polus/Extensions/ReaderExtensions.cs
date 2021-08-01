using Hazel;
using UnityEngine;

namespace Polus.Extensions {
    public static class ReaderExtensions {
        public static Color32 ReadColor(this MessageReader reader) => new(reader.ReadByte(), reader.ReadByte(), reader.ReadByte(), reader.ReadByte());
    }
}