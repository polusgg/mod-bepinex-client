using System;

namespace PolusGGMod.Patches {

    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
    class PermanentPatchAttribute : Attribute
    {
    }
}