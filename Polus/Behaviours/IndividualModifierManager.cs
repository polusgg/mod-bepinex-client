using System;
using UnhollowerRuntimeLib;
using UnityEngine;

namespace Polus.Behaviours {
    public class IndividualModifierManager : MonoBehaviour {
        static IndividualModifierManager() {
            ClassInjector.RegisterTypeInIl2Cpp<IndividualModifierManager>();
        }
        
        public IndividualModifierManager(IntPtr ptr) : base(ptr) { }

        public float SpeedModifer { get; set; } = 1.0f;

        public float VisionModifier { get; set; } = 1.0f;
    }
}