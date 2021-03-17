using UnityEngine;

namespace PolusMod {
    public static class LerpExtensions {
        public static float Lerp(this FloatRange range, float v) {
            return Mathf.Lerp(range.max, range.min, 1f - Mathf.Pow(2f, -10f * v));
        }
    }
}