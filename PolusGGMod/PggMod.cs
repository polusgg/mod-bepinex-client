using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using PolusGGMod.Framework;
using PolusGGMod.Framework.Common.Utilities;

namespace PolusGGMod {
    public class PggMod {
        private Harmony _harmony;
        private List<PatchDetails> ToggledPatches = new();
        public static bool IsPatched;

        public virtual void LoadPatches(Assembly assembly) {
            System.Type[] types = AccessTools.GetTypesFromAssembly(assembly);
            Harmony harmony = new Harmony($"gg.polus.temporary.{assembly.GetName()}");
            foreach (var type in types)
            {
                ToggledPatches.AddRange(PatchManagerUtils.ResolvePatchDetails(type));
            }
        }
        public void Patch()
        {
            foreach (PatchDetails patchDetail in ToggledPatches)
            {
                foreach (MethodInfo prefixPatch in patchDetail.PrefixPatches)
                {
                    _harmony.Patch(patchDetail.OriginalMethodBase, prefix: new HarmonyMethod(prefixPatch));
                    PogusPlugin.Logger.LogInfo(
                        $"Prefixed {patchDetail.OriginalMethodBase.Name} with {prefixPatch.DeclaringType.FullName}->{prefixPatch.Name}");
                }
                foreach (MethodInfo postfixPatch in patchDetail.PostfixPatches)
                {
                    _harmony.Patch(patchDetail.OriginalMethodBase, postfix: new HarmonyMethod(postfixPatch));
                    PogusPlugin.Logger.LogInfo(
                        $"Postfixed {patchDetail.OriginalMethodBase.Name} with {postfixPatch.DeclaringType.FullName}->{postfixPatch.Name}");
                }
            }
            IsPatched = true;
        }
        public void Unpatch()
        {
            foreach (PatchDetails patchDetail in ToggledPatches)
            {
                foreach (MethodInfo prefixPatch in patchDetail.PrefixPatches)
                {
                    _harmony.Unpatch(patchDetail.OriginalMethodBase, prefixPatch);
                    PogusPlugin.Logger.LogInfo(
                        $"Unprefixed {patchDetail.OriginalMethodBase.Name} with {prefixPatch.DeclaringType.FullName}->{prefixPatch.Name}");
                }
                foreach (MethodInfo postfixPatch in patchDetail.PostfixPatches)
                {
                    _harmony.Unpatch(patchDetail.OriginalMethodBase, postfixPatch);
                    PogusPlugin.Logger.LogInfo(
                        $"Unpostfixed {patchDetail.OriginalMethodBase.Name} with {postfixPatch.DeclaringType.FullName}->{postfixPatch.Name}");
                }
            }
            
            IsPatched = false;
        }
    }
}