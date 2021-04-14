using System;

namespace PolusGG.Mods.Patching {

    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
    class PermanentPatchAttribute : Attribute
    {
    }
}