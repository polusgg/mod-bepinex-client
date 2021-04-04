using UnityEngine;

namespace PolusMod.Extensions {
    public static class LerpExtensions {
        public static float Lerp(this FloatRange range, float v) {
            return Mathf.Lerp(range.max, range.min, v);
        }
    }
}