using System;
using HarmonyLib;
using Polus.Enums;
using Polus.Extensions;
using UnhollowerRuntimeLib;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace Polus.Patches.Permanent {
    public class FontMwenuwuPatches {
        public static void Load() {
            "unfortunately it was not meant to be.".Log();
            SceneManager.add_sceneLoaded(new Action<Scene, LoadSceneMode>(OnLoadScene));
        }

        private static void OnLoadScene(Scene arg1, LoadSceneMode arg2) {
            if (arg1.name == GameScenes.MainMenu) {
                LanguageSetter langSetter = Object.FindObjectOfType(Il2CppType.Of<LanguageSetter>(), true)
                    .Cast<LanguageSetter>();
                Transform fontObject = Object.Instantiate(langSetter.transform, langSetter.transform.parent);
                LanguageSetter ls2 = fontObject.GetComponent<LanguageSetter>();
                FontMenuManager fontMenuManager = ls2.gameObject.AddComponent<FontMenuManager>();
                Object.DestroyImmediate(ls2);
                fontMenuManager.ButtonPrefab = langSetter.ButtonPrefab;
                fontMenuManager.ButtonHeight = langSetter.ButtonHeight;
                fontMenuManager.ButtonParent = langSetter.ButtonParent;
                fontMenuManager.ButtonStart = langSetter.ButtonStart;

                SettingsLanguageMenu langButton = Object.FindObjectOfType(Il2CppType.Of<SettingsLanguageMenu>(), true)
                    .Cast<SettingsLanguageMenu>();
                GameObject gameObject = Object.Instantiate(langButton.gameObject, langButton.transform.parent);
                gameObject.transform.position = new Vector3();
                Object.DestroyImmediate(gameObject.GetComponent<SettingsLanguageMenu>());
                gameObject.name = "FunnyFontMenuButton";
                PassiveButton btn = gameObject.GetComponent<PassiveButton>();
                btn.OnClick = new Button.ButtonClickedEvent();
                btn.OnClick.AddListener(new Action(fontMenuManager.Open));
            }
        }

        [HarmonyPatch(typeof(SettingsLanguageMenu), nameof(SettingsLanguageMenu.Awake))]
        public class SetLanguageOpenButtonAwake { }

        private class FontMenuManager : MonoBehaviour {
            public LanguageButton ButtonPrefab;
            public Scroller ButtonParent;
            public float ButtonStart = 0.5f;
            public float ButtonHeight = 0.5f;
            private LanguageButton[] AllButtons;

            static FontMenuManager() {
                ClassInjector.RegisterTypeInIl2Cpp<FontMenuManager>();
            }

            public FontMenuManager(IntPtr ptr) : base(ptr) { }

            private void Start() {
                Collider2D component = ButtonParent.GetComponent<Collider2D>();
                Vector3 localPosition = new(0f, ButtonStart, -0.5f);
                string[] fonts = Font.GetOSInstalledFontNames();
                AllButtons = new LanguageButton[fonts.Length];
                foreach (string fontName in fonts) {
                    LanguageButton button = Instantiate(ButtonPrefab, ButtonParent.Inner);
                    button.Title.text = fontName;
                    button.Title.color = PggSaveManager.FontName == fontName ? Color.green : Color.white;
                    button.transform.localPosition = localPosition;
                    button.Button.ClickMask = component;
                    localPosition.y -= ButtonHeight;
                }

                ButtonParent.YBounds.max = fonts.Length * ButtonHeight - 2f * ButtonStart - 0.1f;
            }

            public void Open() {
                gameObject.active = true;
            }

            public void Close() {
                gameObject.active = false;
            }
        }
    }
}