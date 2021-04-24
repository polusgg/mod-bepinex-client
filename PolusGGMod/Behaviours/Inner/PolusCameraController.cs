using System;
using System.Collections;
using System.Collections.Generic;
using Hazel;
using PolusGG.Enums;
using PolusGG.Extensions;
using PolusGG.Net;
using UnhollowerRuntimeLib;
using UnityEngine;

namespace PolusGG.Behaviours.Inner {
    public class PolusCameraController : PnoBehaviour {
        private Camera camera;
        private FollowerCamera follower;

        static PolusCameraController() {
            ClassInjector.RegisterTypeInIl2Cpp<PolusCameraController>();
        }

        public PolusCameraController(IntPtr ptr) : base(ptr) { }

        private void Start() {
            follower = HudManager.Instance.PlayerCam;
            camera = follower.GetComponent<Camera>();
            pno = PogusPlugin.ObjectManager.LocateNetObject(this);
            pno.OnData = Deserialize;
            pno.OnRpc = HandleRpc;
        }

        private void FixedUpdate() {
            if (pno.HasSpawnData()) Deserialize(pno.GetSpawnData());
        }

        private void HandleRpc(MessageReader reader, byte callid) {
            callid.Log(20, "camera fucking camera");
            if (callid != (int) PolusRpcCalls.BeginAnimationCamera) return;
            List<CameraKeyframe> keyframe = new();
            while (reader.Position < reader.Length - 1) {
                MessageReader message = reader.ReadMessage();
                keyframe.Add(new CameraKeyframe(
                    message.ReadPackedUInt32(),
                    message.ReadPackedUInt32(),
                    message.ReadVector2(),
                    message.ReadSingle(),
                    new Color32(reader.ReadByte(), reader.ReadByte(), reader.ReadByte(), reader.ReadByte()),
                    follower,
                    camera
                ));
            }

            this.StartCoroutine(CoPlayAnimation(keyframe.ToArray(), reader.ReadBoolean()));
        }

        private CameraKeyframe SerializeCurrentState() {
            return new(
                0,
                0,
                follower.Offset,
                Camera.main.transform.eulerAngles.z,
                HudManager.Instance.FullScreen.color,
                follower,
                camera
            );
        }

        private IEnumerator CoPlayAnimation(CameraKeyframe[] frames, bool reset) {
            CameraKeyframe resetTo = SerializeCurrentState();
            CameraKeyframe previous = resetTo;
            CameraKeyframe current;

            for (int i = 0; i < frames.Length; i++) {
                current = frames[i];

                yield return new WaitForSeconds(current.Offset / 1000f);
                yield return Effects.Lerp(current.Duration / 1000f, new Action<float>(dt => {
                    current.CameraOffset = Vector2.Lerp(previous.CameraOffset, current.CameraOffset, dt);
                    current.Angle = Mathf.Lerp(previous.Angle, current.Angle, dt);
                    current.OverlayColor = Color.Lerp(previous.OverlayColor, current.OverlayColor, dt);
                }));

                previous = current;
            }

            if (reset) {
                resetTo.CameraOffset = resetTo.CameraOffset;
                resetTo.Angle = resetTo.Angle;
                resetTo.OverlayColor = resetTo.OverlayColor;
            }
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

        public class CameraKeyframe {
            private readonly float angle;
            public Camera Camera;
            private readonly Vector2 cameraOffset;
            public uint Duration;
            public FollowerCamera FollowerCamera;
            public uint Offset;
            private readonly Color overlayColor;

            public CameraKeyframe(uint animOffset, uint duration, Vector2 cameraOffset, float angle, Color32 color,
                FollowerCamera followerCamera, Camera camera) {
                Offset = animOffset;
                Duration = duration;
                this.cameraOffset = cameraOffset;
                this.angle = angle;
                overlayColor = color;
                FollowerCamera = followerCamera;
                Camera = camera;
            }

            public Vector2 CameraOffset {
                get => cameraOffset;
                set => FollowerCamera.Offset = value;
            }

            public float Angle {
                get => angle;
                set {
                    Transform transform1 = Camera.transform;
                    Vector3 ea = transform1.eulerAngles;
                    ea.y = value;
                    transform1.eulerAngles = ea;
                }
            }

            public Color OverlayColor {
                get => overlayColor;
                set => HudManager.Instance.FullScreen.color = value;
            }
        }
    }
}