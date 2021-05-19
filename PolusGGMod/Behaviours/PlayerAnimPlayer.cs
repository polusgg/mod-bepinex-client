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
            bool[] field = BitfieldParser(reader.ReadUInt16(), 10);
            while (reader.Position < reader.Length - 1) {
                MessageReader message = reader.ReadMessage();
                uint offset = message.ReadPackedUInt32();
                uint duration = message.ReadPackedUInt32();
                
                playerKeyframes.Add(new PlayerKeyframe(
                    offset,
                    duration,
                    field[0] ? message.ReadSingle() : null,
                    field[1] ? message.ReadSingle() : null,
                    field[2] ? message.ReadSingle() : null,
                    field[3] ? message.ReadSingle() : null,
                    field[4] ? new Color32(message.ReadByte(), message.ReadByte(), message.ReadByte(),
                        message.ReadByte()) : null,
                    field[5] ? new Color32(message.ReadByte(), message.ReadByte(), message.ReadByte(),
                        message.ReadByte()) : null,
                    field[6] ? new Color32(message.ReadByte(), message.ReadByte(), message.ReadByte(),
                        message.ReadByte()) : null,
                    field[7] ? message.ReadVector2() : null,
                    field[8] ? message.ReadVector2() : null,
                    field[9] ? message.ReadSingle() : null,
                    Player,
                    this
                ));
            }

            this.StartCoroutine(CoPlayAnimation(field, playerKeyframes.ToArray(), reader.ReadBoolean()));
        }

        [SuppressMessage("ReSharper", "AccessToModifiedClosure")]
        [SuppressMessage("ReSharper", "PossibleInvalidOperationException")]
        private IEnumerator CoPlayAnimation(bool[] field, PlayerKeyframe[] frames, bool reset) {
            PlayerKeyframe resetTo = SerializeCurrentState();
            PlayerKeyframe previous = resetTo;

            foreach (PlayerKeyframe current in frames) {

                yield return new WaitForSeconds(current.Offset / 1000f);
                yield return Effects.Lerp(current.Duration / 1000f, new Action<float>(dt => {
                    if (field[0]) current.PlayerOpacity = Mathf.Lerp(previous.PlayerOpacity.Value, current.PlayerOpacity.Value, dt);
                    if (field[1]) current.HatOpacity = Mathf.Lerp(previous.HatOpacity.Value, current.HatOpacity.Value, dt);
                    if (field[2]) current.PetOpacity = Mathf.Lerp(previous.PetOpacity.Value, current.PetOpacity.Value, dt);
                    if (field[3]) current.SkinOpacity = Mathf.Lerp(previous.SkinOpacity.Value, current.SkinOpacity.Value, dt);
                    current.SetPlayerColors(
                        field[4] ? Color.Lerp(previous.MainColor.Value, current.MainColor.Value, dt) : null,
                        field[5] ? Color.Lerp(previous.ShadowColor.Value, current.ShadowColor.Value, dt) : null,
                        field[6] ? Color.Lerp(previous.VisorColor.Value, current.VisorColor.Value, dt) : null
                    );
                    if (field[7]) current.Scale = Vector2.Lerp(previous.Scale.Value, current.Scale.Value, dt);
                    if (field[8]) current.Position = Vector2.Lerp(previous.Position.Value, current.Position.Value, dt);
                    if (field[9]) current.Angle = Mathf.Lerp(previous.Angle.Value, current.Angle.Value, dt);
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

        public bool[] BitfieldParser(ushort value, byte size) {
            bool[] output = new bool[size];
            for (byte i = 0; i < size; i++) output[i] = (value & (1 << i)) != 0;
            return output;
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
            public uint Offset;
            public uint Duration;

            private readonly float? _playerOpacity;
            private readonly float? _hatOpacity;
            private readonly float? _petOpacity;
            private readonly float? _skinOpacity;
            private readonly Color32? _mainColor;
            private readonly Color32? _shadowColor;
            private readonly Color32? _visorColor;
            private readonly Vector2? _position;
            private readonly Vector2? _scale;
            private readonly float? _angle;

            private readonly PlayerAnimPlayer _animPlayer;
            private readonly PlayerControl _playerControl;

            public PlayerKeyframe(uint offset, uint duration, float? playerOpacity, float? hatOpacity, float? petOpacity,
                float? skinOpacity, Color32? mainColor, Color32? shadowColor, Color32? visorColor, Vector2? scale,
                Vector2? position, float? angle, PlayerControl playerControl, PlayerAnimPlayer animPlayer) {
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

            public float? PlayerOpacity {
                get => _playerOpacity;
                set {
                    if (value != null) _animPlayer._playerColor.a = value.Value;
                }
            }

            public float? HatOpacity {
                get => _hatOpacity ?? 0;
                set {
                    if (value != null) _animPlayer._hatColor.a = value.Value;
                }
            }

            public float? PetOpacity {
                get => _petOpacity ?? 0;
                set {
                    if (value != null) _animPlayer._petColor.a = value.Value;
                }
            }

            public float? SkinOpacity {
                get => _skinOpacity ?? 0;
                set {
                    if (value != null) _animPlayer._skinColor.a = value.Value;
                }
            }

            public Color? MainColor => _mainColor;
            public Color? ShadowColor => _shadowColor;
            public Color? VisorColor => _visorColor;

            public Vector2? Scale {
                get => _scale ?? Vector2.zero;
                set {
                    if (value == null) return;
                    Transform transform1 = _playerControl.transform;
                    transform1.eulerAngles = value.Value;
                }
            }

            //what classifies as position?
            //how do i implement a set for this?
            //do i move the sprite renderers? how do i do that??
            public Vector2? Position => _position;

            public float? Angle {
                get => _angle;
                set {
                    if (value == null) return;
                    Transform transform1 = _playerControl.transform;
                    Vector3 ea = transform1.eulerAngles;
                    ea.y = value.Value;
                    transform1.eulerAngles = ea;
                }
            }

            public void SetPlayerColors(Color? playerColor, Color? shadowColor, Color? visorColor) {
                if (playerColor.HasValue) _playerControl.myRend.material.SetColor(BackColorID, playerColor.Value);
                if (shadowColor.HasValue) _playerControl.myRend.material.SetColor(BodyColorID, shadowColor.Value);
                if (visorColor.HasValue) _playerControl.myRend.material.SetColor(VisorColorID, visorColor.Value);
            }
        }
    }
}