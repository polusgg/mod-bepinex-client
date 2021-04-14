#nullable enable
using System.Collections.Generic;
using System.Reflection;

namespace PolusGG.Patching {
    public class PatchDetails
    {
        public MethodBase? OriginalMethodBase;
        public List<MethodInfo> PrefixPatches;
        public List<MethodInfo> PostfixPatches;
        public PatchDetails(MethodBase originalMethodBase, List<MethodInfo> prefixPatches, List<MethodInfo> postfixPatches)
        {
            this.OriginalMethodBase = originalMethodBase;
            this.PrefixPatches = prefixPatches;
            this.PostfixPatches = postfixPatches;
        }
    }
}