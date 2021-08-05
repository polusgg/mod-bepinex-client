using System;
using PolusggSlim.Utils.Attributes;
using UnityEngine;
using Coroutine = PolusggSlim.Coroutines.Coroutine;

namespace PolusggSlim.Coroutines
{
    [RegisterInIl2Cpp]
    public class CoroutineProcessor : MonoBehaviour
    {
        public CoroutineProcessor(IntPtr ptr) : base(ptr)
        {
        }

        public void Start()
        {
            Camera.onPostRender = new Action<Camera>(_ => { Coroutine.ProcessWaitForEndOfFrame(); });
        }

        public void Update()
        {
            Coroutine.Process();
        }

        public void FixedUpdate()
        {
            Coroutine.ProcessWaitForFixedUpdate();
        }

        public void OnDestroy()
        {
            foreach (var coroTuple in Coroutine.CoroutinesStore) Coroutine.Stop(coroTuple.Coroutine);
        }
    }
}