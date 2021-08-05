using Hazel;
using UnityEngine;

namespace PolusggSlim.Utils.Extensions
{
    public static class MessageExtensions
    {
        private const float MIN = -50f;
        private const float MAX = 50f;

        private static float ReverseLerp(float t)
        {
            return Mathf.Clamp((t - MIN) / (MAX - MIN), 0f, 1f);
        }

        public static void WriteVector2(this MessageWriter writer, Vector2 value)
        {
            var x = (ushort) (ReverseLerp(value.x) * ushort.MaxValue);
            var y = (ushort) (ReverseLerp(value.y) * ushort.MaxValue);

            writer.Write(x);
            writer.Write(y);
        }

        public static Vector2 ReadVector2(this MessageReader reader)
        {
            var x = reader.ReadUInt16() / (float) ushort.MaxValue;
            var y = reader.ReadUInt16() / (float) ushort.MaxValue;

            return new Vector2(Mathf.Lerp(MIN, MAX, x), Mathf.Lerp(MIN, MAX, y));
        }

        public static Color32 ReadColor(this MessageReader reader)
        {
            return new(reader.ReadByte(), reader.ReadByte(), reader.ReadByte(), reader.ReadByte());
        }
    }
}