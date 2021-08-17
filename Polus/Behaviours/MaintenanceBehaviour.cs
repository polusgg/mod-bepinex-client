using System;
using System.Collections;
using System.Collections.Generic;
using Polus.Extensions;
using TMPro;
using UnhollowerBaseLib.Attributes;
using UnhollowerRuntimeLib;
using UnityEngine;

namespace Polus.Behaviours {
    public class MaintenanceBehaviour : MonoBehaviour {
        public static MaintenanceBehaviour Instance;
        public static bool InstanceExists => Instance != null;
        private static readonly GameObject DisguisedToast;
        private readonly float distance = 0.4f;
        private readonly float duration = 1f;
        private readonly Queue<(string text, float time)> _notificationQueue = new();
        public bool CoroutineRunning;

        static MaintenanceBehaviour() {
            ClassInjector.RegisterTypeInIl2Cpp<MaintenanceBehaviour>();
            DisguisedToast = PogusPlugin.Bundle.LoadAsset("Assets/Mods/Generic UI/Toast.prefab").Cast<GameObject>()
                .DontDestroy();
            DisguisedToast.SetActive(false);
            DisguisedToast.layer = LayerMask.NameToLayer("UI");
            DisguisedToast.transform.GetChild(0).gameObject.layer = LayerMask.NameToLayer("UI");
            DisguisedToast.transform.GetChild(1).gameObject.layer = LayerMask.NameToLayer("UI");
        }

        public MaintenanceBehaviour(IntPtr ptr) : base(ptr) { }

        private void Start() {
            Instance = this;
            // ShowToast("Hello world");
        }

        private void Update() {
            if (CoroutineRunning || _notificationQueue.Count == 0) return;
            CoroutineRunning = true;
            (string text, float time) = _notificationQueue.Dequeue();
            this.StartCoroutine(CoStart(text, time));
        }

        [HideFromIl2Cpp]
        public void ShowToast(string text, float time = 10f) {
            _notificationQueue.Enqueue((text, time));
        }

        [HideFromIl2Cpp]
        public IEnumerator CoStart(string text, float time) {
            while (!HudManager.InstanceExists) yield return null;
            text.Log(comment: "Maintenance message issued POG");
            GameObject toast = Instantiate(DisguisedToast);
            toast.active = true;
            Transform toastransform = toast.transform;
            if (HudManager.InstanceExists) toastransform.parent = HudManager.Instance.transform;
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

            yield return new WaitForSeconds(time);

            top = AspectPosition.ComputePosition(AspectPosition.EdgeAlignments.Top, new Vector3(0, distance, -10f));
            vec = AspectPosition.ComputePosition(AspectPosition.EdgeAlignments.Top, new Vector3(0, -distance, -10f));
            yield return Effects.Slide2D(toastransform, top, vec, duration);

            DestroyImmediate(toast);
            CoroutineRunning = false;
        }
    }
}