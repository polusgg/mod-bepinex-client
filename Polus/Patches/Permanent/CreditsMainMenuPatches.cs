using System;
using Polus.Enums;
using Polus.Extensions;
using Polus.Utils;
using TMPro;
using UnhollowerBaseLib;
using UnhollowerRuntimeLib;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace Polus.Patches.Permanent {
    public class CreditsMainMenuPatches {
        private static Sprite _crunchuSprite;
        private static CreditsMenuHolder _creditsMenu;

        public static void Load() {
            _creditsMenu = PogusPlugin.Bundle
                .LoadAsset("Assets/Mods/CreditsMenu/CreditsMenu.prefab")
                .Cast<GameObject>()
                .DontDestroy()
                .AddComponent<CreditsMenuHolder>();
            // PogusPlugin.Bundle.GetAllAssetNames().Length.Log(10, "impostor");
            foreach (string allScenePath in PogusPlugin.Bundle.GetAllAssetNames())
                allScenePath.Log(comment: "41i912481249");
            Texture2D tex = PogusPlugin.Bundle
                .LoadAsset("Assets/Mods/CreditsMenu/crunchu.png")
                .Cast<Texture2D>().DontDestroy();
            _crunchuSprite = Sprite.Create(tex, new Rect(0f, 0f, tex.width, tex.height), new Vector2(0.5f, 0.5f))
                .DontDestroy();
            SceneManager.add_sceneLoaded(new Action<Scene, LoadSceneMode>(SceneLoaded));
        }

        private static void SceneLoaded(Scene scene, LoadSceneMode _) {
            if (scene.name != GameScenes.MainMenu) return;
            "log".Log(20);
            GameObject main = new("PolusCreditsButton");
            main.AddComponent<SpriteRenderer>().sprite = _crunchuSprite;
            main.AddComponent<CircleCollider2D>().radius = 0.5f;
            PassiveButton button = UIMethods.CreateButton(main, () => {
                GameObject creditsMenu = Object.Instantiate(_creditsMenu.gameObject, new Vector3(0, 0, -100f), new Quaternion());
                CreditsMenuHolder menu = creditsMenu.GetComponent<CreditsMenuHolder>();
                menu.transition = creditsMenu.AddComponent<TransitionOpen>();
                menu.transition.OnClose = new Button.ButtonClickedEvent();
                menu.transition.OnClose.AddListener(new Action(() => Object.Destroy(creditsMenu)));
                TextMeshPro tmp = creditsMenu.FindRecursive(x => x.name == "StaffText").GetComponent<TextMeshPro>();
                tmp.gameObject.AddComponent<OpenHyperlinks>().pTextMeshPro = tmp;
                GameObject left = creditsMenu.FindRecursive(obj => obj.name == "LeftPane");
                UIMethods.CreateButton(left, () => { }, false).Colliders = new Il2CppReferenceArray<Collider2D>(left.GetComponents<BoxCollider2D>());
                GameObject right = creditsMenu.FindRecursive(obj => obj.name == "RightPane");
                UIMethods.CreateButton(right, () => { }, false).Colliders = new Il2CppReferenceArray<Collider2D>(right.GetComponents<BoxCollider2D>());
                GameObject bg = creditsMenu.FindRecursive(obj => obj.name == "BackgroundClickHandler");
                bg.transform.localPosition += new Vector3(0, 0, -50f);
                menu.back = UIMethods.CreateButton(bg, () => menu.transition.Close(), false);
                menu.back.Colliders = new Il2CppReferenceArray<Collider2D>(bg.GetComponents<BoxCollider2D>());
                AspectSize aspec = creditsMenu.AddComponent<AspectSize>();
                aspec.PercentWidth = .80f;
                aspec.Background = creditsMenu.GetComponent<SpriteRenderer>().sprite;
                aspec.OnEnable();
                // Object.find
            });
            DotAligner da = Object.FindObjectOfType<DotAligner>();
            button.transform.parent = da.transform;
        }

        public class CreditsMenuHolder : MonoBehaviour {
            public static CreditsMenuHolder Instance;
            public TransitionOpen transition;
            public PassiveButton back;
            static CreditsMenuHolder() => ClassInjector.RegisterTypeInIl2Cpp<CreditsMenuHolder>();
            public CreditsMenuHolder(IntPtr ptr) : base(ptr) { }

            private void Start() {
                Instance = this;
                DownloadJesters();
                ControllerManager.Instance.OpenOverlayMenu("PolusCreditsMenu", back);
            }

            private void OnDisable() {
                ControllerManager.Instance.CloseOverlayMenu("PolusCreditsMenu");
            }

            public void DownloadJesters() {
                TextMeshPro tmp = gameObject.FindRecursive(x => x.name == "PatreonText").GetComponent<TextMeshPro>();
                tmp.text = "TODO";
            }
        }
    }
}