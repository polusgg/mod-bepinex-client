using System;
using System.Collections;
using UnhollowerRuntimeLib;
using UnityEngine;

namespace PolusGG.Behaviours {
    public class PlayerAnimPlayer : MonoBehaviour {
        public PlayerControl Player;
        static PlayerAnimPlayer() {
            ClassInjector.RegisterTypeInIl2Cpp<PlayerAnimPlayer>();
        }

        public PlayerAnimPlayer(IntPtr ptr) : base(ptr) {}

        private void Start() {
            Player = GetComponent<PlayerControl>();
        }

        public IEnumerator CoPlayAnimation() {
            yield break;
        }

        public class PlayerKeyframe {
            public uint Offset;
            public uint Duration;
            
        }
    }
}