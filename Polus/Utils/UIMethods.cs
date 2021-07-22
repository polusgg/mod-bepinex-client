using System;
using Il2CppSystem.Text;
using Polus.Extensions;
using TMPro;
using UnhollowerBaseLib;
using UnityEngine;
using UnityEngine.UI;

namespace Polus.Utils {
    public class UIMethods {
        private static readonly SpriteRenderer _stupidGlyphRenderer;

        static UIMethods() {
            _stupidGlyphRenderer = new GameObject("stupidspriterendererfix").DontDestroy()
                .AddComponent<SpriteRenderer>();
        }

        public static TextBoxTMP LinkTextBox(GameObject main) {
            TextBoxTMP box = main.AddComponent<TextBoxTMP>();
            box.outputText = main.transform.Find("BoxLabel").GetComponent<TextMeshPro>();
            box.outputText.text = "";
            box.text = "";
            box.characterLimit = 72;
            box.quickChatGlyph = _stupidGlyphRenderer;
            box.sendButtonGlyph = _stupidGlyphRenderer;
            box.Background = main.GetComponent<SpriteRenderer>();
            box.Pipe = main.transform.Find("Pipe").GetComponent<MeshRenderer>();
            box.colliders = new Il2CppReferenceArray<Collider2D>(new Collider2D[] {main.GetComponent<BoxCollider2D>()});
            box.OnEnter = new Button.ButtonClickedEvent();
            box.OnChange = new Button.ButtonClickedEvent();
            box.OnFocusLost = new Button.ButtonClickedEvent();
            box.tempTxt = new StringBuilder();
            CreateButton(main, box.GiveFocus);
            return box;
        }

        public static PassiveButton CreateButton(GameObject main, Action clickHandler, bool shouldRollover = true) {
            PassiveButton button = main.AddComponent<PassiveButton>();
            button.OnClick = new Button.ButtonClickedEvent();
            button.OnMouseOut = new Button.ButtonClickedEvent();
            button.OnMouseOver = new Button.ButtonClickedEvent();
            button.OnClick.AddListener(clickHandler);
            if (shouldRollover) {
                ButtonRolloverHandler rollover = main.AddComponent<ButtonRolloverHandler>();
                rollover.Target = main.GetComponent<SpriteRenderer>();
                rollover.OutColor = Color.white;
                rollover.OverColor = Color.green;
                rollover.HoverSound = null;
            }

            PassiveButtonManager.Instance.RegisterOne(button);
            return button;
        }
    }
}