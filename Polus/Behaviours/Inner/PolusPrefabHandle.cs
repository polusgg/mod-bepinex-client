using System;
using Hazel;
using Polus.Enums;
using Polus.Net.Objects;
using UnhollowerRuntimeLib;
using UnityEngine;

namespace Polus.Behaviours.Inner {
    public class PolusPrefabHandle : PnoBehaviour {
        static PolusPrefabHandle() {
            ClassInjector.RegisterTypeInIl2Cpp<PolusPrefabHandle>();
        }

        public PolusPrefabHandle(IntPtr ptr) : base(ptr) { }

        private void Update() {
            if (pno.GetData() is MessageReader reader) {
                Instantiate(PogusPlugin.Cache.CachedFiles[reader.ReadPackedUInt32()].Get<GameObject>(),
                    transform); //
            }
        }
    }
}