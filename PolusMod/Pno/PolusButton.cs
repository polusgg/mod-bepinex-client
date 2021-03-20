using System;
using UnhollowerRuntimeLib;
using UnityEngine;

namespace PolusMod.Pno {
    public class PolusButton : MonoBehaviour {
        public PolusButton(IntPtr ptr) : base(ptr) { }

        static PolusButton() {
            ClassInjector.RegisterTypeInIl2Cpp<PolusButton>();
        }
    }
}