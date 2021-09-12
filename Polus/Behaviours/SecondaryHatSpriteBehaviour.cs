using System;
using System.Collections.Generic;
using Polus.Enums;
using Polus.Extensions;
using Reactor;
using UnhollowerRuntimeLib;
using UnityEngine;

namespace Polus.Behaviours {
    public class SecondaryHatSpriteBehaviour : MonoBehaviour {
        static SecondaryHatSpriteBehaviour() => ClassInjector.RegisterTypeInIl2Cpp<SecondaryHatSpriteBehaviour>();
        public SecondaryHatSpriteBehaviour(IntPtr ptr) : base(ptr) { }
        private static Dictionary<HatParent, SecondaryHatSpriteBehaviour> Cache = new(Il2CppEqualityComparer<HatParent>.Instance);
        public static SecondaryHatSpriteBehaviour GetHelper(HatParent hat) => Cache.ContainsKey(hat) ? Cache[hat] : Cache[hat] = Create(hat);
        public HatParent parent;
        public SpriteRenderer thirdLayer;
        public int color = int.MaxValue;
        public HatState state;

        private static SecondaryHatSpriteBehaviour Create(HatParent parent) {
            SpriteRenderer front2 = Instantiate(parent.FrontLayer, parent.transform);
            SecondaryHatSpriteBehaviour sec = front2.gameObject.AddComponent<SecondaryHatSpriteBehaviour>();
            sec.parent = parent;
            sec.thirdLayer = front2;
            return sec;
        }

        public void SetColor(int colorParam) {
            color = colorParam;
            
            Update();
        }

        public void Update() {
            PlayerControl.SetPlayerMaterialColors(color, thirdLayer);
        }
    }
}