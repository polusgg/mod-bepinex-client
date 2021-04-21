using System;

namespace PolusGG.Mods.Patching {
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
    internal class PermanentPatchAttribute : Attribute { }
}