using System;
using UnhollowerBaseLib.Attributes;
using UnhollowerRuntimeLib;
using UnityEngine;

namespace Polus.Behaviours {
    public class IndividualModifierManager : MonoBehaviour {
        static IndividualModifierManager() {
            ClassInjector.RegisterTypeInIl2Cpp<IndividualModifierManager>();
        }
        
        public IndividualModifierManager(IntPtr ptr) : base(ptr) { }

        [HideFromIl2Cpp]
        public float SpeedModifer { get; set; } = 1.0f;

        [HideFromIl2Cpp]
        public float VisionModifier { get; set; } = 1.0f;
    }
}