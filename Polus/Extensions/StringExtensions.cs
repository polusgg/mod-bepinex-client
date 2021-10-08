using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using BepInEx.Logging;
using HarmonyLib;

namespace Polus.Extensions {
    public static class StringExtensions {
        public static string Hex(this IEnumerable<byte> value) {
            return value.Select(x => x.ToString("X2")).Join(delimiter: "");
        }
        
        public static byte[] FromHex(this string text) {
            return Enumerable.Range(0, text.Length)
                .Where(x => x % 2 == 0)
                .Select(x => Convert.ToByte(text.Substring(x, 2), 16))
                .ToArray();
        }

        public static string SanitizationRegex = "(:?<size.+?(=?</size>))|(:?<voffset.+?(=?</voffset>))|(:?<sprite[^>]*>)";
        public static string SanitizeName(this string name) {
            return Regex.Replace(name, SanitizationRegex, "");
        }
    }
}