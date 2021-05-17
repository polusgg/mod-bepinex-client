using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Hazel;
using PolusGG.Extensions;
using UnhollowerRuntimeLib;
using UnityEngine;

namespace PolusGG.Behaviours {
    public class PlayerAnimPlayer : MonoBehaviour {
        public PlayerControl Player;
        private static readonly int BackColorID = Shader.PropertyToID("_BackColor");
        private static readonly int BodyColorID = Shader.PropertyToID("_BodyColor");
        private static readonly int VisorColorID = Shader.PropertyToID("_VisorColor");
        private Color _playerColor;
        private Color _hatColor;
        private Color _petColor;
        private Color _skinColor;

        static PlayerAnimPlayer() {
            ClassInjector.RegisterTypeInIl2Cpp<PlayerAnimPlayer>();
        }

        public PlayerAnimPlayer(IntPtr ptr) : base(ptr) { }

        private void Start() {
            Player = GetComponent<PlayerControl>();
        }

        private void Update() {
            Player.myRend.color = _playerColor;
            Player.HatRenderer.color = _hatColor;
            Player.MyPhysics.rend.color = _skinColor;
            if (Player.CurrentPet) Player.CurrentPet.rend.color = _petColor;
        }

        public void HandleMessage(MessageReader reader) {
            //todo handle player messages reading like camera controller
            List<PlayerKeyframe> playerKeyframes = new();
            while (reader.Position < reader.Length - 1) {
                MessageReader message = reader.ReadMessage();
                playerKeyframes.Add(new PlayerKeyframe(
                    message.ReadPackedUInt32(),
                    message.ReadPackedUInt32(),
                    message.ReadSingle(),
                    message.ReadSingle(),
                    message.ReadSingle(),
                    message.ReadSingle(),
                    new Color32(message.ReadByte(), message.ReadByte(), message.ReadByte(),
                        message.ReadByte()),
                    new Color32(message.ReadByte(), message.ReadByte(), message.ReadByte(),
                        message.ReadByte()),
                    new Color32(message.ReadByte(), message.ReadByte(), message.ReadByte(),
                        message.ReadByte()),
                    message.ReadVector2(),
                    message.ReadVector2(),
                    message.ReadSingle(),
                    Player,
                    this
                ));
            }

            this.StartCoroutine(CoPlayAnimation(playerKeyframes.ToArray(), reader.ReadBoolean()));
        }

        [SuppressMessage("ReSharper", "AccessToModifiedClosure")]
        private IEnumerator CoPlayAnimation(PlayerKeyframe[] frames, bool reset) {
            PlayerKeyframe resetTo = SerializeCurrentState();
            PlayerKeyframe previous = resetTo;

            foreach (PlayerKeyframe current in frames) {

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

            if (!reset) yield break;
            resetTo.PlayerOpacity = resetTo.PlayerOpacity;
            resetTo.HatOpacity = resetTo.HatOpacity;
            resetTo.PetOpacity = resetTo.PetOpacity;
            resetTo.SkinOpacity = resetTo.SkinOpacity;
            resetTo.SetPlayerColors(resetTo.MainColor, resetTo.ShadowColor, resetTo.VisorColor);
            resetTo.Scale = resetTo.Scale;
            resetTo.Position = resetTo.Position;
            resetTo.Angle = resetTo.Angle;
        }

        private PlayerKeyframe SerializeCurrentState() {
            return new(
                0,
                0,
                Player.myRend.color.a,
                Player.HatRenderer.FrontLayer.color.a,
                Player.CurrentPet.rend.color.a,
                Player.MyPhysics.Skin.layer.color.a,
                Player.myRend.material.GetColor(BackColorID),
                Player.myRend.material.GetColor(BodyColorID),
                Player.myRend.material.GetColor(VisorColorID),
                Player.transform.localScale,
                Player.transform.position,
                Player.transform.rotation.eulerAngles.y,
                Player,
                this
            );
        }

        public class PlayerKeyframe {
            private readonly float _angle;
            public uint Duration;
            private readonly float _hatOpacity;
            private readonly Color32 _mainColor;
            public uint Offset;
            private readonly float _petOpacity;
            private readonly PlayerControl _playerControl;
            private readonly PlayerAnimPlayer _animPlayer;

            private readonly float _playerOpacity;
            private readonly Vector2 _position;
            private readonly Vector2 _scale;
            private readonly Color32 _shadowColor;
            private readonly float _skinOpacity;
            private readonly Color32 _visorColor;

            public PlayerKeyframe(uint offset, uint duration, float playerOpacity, float hatOpacity, float petOpacity,
                float skinOpacity, Color32 mainColor, Color32 shadowColor, Color32 visorColor, Vector2 scale,
                Vector2 position, float angle, PlayerControl playerControl, PlayerAnimPlayer animPlayer) {
                Offset = offset;
                Duration = duration;
                _playerOpacity = playerOpacity;
                _hatOpacity = hatOpacity;
                _petOpacity = petOpacity;
                _skinOpacity = skinOpacity;
                _mainColor = mainColor;
                _shadowColor = shadowColor;
                _visorColor = visorColor;
                _scale = scale;
                _position = position;
                _angle = angle;
                _playerControl = playerControl;
                _animPlayer = animPlayer;
            }

            public float PlayerOpacity {
                get => _playerOpacity;
                set => _animPlayer._playerColor.a = value;
            }

            public float HatOpacity {
                get => _hatOpacity;
                set => _animPlayer._hatColor.a = value;
            }

            public float PetOpacity {
                get => _petOpacity;
                set => _animPlayer._petColor.a = value;
            }

            public float SkinOpacity {
                get => _skinOpacity;
                set => _animPlayer._skinColor.a = value;
            }

            public Color MainColor => _mainColor;
            public Color ShadowColor => _shadowColor;
            public Color VisorColor => _visorColor;

            public Vector2 Scale {
                get => _scale;
                set {
                    Transform transform1 = _playerControl.transform;
                    Vector3 ea = value;
                    transform1.eulerAngles = ea;
                }
            }

            public Vector2 Position {
                get => _position;
                set {
                    // _playerControl.myRend.;
                }
            }

            public float Angle {
                get => _angle;
                set {
                    Transform transform1 = _playerControl.transform;
                    Vector3 ea = transform1.eulerAngles;
                    ea.y = value;
                    transform1.eulerAngles = ea;
                }
            }

            public void SetPlayerColors(Color playerColor, Color shadowColor, Color visorColor) {
                _playerControl.myRend.material.SetColor("_BackColor", playerColor);
                _playerControl.myRend.material.SetColor("_BodyColor", shadowColor);
                _playerControl.myRend.material.SetColor("_VisorColor", visorColor);
            }
        }
    }
}