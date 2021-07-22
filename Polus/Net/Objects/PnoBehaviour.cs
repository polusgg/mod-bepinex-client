using System;
using UnhollowerRuntimeLib;
using UnityEngine;

namespace Polus.Net.Objects {
    public class PnoBehaviour : MonoBehaviour {
        public PolusNetObject pno;

        static PnoBehaviour() {
            ClassInjector.RegisterTypeInIl2Cpp<PnoBehaviour>();
        }

        public PnoBehaviour(IntPtr ptr) : base(ptr) { }

        // private void Awake() {
        //     this.StartCoroutine(CoSetPnoSiblings());
        // }

        // private IEnumerator CoSetPnoSiblings() {
        //     PnoBehaviour[] componentsInChildren = GetComponentsInChildren<PnoBehaviour>();
        //     while (componentsInChildren.Any(p => (p.pno == null).Log())) yield return null;
        //     foreach (PnoBehaviour p in componentsInChildren) {
        //         p.pno.Siblings = componentsInChildren.Where(x => x.pno.NetId != pno.NetId)
        //             .Select(x => x.pno.NetId.Log()).ToArray();
        //     }
        // }
    }
}