using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using PolusGGMod.Framework;
using PolusGGMod.Framework.Common.Utilities;
using PolusGGMod.Patching;

namespace PolusGGMod {
    public class PggMod {
        protected Harmony _harmony;
        protected List<PatchDetails> ToggledPatches = new();
        public bool IsPatched;
        public Harmony Harmony => _harmony;

        public void LoadPatches(Assembly assembly) {
            System.Type[] types = AccessTools.GetTypesFromAssembly(assembly);
            _harmony = new Harmony($"gg.polus.temporary.{assembly.FullName}");
            LoadPatches(null, types);
        }

        public virtual void LoadPatches(string harmonyName, System.Type[] types) {
            if (_harmony == null) _harmony = new Harmony($"gg.polus.temporary.{harmonyName}");
            foreach (var type in types)
            {
                ToggledPatches.AddRange(PatchManagerUtils.ResolvePatchDetails(type, false));
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