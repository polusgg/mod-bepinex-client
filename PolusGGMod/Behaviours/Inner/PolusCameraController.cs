using System;
using Hazel;
using PolusGG.Extensions;
using PolusGG.Net;
using UnhollowerRuntimeLib;
using UnityEngine;

namespace PolusGG.Inner {
    public class PolusCameraController : PnoBehaviour {
        public PolusCameraController(IntPtr ptr) : base(ptr) { }

        static PolusCameraController() {
            ClassInjector.RegisterTypeInIl2Cpp<PolusCameraController>();
        }

        private void Start() {
            pno = IObjectManager.Instance.LocateNetObject(this);
            pno.OnData = Deserialize;
            pno.OnRpc = HandleRpc;
        }

        private void HandleRpc(MessageReader reader, byte callid) {
            
        }

        private void FixedUpdate() {
            if (pno.HasSpawnData()) Deserialize(pno.GetSpawnData());
        }

        private void Deserialize(MessageReader reader) {
            // Camera hudCamera = HudManager.Instance.GetComponentInChildren<Camera>();
            // Camera camera = Camera.main;
            // Camera shadowCamera = FindObjectOfType<ShadowCamera>().GetComponent<Camera>();
            // HudManager.Instance.ShadowQuad.transform.localScale = 
            // shadowCamera.orthographicSize = camera.orthographicSize = hudCamera.orthographicSize = reader.ReadSingle();
            // ResolutionManager.ResolutionChanged.Invoke((float)Screen.width / Screen.height);
            // todo add scale later on
            HudManager.Instance.PlayerCam.Offset = reader.ReadVector2();
        }
    }
}