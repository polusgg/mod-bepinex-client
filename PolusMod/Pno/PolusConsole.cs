using System;
using Hazel;
using PolusApi.Net;
using PolusApi.Resources;
using UnhollowerRuntimeLib;
using UnityEngine;

namespace PolusMod.Pno {
    public class PolusButton : PnoBehaviour {
        private TextRenderer textRenderer;
        public PolusButton(IntPtr ptr) : base(ptr) { }

        static PolusButton() {
            ClassInjector.RegisterTypeInIl2Cpp<PolusButton>();
        }
        
        // private void Start() {
        //     pno = IObjectManager.Instance.LocateNetObject(this);
        //     pno.OnData = Deserialize;
        //     Renderer = GetComponent<SpriteRenderer>();
        // }
        //
        // private void FixedUpdate() {
        //     if (pno.HasSpawnData()) Deserialize(pno.GetSpawnData());
        // }
        //
        // private void Deserialize(MessageReader reader) {
        // }
    }
}