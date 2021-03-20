using System;
using UnhollowerRuntimeLib;
using UnityEngine;

namespace PolusMod.Pno {
    public class PolusGraphic : MonoBehaviour {
        public Texture2D Texture2D;
        public PolusGraphic(IntPtr ptr) : base(ptr) { }

        static PolusGraphic() {
            ClassInjector.RegisterTypeInIl2Cpp<PolusGraphic>();
        }
    }
}