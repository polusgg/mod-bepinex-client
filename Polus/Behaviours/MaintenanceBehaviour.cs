#nullable enable
using System;
using System.Collections;
using System.Collections.Generic;
using Polus.Extensions;
using TMPro;
using UnhollowerBaseLib.Attributes;
using UnhollowerRuntimeLib;
using UnityEngine;
using UnityEngine.Serialization;

namespace Polus.Behaviours {
    public class MaintenanceBehaviour : MonoBehaviour {
        private static readonly GameObject DisguisedToast;
        private const float Distance = 0.4f;
        private const float Duration = 1f;
        private readonly Queue<(string text, float time)> notificationQueue = new();
        public bool coroutineRunning;

        static MaintenanceBehaviour() {
            ClassInjector.RegisterTypeInIl2Cpp<MaintenanceBehaviour>();
            DisguisedToast = PogusPlugin.Bundle.LoadAsset("Assets/Mods/UI/Toast.prefab").Cast<GameObject>()
                .DontDestroy();
            DisguisedToast.SetActive(false);
            DisguisedToast.layer = LayerMask.NameToLayer("UI");
            DisguisedToast.transform.GetChild(0).gameObject.layer = LayerMask.NameToLayer("UI");
            DisguisedToast.transform.GetChild(1).gameObject.layer = LayerMask.NameToLayer("UI");
        }

        public MaintenanceBehaviour(IntPtr ptr) : base(ptr) { }

        private void Update() {
            if (coroutineRunning || notificationQueue.Count == 0) return;
            coroutineRunning = true;
            (string text, float time) = notificationQueue.Dequeue();
            this.StartCoroutine(CoStart(text, time));
        }

        [HideFromIl2Cpp]
        public void ShowToast(string text, float time = 10f) {
            notificationQueue.Enqueue((text, time));
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
                AspectPosition.ComputePosition(AspectPosition.EdgeAlignments.Top, new Vector3(0, -Distance, -10f));
            toastransform.localScale = new Vector3(0.5f, 0.5f, 1f);
            toast.GetComponentInChildren<TMP_Text>().text = text;

            Vector3 top =
                AspectPosition.ComputePosition(AspectPosition.EdgeAlignments.Top, new Vector3(0, -Distance, -10f));
            Vector3 vec =
                AspectPosition.ComputePosition(AspectPosition.EdgeAlignments.Top, new Vector3(0, Distance, -10f));

            yield return Effects.Slide2D(toastransform, top, vec, Duration);

            toastransform.localPosition = vec;

            yield return new WaitForSeconds(time);

            top = AspectPosition.ComputePosition(AspectPosition.EdgeAlignments.Top, new Vector3(0, Distance, -10f));
            vec = AspectPosition.ComputePosition(AspectPosition.EdgeAlignments.Top, new Vector3(0, -Distance, -10f));
            yield return Effects.Slide2D(toastransform, top, vec, Duration);

            DestroyImmediate(toast);
            coroutineRunning = false;
        }
    }
}