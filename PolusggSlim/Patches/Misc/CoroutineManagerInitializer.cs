using System;
using PolusggSlim.Utils;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace PolusggSlim.Patches.Misc
{
    public class CoroutineManagerInitializer
    {
        private static readonly Action<Scene, LoadSceneMode> ManagerInitializeHook = (scene, _) =>
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