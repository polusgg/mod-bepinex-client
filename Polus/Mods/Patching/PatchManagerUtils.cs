using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HarmonyLib;

namespace Polus.Mods.Patching {
    namespace Common.Utilities {
        public static class PatchManagerUtils {
            private static readonly List<Type> _harmonyBulkPatchTypes = new() {
                typeof(HarmonyTargetMethod),
                typeof(HarmonyTargetMethods)
            };

            private static readonly HarmonyPatchType[] _harmonyPatchTypes = {
                HarmonyPatchType.Prefix,
                HarmonyPatchType.Postfix
            };

            public static List<PatchDetails>
                ResolvePatchDetails(Type assemblyType, bool patchPermanently) //TODO: change return type to PatchDetails
            {
                List<PatchDetails> patchDetails = new();

                MethodBase originalMethod = null;
                List<MethodBase> bulkOriginalMethods = new();
                List<MethodInfo> prefixPatches = new();
                List<MethodInfo> postfixPatches = new();

                HarmonyMethod assemblyContainerAttrs = HarmonyMethodExtensions.GetMergedFromType(assemblyType);
                if (assemblyContainerAttrs.methodType is null) assemblyContainerAttrs.methodType = MethodType.Normal;

                if (assemblyContainerAttrs.methodName is not null)
                    originalMethod = GetOriginalMethodBase(assemblyContainerAttrs);

                List<MethodInfo> allAssemblyTypeMethods = AccessTools.GetDeclaredMethods(assemblyType);

                //Get patch methods for all non-permanent patches
                foreach (MethodInfo method in allAssemblyTypeMethods) {
                    PermanentPatchAttribute permanentPatch = method.GetCustomAttribute<PermanentPatchAttribute>(true);

                    object[] allCustomAttributes = method.GetCustomAttributes(true);

                    HashSet<string> allHarmonyAttributes = new(method.GetCustomAttributes(true)
                        .Select(attr => attr.GetType().FullName)
                        .Where(name => name.StartsWith("Harmony"))
                    );

                    foreach (HarmonyPatchType patchType in _harmonyPatchTypes) {
                        string name = patchType.ToString();

                        if (name == method.Name ||
                            allHarmonyAttributes.Contains(
                                $"HarmonyLib.Harmony{name}")) // Debug.Log($"Harmony lmoa {permanentPatch is null} {patchPermanently}");
                            switch (patchType) {
                                case HarmonyPatchType.Prefix: {
                                    if ((permanentPatch is not null || !patchPermanently) &&
                                        (permanentPatch is null || patchPermanently))
                                        prefixPatches.Add(method);
                                    break;
                                }
                                case HarmonyPatchType.Postfix: {
                                    if ((permanentPatch is not null || !patchPermanently) &&
                                        (permanentPatch is null || patchPermanently))
                                        postfixPatches.Add(method);
                                    break;
                                }
                            }
                    }
                }

                //Test for bulk/generic patching methods like HarmonyTargetMethods
                foreach (Type bulkPatchType in _harmonyBulkPatchTypes) {
                    MethodInfo method = assemblyType.GetMethods(AccessTools.all).FirstOrDefault(method =>
                        method.GetCustomAttributes(true)
                            .Any(attr => attr.GetType().FullName == bulkPatchType.FullName)) ?? assemblyType.GetMethod(bulkPatchType.FullName.Replace("HarmonyLib.Harmony", ""),
                        AccessTools.all);

                    if (method is not null) {
                        IEnumerable<MethodBase> methodBases = (IEnumerable<MethodBase>) method.Invoke(null, null);
                        bulkOriginalMethods.AddRange(methodBases);
                    }
                }

                if (originalMethod is not null)
                    patchDetails.Add(new PatchDetails(originalMethod, prefixPatches, postfixPatches));

                patchDetails.AddRange(bulkOriginalMethods.Select(targetMethod => new PatchDetails(targetMethod, prefixPatches, postfixPatches)));

                return patchDetails;
            }

            private static MethodBase GetOriginalMethodBase(HarmonyMethod attr) {
                switch (attr.methodType) {
                    case MethodType.Normal: {
                        return AccessTools.DeclaredMethod(attr.declaringType, attr.methodName, attr.argumentTypes);
                    }
                    case MethodType.Getter: {
                        return AccessTools.DeclaredProperty(attr.declaringType, attr.methodName).GetGetMethod(true);
                    }
                    case MethodType.Setter: {
                        return AccessTools.DeclaredProperty(attr.declaringType, attr.methodName).GetSetMethod(true);
                    }
                    //case MethodType.Constructor:
                    //    return AccessTools.DeclaredConstructor(attr.declaringType, attr.argumentTypes);
                    //case MethodType.StaticConstructor:
                    //    return AccessTools.GetDeclaredConstructors(attr.declaringType)
                    //            .Where(c => c.IsStatic)
                    //            .FirstOrDefault();
                    default: {
                        return null;
                    }
                }
            }
        }
    }
}