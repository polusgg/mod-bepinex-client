using System;
using Il2CppSystem.Collections.Generic;
using UnhollowerBaseLib;

namespace PolusggSlim.Utils
{
    public static class InternalCallHelper
    {
        private static readonly Dictionary<string, Delegate> InternalCallCache = new();

        public static T GetICall<T>(string iCallName) where T : Delegate
        {
            if (InternalCallCache.ContainsKey(iCallName))
                return (T) InternalCallCache[iCallName];

            var del = IL2CPP.ResolveICall<T>(iCallName);

            if (del == null)
                throw new MissingMethodException($"Could not resolve internal call by name '{iCallName}'!");

            InternalCallCache.Add(iCallName, del);

            return del;
        }
    }
}