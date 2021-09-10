using System;
using System.Collections;
using TMPro;
using UnhollowerRuntimeLib;
using UnityEngine;

namespace Polus.Behaviours {
    //why is innersloth this fucking sloth-like
    public class RegionTextMonitorTMP : MonoBehaviour {
        static RegionTextMonitorTMP() {
            ClassInjector.RegisterTypeInIl2Cpp<RegionTextMonitorTMP>();
        }
        public RegionTextMonitorTMP(IntPtr ptr) : base(ptr) {}
        public void Start() {
            // innersloth fixed the funny issue so i'm commenting this out
            // this.StartCoroutine(ReloadText());
        }

        private IEnumerator ReloadText() {
            while (DestroyableSingleton<ServerManager>.Instance.CurrentRegion == null)
                yield return null;

            GetComponent<TextMeshPro>().text = DestroyableSingleton<ServerManager>.Instance.CurrentRegion.Name;
        }
    }
}