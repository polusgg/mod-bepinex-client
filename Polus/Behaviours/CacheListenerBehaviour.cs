using System;
using Polus.Resources;
using UnhollowerRuntimeLib;
using UnityEngine;

namespace Polus.Behaviours {
    public class CacheListenerBehaviour : MonoBehaviour {
        static CacheListenerBehaviour() => ClassInjector.RegisterTypeInIl2Cpp<CacheListenerBehaviour>();
        public CacheListenerBehaviour(IntPtr ptr) : base(ptr) {}
        private CacheListener listener;
        public CacheListenerBehaviour Initialize(CacheListener cacheListener) {
            listener = cacheListener;
            return this;
        }

        private void OnDestroy() {
            listener?.Dispose();
        }
    }
}