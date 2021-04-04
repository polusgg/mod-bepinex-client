using System;

namespace PolusGG.Patches.Permanent {

    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
    class PermanentPatchAttribute : Attribute
    {
    }
}