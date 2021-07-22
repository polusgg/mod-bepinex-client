using System;
using UnhollowerRuntimeLib;
using UnityEngine;

namespace Polus.Behaviours {
    // fuck you ConditionalHide for being so garbage
    public class UnconditionalHide : MonoBehaviour{
        static UnconditionalHide() {
            ClassInjector.RegisterTypeInIl2Cpp<UnconditionalHide>();
        }
        public UnconditionalHide(IntPtr ptr) : base(ptr) { }

        public GameObject WhatToHide;

        private void Start() {
            OnEnable();
        }

        private void OnEnable() {
            gameObject.SetActive(false);
        }
    }
}