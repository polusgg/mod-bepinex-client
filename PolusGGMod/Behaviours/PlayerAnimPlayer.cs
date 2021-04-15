using System;
using System.Collections;
using Hazel;
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
        }

        public IEnumerator CoPlayAnimation() {
            yield break;
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

            public Color32 MainColor => mainColor;
            public Color32 ShadowColor => shadowColor;
            public Color32 VisorColor => visorColor;

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
            
            
        }
    }
}