using System.Collections;
using Polus.Behaviours;
using UnityEngine;

namespace Polus.Extensions {
    public static class CoroutineExtensions {
        public static IEnumerator StartCoroutine(this MonoBehaviour component, IEnumerator coroutine) {
            return component.gameObject.EnsureComponent<CoroutineManager>().Start(coroutine);
        }

        public static void StopCoroutine(this MonoBehaviour component, IEnumerator coroutine) {
            component.gameObject.EnsureComponent<CoroutineManager>().Stop(coroutine);
        }
    }
}