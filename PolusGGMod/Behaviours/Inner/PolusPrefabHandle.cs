#if no
using System;
using PolusGG.Net;
using UnhollowerRuntimeLib;
using UnityEngine;

namespace PolusGG.Behaviours.Inner {
    public class PolusPrefabHandle : PnoBehaviour {
        static PolusPrefabHandle() {
            ClassInjector.RegisterTypeInIl2Cpp<PolusPrefabHandle>();
        }

        public PolusPrefabHandle(IntPtr ptr) : base(ptr) { }

        private void Start() {
            pno = PogusPlugin.ObjectManager.LocateNetObject(this);
            if (pno.HasRpc())
                Instantiate(PogusPlugin.Cache.CachedFiles[pno.GetSpawnData().ReadPackedUInt32()].Get<GameObject>(),
                    transform);
        }
    }
}
#endif