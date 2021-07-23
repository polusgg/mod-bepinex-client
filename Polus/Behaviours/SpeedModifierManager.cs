using System;
using UnhollowerRuntimeLib;
using UnityEngine;

namespace Polus.Behaviours {
    public class SpeedModifierManager : MonoBehaviour {
        static SpeedModifierManager() {
            ClassInjector.RegisterTypeInIl2Cpp<SpeedModifierManager>();
        }
        
        public SpeedModifierManager(IntPtr ptr) : base(ptr) { }

        public float SpeedModifer { get; set; } = 1.0f;
    }
}