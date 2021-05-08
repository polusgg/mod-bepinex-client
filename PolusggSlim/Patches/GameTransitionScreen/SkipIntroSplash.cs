using System;
using UnityEngine.SceneManagement;

namespace PolusggSlim.Patches.GameTransitionScreen
{
    public static class SkipIntroSplash
    {
        private static readonly Action<Scene, LoadSceneMode> SkipIntroHook = (scene, _) =>
        {
            if (scene.name == "SplashIntro") SceneManager.LoadScene("MainMenu");
        };

        public static void Load()
        {
            SceneManager.add_sceneLoaded(SkipIntroHook);
        }

        public static void Unload()
        {
            SceneManager.remove_sceneLoaded(SkipIntroHook);
        }
    }
}