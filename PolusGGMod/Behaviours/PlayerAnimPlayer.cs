using System;
using UnhollowerRuntimeLib;
using UnityEngine;

namespace PolusGG.Behaviours {
    public class PlayerAnimPlayer : MonoBehaviour {
        static PlayerAnimPlayer() {
            ClassInjector.RegisterTypeInIl2Cpp<MaintenanceBehaviour>();
        }

        public PlayerAnimPlayer(IntPtr ptr) : base(ptr) {}
    }
}