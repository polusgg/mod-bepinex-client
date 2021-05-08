using System;
using PolusGG.Extensions;
using PolusGG.Utils;
using TMPro;
using UnhollowerRuntimeLib;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace PolusGG.Patches.Permanent {
    public class CreditsMainMenuPatches {
        private static Sprite _crunchuSprite;
        private static CreditsMenuHolder _creditsMenu;

        public static void Load() {
            _creditsMenu = Object.Instantiate(PogusPlugin.Bundle
                .LoadAsset("Assets/Mods/CreditsMenu/CreditsMenu.prefab")
                .Cast<GameObject>())
                .DontDestroy()
                .AddComponent<CreditsMenuHolder>();
            TransitionOpen transitionOpen = _creditsMenu.gameObject.AddComponent<TransitionOpen>();
            transitionOpen.OnClose = new Button.ButtonClickedEvent();
            transitionOpen.OnClose.AddListener(new Action(() => _creditsMenu.gameObject.SetActive(false)));
            Texture2D tex = PogusPlugin.Bundle
                .LoadAsset("Assets/Mods/CreditsMenu/crunchu.png")
                .Cast<Texture2D>().DontDestroy();
            _crunchuSprite = Sprite.Create(tex, new Rect(0f, 0f, tex.width, tex.height), new Vector2(0.5f, 0.5f)).DontDestroy();
        }

        public class CreditsMenuHolder : MonoBehaviour {
            static CreditsMenuHolder() => ClassInjector.RegisterTypeInIl2Cpp<CreditsMenuHolder>();
            public CreditsMenuHolder(IntPtr ptr) : base(ptr) { }

            private void Start() {
                DownloadJesters();
                SceneManager.add_sceneLoaded(new Action<Scene, LoadSceneMode>(SceneLoaded));
            }

            public void DownloadJesters() {
                TextMeshPro tmp = gameObject.FindRecursive(x => x.name == "PatreonText").GetComponent<TextMeshPro>();

                // tmp.text = "TODO";
            }

            private void SceneLoaded(Scene scene, LoadSceneMode _) {
                if (scene.name != "MainMenu") return;
                GameObject main = new("PolusCreditsButton");
                main.AddComponent<SpriteRenderer>().sprite = _crunchuSprite;
                main.AddComponent<CircleCollider2D>().radius = 0.5f;
                PassiveButton button = UIMethods.CreateButton(main, () => { gameObject.SetActive(true); });
                DotAligner da = FindObjectOfType<DotAligner>();
                button.transform.parent = da.transform;
                DownloadJesters();
            }
        }
    }
}