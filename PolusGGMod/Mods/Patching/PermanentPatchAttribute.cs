using System;

namespace PolusGG.Patching {

    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
    class PermanentPatchAttribute : Attribute
    {
    }
}