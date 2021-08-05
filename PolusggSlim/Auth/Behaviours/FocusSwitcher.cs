using System;
using System.Collections.Generic;
using PolusggSlim.Utils.Attributes;
using UnityEngine;

namespace PolusggSlim.Auth.Behaviours
{
    [RegisterInIl2Cpp]
    public class FocusSwitcher : MonoBehaviour
    {
        public List<TextBoxTMP> TextBoxes { get; } = new();
        
        
        public FocusSwitcher(IntPtr ptr) : base(ptr) { }

        public void FixedUpdate()
        {
            if (Input.GetKeyUp(KeyCode.Tab) || Input.inputString.Contains("\t"))
            {
                var firstFocusedTextbox = TextBoxes.FindIndex(x => x.hasFocus);
                TextBoxes[firstFocusedTextbox].LoseFocus();
                
                TextBoxes.ForEach(x => x.LoseFocus());

                var nextTextbox = TextBoxes[(firstFocusedTextbox + 1) % TextBoxes.Count];
                nextTextbox.GiveFocus();
            }
        }
    }
}
