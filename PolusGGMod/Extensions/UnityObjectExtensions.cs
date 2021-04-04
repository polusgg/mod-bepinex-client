using UnityEngine;

namespace PolusGG.Extensions {
    public static class UnityObjectExtensions {
        public static T DontDestroy<T>(this T obj) where T : Object {
            obj.hideFlags = HideFlags.DontSave;
            Object.DontDestroyOnLoad(obj);
            return obj;
        }
    }
}