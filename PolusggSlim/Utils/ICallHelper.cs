using System;
using Il2CppSystem.Collections.Generic;
using UnhollowerBaseLib;

namespace PolusggSlim.Utils
{
    public static class ICallHelper
    {
        private static readonly Dictionary<string, Delegate> ICallCache = new();

        public static T GetICall<T>(string iCallName) where T : Delegate
        {
            if (ICallCache.ContainsKey(iCallName))
                return (T) ICallCache[iCallName];

            var del = IL2CPP.ResolveICall<T>(iCallName);

            if (del == null)
                throw new MissingMethodException($"Could not resolve internal call by name '{iCallName}'!");

            ICallCache.Add(iCallName, del);

            return del;
        }
    }
}