using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using Hazel;
using Polus.Enums;
using Polus.Extensions;
using Polus.Net.Objects;
using UnhollowerRuntimeLib;
using UnityEngine;

namespace Polus.Behaviours.Inner {
    public class PolusCameraController : PnoBehaviour {
        private Camera camera;
        private FollowerCamera follower;
        private SpriteRenderer fullScreen;
        public static readonly LayerMask DefaultLayer = LayerMask.NameToLayer("Default");

        static PolusCameraController() {
            ClassInjector.RegisterTypeInIl2Cpp<PolusCameraController>();
        }

        public PolusCameraController(IntPtr ptr) : base(ptr) { }

        private void Start() {
            follower = HudManager.Instance.PlayerCam;
            camera = follower.GetComponent<Camera>();
            fullScreen = Instantiate(HudManager.Instance.FullScreen.gameObject, HudManager.Instance.transform, true).GetComponent<SpriteRenderer>();
            fullScreen.name = "FullScreen499";
            fullScreen.gameObject.layer = DefaultLayer;
            fullScreen.transform.position += new Vector3(0, 0, 1);
        }

        private void Update() {
            if (pno.HasData()) Deserialize(pno.GetData());
            if (pno.HasRpc()) HandleRpc(pno.GetRpcData());
            if (fullScreen.transform.parent == null && HudManager.Instance) {
                fullScreen.transform.parent = HudManager.Instance.transform;
            }
        }

        private void HandleRpc(PolusNetObject.Rpc rpc) {
            if (rpc.CallId != (int) PolusRpcCalls.BeginAnimationCamera) return;
            List<CameraKeyframe> keyframe = new();
            while (rpc.Reader.Position < rpc.Reader.Length - 1) {
                MessageReader message = rpc.Reader.ReadMessage();
                keyframe.Add(new CameraKeyframe(
                    message.ReadPackedUInt32(),
                    message.ReadPackedUInt32(),
                    message.ReadVector2(),
                    message.ReadSingle(),
                    new Color32(message.ReadByte(), message.ReadByte(), message.ReadByte(), message.ReadByte()),
                    follower,
                    camera,
                    this
                ));
            }

            this.StartCoroutine(CoPlayAnimation(keyframe.ToArray(), rpc.Reader.ReadBoolean()));
        }

        private CameraKeyframe SerializeCurrentState() {
            return new(
                0,
                0,
                follower.Offset,
                Camera.main.transform.eulerAngles.z,
                fullScreen.color,
                follower,
                camera,
                this
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
                    Color color = Color.Lerp(previous.OverlayColor, current.OverlayColor, dt);
                    fullScreen.enabled = color.a != 0;
                    current.OverlayColor = color;
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
            private PolusCameraController controller;

            public CameraKeyframe(uint animOffset, uint duration, Vector2 cameraOffset, float angle, Color32 color,
                FollowerCamera followerCamera, Camera camera, PolusCameraController controller) {
                Offset = animOffset;
                Duration = duration;
                this.cameraOffset = cameraOffset;
                this.angle = angle;
                overlayColor = color;
                FollowerCamera = followerCamera;
                Camera = camera;
                this.controller = controller;
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
                set => controller.fullScreen.color = value;
            }

            public override string ToString() {
                StringBuilder builder = new();
                Type type = GetType();
                builder.Append(type.Name).Append(" {\n");
                PropertyInfo[] props = type.GetProperties();
                foreach (PropertyInfo propertyInfo in props) {
                    builder.Append("  ").Append(propertyInfo.Name).Append(" = ").Append(propertyInfo.GetValue(this)).Append("\n");
                }

                builder.Append("}");
                return builder.ToString();
            }
        }
    }
}