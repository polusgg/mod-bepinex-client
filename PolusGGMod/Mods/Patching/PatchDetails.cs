#nullable enable
using System.Collections.Generic;
using System.Reflection;

namespace PolusGG.Mods.Patching {
    public class PatchDetails
    {
        public MethodBase? OriginalMethodBase;
        public List<MethodInfo> PrefixPatches;
        public List<MethodInfo> PostfixPatches;
        public PatchDetails(MethodBase originalMethodBase, List<MethodInfo> prefixPatches, List<MethodInfo> postfixPatches)
        {
            OriginalMethodBase = originalMethodBase;
            PrefixPatches = prefixPatches;
            PostfixPatches = postfixPatches;
        }
    }
}