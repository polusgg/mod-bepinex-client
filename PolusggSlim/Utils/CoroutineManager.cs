using System;
using PolusggSlim.Utils.Attributes;
using UnityEngine;

namespace PolusggSlim.Utils
{
    [RegisterInIl2Cpp]
    public class CoroutineProcessor : MonoBehaviour
    {
        public CoroutineProcessor(IntPtr ptr) : base(ptr)
        {
        }

        public void Start()
        {
            Camera.onPostRender = new Action<Camera>(camera => { Coroutine.ProcessWaitForEndOfFrame(); });
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
            foreach (var coroTuple in Coroutine.coroutinesStore) Coroutine.Stop(coroTuple.Coroutine);
        }
    }
}