using System;
using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using Polus.Mods.Patching;
using Polus.Mods.Patching.Common.Utilities;

namespace Polus {
    public class PggMod {
        protected Harmony _harmony;
        public bool IsPatched;
        protected List<PatchDetails> ToggledPatches = new();
        public Harmony Harmony => _harmony;

        public void LoadPatches(Assembly assembly) {
            Type[] types = AccessTools.GetTypesFromAssembly(assembly);
            _harmony = new Harmony($"gg.polus.temporary.{assembly.FullName}");
            LoadPatches(null, types);
        }

        public virtual void LoadPatches(string harmonyName, Type[] types) {
            if (_harmony == null) _harmony = new Harmony($"gg.polus.temporary.{harmonyName}");
            foreach (Type type in types) ToggledPatches.AddRange(PatchManagerUtils.ResolvePatchDetails(type, false));
        }

        public void Patch() {
            foreach (PatchDetails patchDetail in ToggledPatches) {
                foreach (MethodInfo prefixPatch in patchDetail.PrefixPatches) {
                    _harmony.Patch(patchDetail.OriginalMethodBase, new HarmonyMethod(prefixPatch));
                    PogusPlugin.Logger.LogInfo(
                        $"Prefixed {patchDetail.OriginalMethodBase.Name} with {prefixPatch.DeclaringType.FullName}->{prefixPatch.Name}");
                }

                foreach (MethodInfo postfixPatch in patchDetail.PostfixPatches) {
                    _harmony.Patch(patchDetail.OriginalMethodBase, postfix: new HarmonyMethod(postfixPatch));
                    PogusPlugin.Logger.LogInfo(
                        $"Postfixed {patchDetail.OriginalMethodBase.Name} with {postfixPatch.DeclaringType.FullName}->{postfixPatch.Name}");
                }
            }

            IsPatched = true;
        }

        public void Unpatch() {
            foreach (PatchDetails patchDetail in ToggledPatches) {
                foreach (MethodInfo prefixPatch in patchDetail.PrefixPatches) {
                    _harmony.Unpatch(patchDetail.OriginalMethodBase, prefixPatch);
                    PogusPlugin.Logger.LogInfo(
                        $"Unprefixed {patchDetail.OriginalMethodBase.Name} with {prefixPatch.DeclaringType.FullName}->{prefixPatch.Name}");
                }

                foreach (MethodInfo postfixPatch in patchDetail.PostfixPatches) {
                    _harmony.Unpatch(patchDetail.OriginalMethodBase, postfixPatch);
                    PogusPlugin.Logger.LogInfo(
                        $"Unpostfixed {patchDetail.OriginalMethodBase.Name} with {postfixPatch.DeclaringType.FullName}->{postfixPatch.Name}");
                }
            }

            IsPatched = false;
        }
    }
}