using System.Collections.Generic;
using System.Linq;
using BepInEx.Logging;
using HarmonyLib;

namespace Polus.Extensions {
    public static class StringExtensions {
        public static string Hex(this IEnumerable<byte> value) {
            return value.Select(x => x.ToString("X2")).Join(delimiter: "");
        }
    }
}