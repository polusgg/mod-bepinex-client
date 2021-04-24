using System;
using System.Collections;
using PolusGG.Extensions;
using TMPro;
using UnhollowerBaseLib.Attributes;
using UnhollowerRuntimeLib;
using UnityEngine;

namespace PolusGG.Behaviours {
    public class MaintenanceBehaviour : MonoBehaviour {
        public static MaintenanceBehaviour Instance;
        private static readonly GameObject disguisedToast;
        private CoroutineManager coroutineManager;
        private readonly float distance = 0.4f;
        private readonly float duration = 1f;

        static MaintenanceBehaviour() {
            ClassInjector.RegisterTypeInIl2Cpp<MaintenanceBehaviour>();
            disguisedToast = PogusPlugin.Bundle.LoadAsset("Assets/Mods/Generic UI/Toast.prefab").Cast<GameObject>()
                .DontDestroy();
            disguisedToast.SetActive(false);
            disguisedToast.layer = LayerMask.NameToLayer("UI");
            disguisedToast.transform.GetChild(0).gameObject.layer = LayerMask.NameToLayer("UI");
            disguisedToast.transform.GetChild(1).gameObject.layer = LayerMask.NameToLayer("UI");
        }

        public MaintenanceBehaviour(IntPtr ptr) : base(ptr) { }

        private void Start() {
            Instance = this;
            // ShowToast("Fortebased said your mom");
        }

        [HideFromIl2Cpp]
        public void ShowToast(string text) {
            this.StartCoroutine(CoStart(text));
        }

        [HideFromIl2Cpp]
        public IEnumerator CoStart(string text) {
            while (!HudManager.Instance) yield return null;
            GameObject toast = Instantiate(disguisedToast);
            toast.active = true;
            Transform toastransform = toast.transform;
            toastransform.parent = HudManager.Instance.transform;
            toastransform.localPosition =
                AspectPosition.ComputePosition(AspectPosition.EdgeAlignments.Top, new Vector3(0, -distance, 0));
            toastransform.localScale = new Vector3(0.5f, 0.5f, 1f);
            toast.GetComponentInChildren<TMP_Text>().text = text;

            Vector3 top =
                AspectPosition.ComputePosition(AspectPosition.EdgeAlignments.Top, new Vector3(0, -distance, -10f));
            Vector3 vec =
                AspectPosition.ComputePosition(AspectPosition.EdgeAlignments.Top, new Vector3(0, distance, -10f));
            yield return Effects.Slide2D(toastransform, top, vec, duration);

            toastransform.localPosition = vec;

            yield return new WaitForSeconds(10f);

            top = AspectPosition.ComputePosition(AspectPosition.EdgeAlignments.Top, new Vector3(0, distance, -10f));
            vec = AspectPosition.ComputePosition(AspectPosition.EdgeAlignments.Top, new Vector3(0, -distance, -10f));
            yield return Effects.Slide2D(toastransform, top, vec, duration);

            Destroy(toast);
        }
    }
}