using System;
using System.Collections;
using System.Collections.Generic;
using Hazel;
using PolusGG.Extensions;
using UnhollowerRuntimeLib;
using UnityEngine;

namespace PolusGG.Behaviours {
    public class PlayerAnimPlayer : MonoBehaviour {
        public PlayerControl Player;

        static PlayerAnimPlayer() {
            ClassInjector.RegisterTypeInIl2Cpp<PlayerAnimPlayer>();
        }

        public PlayerAnimPlayer(IntPtr ptr) : base(ptr) { }

        private void Start() {
            Player = GetComponent<PlayerControl>();
        }

        public void HandleMessage(MessageReader reader) {
            //todo handle player messages reading like camera controller
            List<PlayerKeyframe> playerKeyframes = new();
            while (reader.Position < reader.Length - 1) {
                MessageReader message = reader.ReadMessage();
                playerKeyframes.Add(new PlayerKeyframe (
                     message.ReadPackedUInt32(),
                    message.ReadPackedUInt32(),
                    message.ReadSingle(),
                     message.ReadSingle(),
                    message.ReadSingle(),
                     message.ReadSingle(),
                     new Color32(reader.ReadByte(), reader.ReadByte(), reader.ReadByte(),
                         reader.ReadByte()),
                     new Color32(reader.ReadByte(), reader.ReadByte(), reader.ReadByte(),
                         reader.ReadByte()),
                     new Color32(reader.ReadByte(), reader.ReadByte(), reader.ReadByte(),
                        reader.ReadByte()),
                    message.ReadVector2(),
                     message.ReadVector2(),
                    message.ReadSingle()
                ));
            }

            this.StartCoroutine(CoPlayAnimation(playerKeyframes.ToArray(), reader.ReadBoolean()));
        }

        public IEnumerator CoPlayAnimation(PlayerKeyframe[] frames, bool reset) {
            PlayerKeyframe resetTo = SerializeCurrentState();
            PlayerKeyframe previous = resetTo;
            PlayerKeyframe current;

            for (int i = 0; i < frames.Length; i++) {
                current = frames[i];

                yield return new WaitForSeconds(current.Offset / 1000f);
                yield return Effects.Lerp(current.Duration / 1000f, new Action<float>(dt => {
                    current.PlayerOpacity = Mathf.Lerp(previous.PlayerOpacity, current.PlayerOpacity, dt);
                    current.HatOpacity = Mathf.Lerp(previous.HatOpacity, current.HatOpacity, dt);
                    current.PetOpacity = Mathf.Lerp(previous.PetOpacity, current.PetOpacity, dt);
                    current.SkinOpacity = Mathf.Lerp(previous.SkinOpacity, current.SkinOpacity, dt);
                    current.SetPlayerColors(
                        Color.Lerp(previous.MainColor, current.MainColor, dt),
                        Color.Lerp(previous.ShadowColor, current.ShadowColor, dt),
                        Color.Lerp(previous.VisorColor, current.VisorColor, dt)
                    );
                    current.Scale = Vector2.Lerp(previous.Scale, current.Scale, dt);
                    current.Position = Vector2.Lerp(previous.Position, current.Position, dt);
                    current.Angle = Mathf.Lerp(previous.Angle, current.Angle, dt);
                }));

                previous = current;
            }

            if (reset) {
                resetTo.PlayerOpacity = resetTo.PlayerOpacity;
                resetTo.HatOpacity = resetTo.HatOpacity;
                resetTo.PetOpacity = resetTo.PetOpacity;
                resetTo.SkinOpacity = resetTo.SkinOpacity;
                resetTo.SetPlayerColors(resetTo.MainColor, resetTo.ShadowColor, resetTo.VisorColor);
                resetTo.Scale = resetTo.Scale;
                resetTo.Position = resetTo.Position;
                resetTo.Angle = resetTo.Angle;
            }
        }

        private PlayerKeyframe SerializeCurrentState() {
            throw new NotImplementedException();
        }

        public class PlayerKeyframe {
            private PlayerControl playerControl;
            public uint Offset;
            public uint Duration;

            private float playerOpacity;
            private float hatOpacity;
            private float petOpacity;
            private float skinOpacity;
            private Color32 mainColor;
            private Color32 shadowColor;
            private Color32 visorColor;
            private Vector2 scale;
            private Vector2 position;
            private float angle;

            public PlayerKeyframe(uint offset, uint duration, float playerOpacity, float hatOpacity, float petOpacity, float skinOpacity, Color32 mainColor, Color32 shadowColor, Color32 visorColor, Vector2 scale, Vector2 position, float angle) {
                Offset = offset;
                Duration = duration;
                this.playerOpacity = playerOpacity;
                this.hatOpacity = hatOpacity;
                this.petOpacity = petOpacity;
                this.skinOpacity = skinOpacity;
                this.mainColor = mainColor;
                this.shadowColor = shadowColor;
                this.visorColor = visorColor;
                this.scale = scale;
                this.position = position;
                this.angle = angle;
            }

            public float PlayerOpacity {
                get => playerOpacity;
                set => playerControl.myRend.color = new Color(1f, 1f, 1f, value);
            }

            public float HatOpacity {
                get => hatOpacity;
                set => playerControl.HatRenderer.color = new Color(1f, 1f, 1f, value);
            }

            public float PetOpacity {
                get => petOpacity;
                set => playerControl.CurrentPet.rend.color = new Color(1f, 1f, 1f, value);
            }

            public float SkinOpacity {
                get => skinOpacity;
                set => playerControl.MyPhysics.Skin.layer.color = new Color(1f, 1f, 1f, value);
            }

            public Color MainColor => mainColor;
            public Color ShadowColor => shadowColor;
            public Color VisorColor => visorColor;

            public Vector2 Scale {
                get => scale;
                set {
                    Transform transform1 = playerControl.transform;
                    Vector3 ea = value;
                    transform1.eulerAngles = ea;
                }
            }

            public Vector2 Position {
                get => position;
                set => playerControl.transform.localPosition = value;
            }

            public float Angle {
                get => angle;
                set {
                    Transform transform1 = playerControl.transform;
                    Vector3 ea = transform1.eulerAngles;
                    ea.y = value;
                    transform1.eulerAngles = ea;
                }
            }

            public void SetPlayerColors(Color playerColor, Color shadowColor, Color visorColor) {
                playerControl.myRend.material.SetColor("_BackColor", playerColor);
                playerControl.myRend.material.SetColor("_BodyColor", shadowColor);
                playerControl.myRend.material.SetColor("_VisorColor", visorColor);
            }
        }
    }
}