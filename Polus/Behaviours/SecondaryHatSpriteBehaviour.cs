using System;
using System.Collections.Generic;
using Polus.Enums;
using Polus.Extensions;
using Polus.Patches.Permanent;
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
        public Color backColor = Color.magenta;
        public Color bodyColor = Color.magenta;
        public float hatOpacity = 1f;
        public HatState state;

        private static SecondaryHatSpriteBehaviour Create(HatParent parent) {
            SpriteRenderer front2 = Instantiate(parent.FrontLayer, parent.transform);
            SecondaryHatSpriteBehaviour sec = front2.gameObject.AddComponent<SecondaryHatSpriteBehaviour>();
            sec.parent = parent;
            sec.thirdLayer = front2;
            return sec;
        }

        public void SetColor(int colorParam) => SetColor(Palette.ShadowColors[colorParam], Palette.PlayerColors[colorParam]);

        public void SetColor(Color? back, Color? body) {
            if (back.HasValue) backColor = back.Value;
            if (body.HasValue) bodyColor = body.Value;
            
            Update();
        }

        public void Update() {
            thirdLayer.SetAlpha(hatOpacity);
            parent.FrontLayer.SetAlpha(hatOpacity);
            parent.BackLayer.SetAlpha(hatOpacity);
            thirdLayer.SetPlayerMaterialColors(backColor, bodyColor);
            parent.FrontLayer.SetPlayerMaterialColors(backColor, bodyColor);
            parent.BackLayer.SetPlayerMaterialColors(backColor, bodyColor);
        }
    }
}