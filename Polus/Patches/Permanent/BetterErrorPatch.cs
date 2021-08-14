using BepInEx.Logging;
using HarmonyLib;
using Il2CppSystem;
using Il2CppSystem.Diagnostics;
using Polus.Extensions;
using UnhollowerBaseLib;
using IntPtr = System.IntPtr;

namespace Polus.Patches.Permanent {
    [HarmonyPatch(typeof(Il2CppException), nameof(Il2CppException.RaiseExceptionIfNecessary))]
    public class BetterErrorPatch {
        [HarmonyPrefix]
        public static bool Raise([HarmonyArgument(0)] IntPtr returnedException) {
            if (!(returnedException == IntPtr.Zero)) {
                new StackTrace(new Exception(returnedException), false).ToString().Log(1, "stack trace", LogLevel.Error);
                throw new Il2CppException(returnedException);
            }
            return false;
        }
    }
}