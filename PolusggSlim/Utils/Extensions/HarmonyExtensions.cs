using System;
using System.Linq;
using System.Reflection;
using HarmonyLib;

namespace PolusggSlim.Utils.Extensions
{
    public static class HarmonyExtensions
    {
        public static void PatchAllExcept(this Harmony harmony, Type ignoredType)
        {
            harmony.PatchAllExcept(new []{ ignoredType });
        }
        
        public static void PatchAllExcept(this Harmony harmony, Type[] ignoredTypes)
        {
            foreach (var typeToPatch in AccessTools.GetTypesFromAssembly(Assembly.GetExecutingAssembly()))
            {
                if (!ignoredTypes.Contains(typeToPatch))
                    harmony.CreateClassProcessor(typeToPatch).Patch();
            }
        }
    }
}