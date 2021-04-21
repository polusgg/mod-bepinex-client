using System.Collections;
using PolusGG.Behaviours;
using UnityEngine;

namespace PolusGG.Extensions {
    public static class CoroutineExtensions {
        public static IEnumerator StartCoroutine(this MonoBehaviour component, IEnumerator coroutine) {
            return component.gameObject.EnsureComponent<CoroutineManager>().Start(coroutine);
        }

        public static void StopCoroutine(this MonoBehaviour component, IEnumerator coroutine) {
            component.gameObject.EnsureComponent<CoroutineManager>().Stop(coroutine);
        }
    }
}