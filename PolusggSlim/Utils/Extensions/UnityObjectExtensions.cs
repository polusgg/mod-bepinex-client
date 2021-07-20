using System;
using UnityEngine;
using Object = UnityEngine.Object;

namespace PolusggSlim.Utils.Extensions
{
    public static class UnityObjectExtensions
    {
        public static T DontDestroy<T>(this T obj) where T : Object
        {
            obj.hideFlags = HideFlags.DontSave;
            Object.DontDestroyOnLoad(obj);
            return obj;
        }

        public static T EnsureComponent<T>(this GameObject gameObject) where T : Component
        {
            return gameObject.GetComponent<T>() ?? gameObject.AddComponent<T>();
        }

        public static bool TryDestroyComponent<T>(this GameObject gameObject) where T : Component
        {
            var component = gameObject.GetComponent<T>();
            if (component != null)
                Object.Destroy(component);
            return component != null;
        }

        public static void Destroy(this GameObject gameObject, bool immediate = false)
        {
            if (immediate)
                Object.DestroyImmediate(gameObject);
            else
                Object.Destroy(gameObject);
        }

        public static GameObject FindRecursive(this GameObject obj, Func<GameObject, bool> search)
        {
            GameObject result = null;

            for (var i = 0; i < obj.transform.childCount; i++)
            {
                var child = obj.transform.GetChild(i);

                if (search(child.gameObject))
                    return child.gameObject;

                result = FindRecursive(child.gameObject, search);

                if (result)
                    break;
            }

            return result;
        }
    }
}