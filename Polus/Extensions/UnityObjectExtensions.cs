using System;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Polus.Extensions {
    public static class UnityObjectExtensions {
        public static T DontDestroy<T>(this T obj) where T : Object {
            obj.hideFlags = HideFlags.DontSave;
            Object.DontDestroyOnLoad(obj);
            return obj;
        }

        public static T EnsureComponent<T>(this GameObject gameObject) where T : Component {
            T t = gameObject.GetComponent<T>();
            if (t != null) return t;
            return gameObject.AddComponent<T>();
        }

        // public static T EnsureComponent<T>(this Component gameObject) where T : Component {
        //     return gameObject.TryGetComponent(Il2CppType.Of<T>(), out Component component) ? component.Cast<T>() : gameObject.gameObject.AddComponent<T>();
        // }

        public static GameObject FindRecursive(this GameObject obj, Func<GameObject, bool> search) {
            GameObject result = null;
            for (int i = 0; i < obj.transform.childCount; i++) {
                Transform child = obj.transform.GetChild(i);
                if (search(child.gameObject)) return child.gameObject;

                result = FindRecursive(child.gameObject, search);

                if (result) break;
            }

            return result;
        }
    }
}