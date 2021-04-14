using System;
using PolusGG.Net;
using UnhollowerRuntimeLib;
using UnityEngine;

namespace PolusGG.Behaviours.Inner {
    public class PolusPrefabHandle : PnoBehaviour {
        public PolusPrefabHandle(IntPtr ptr) : base(ptr) { }

        static PolusPrefabHandle() {
            ClassInjector.RegisterTypeInIl2Cpp<PolusPrefabHandle>();
        }

        private void FixedUpdate() {
            if (pno.HasSpawnData()) {
                Instantiate(PogusPlugin.Cache.CachedFiles[pno.GetSpawnData().ReadPackedUInt32()].Get<GameObject>(), transform);
            }
        }
    }
}