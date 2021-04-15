using System;
using UnityEngine.SceneManagement;

namespace PolusggSlim.Patches.Misc
{
    public static class SkipIntroSplash
    {
        public static void Init()
        {
            SceneManager.add_sceneLoaded(new Action<Scene, LoadSceneMode>((scene, mode) =>
            {
                if (scene.name == "SplashIntro")
                {
                    SceneManager.LoadScene("MainMenu");
                }       
            }));
        }
    }
}