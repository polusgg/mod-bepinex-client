using System;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

namespace PolusggSlim.Patches.Misc
{
    public static class SkipIntroSplash
    {
        private static UnityAction<Scene, LoadSceneMode> SkipIntroHook = (Action<Scene, LoadSceneMode>) ((scene, _) =>
        {
            if (scene.name == "SplashIntro")
            {
                SceneManager.LoadScene("MainMenu");
            }
        });
        
        public static void Load() => SceneManager.add_sceneLoaded(SkipIntroHook);

        public static void Unload() => SceneManager.remove_sceneLoaded(SkipIntroHook);
    }
}