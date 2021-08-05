using System;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace PolusggSlim.Coroutines
{
    public class CoroutineManagerInitializer
    {
        private static readonly Action<Scene, LoadSceneMode> ManagerInitializeHook = (_, _) =>
        {
            var gameObject = new GameObject($"PolusggSlim - {nameof(CoroutineProcessor)}");
            gameObject.AddComponent<CoroutineProcessor>();
        };

        public static void Load()
        {
            SceneManager.add_sceneLoaded(ManagerInitializeHook);
        }

        public static void Unload()
        {
            SceneManager.remove_sceneLoaded(ManagerInitializeHook);
        }
    }
}