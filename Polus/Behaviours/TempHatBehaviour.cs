using System;
using UnhollowerRuntimeLib;

namespace Polus.Behaviours {
    public class TempHatBehaviour : HatBehaviour {
        static TempHatBehaviour() => ClassInjector.RegisterTypeInIl2Cpp<TempHatBehaviour>();
        public TempHatBehaviour(IntPtr ptr) : base(ptr) {}
    }
}