using System;

namespace Polus.Mods.Patching {
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
    internal class PermanentPatchAttribute : Attribute { }
}