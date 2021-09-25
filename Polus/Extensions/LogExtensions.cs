using System.Linq;
using BepInEx.Logging;
using HarmonyLib;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Il2CppSystem;
using UnhollowerBaseLib;

namespace Polus.Extensions {
    public static class LogExtensions {
        public static T Log<T>(this T value, int times = 1, string comment = "", LogLevel level = LogLevel.Info, bool trimNewLine = true) {
            return value.Log(PogusPlugin.Logger, times, comment, level, trimNewLine);
        }

        public static T Log<T>(this T value, ManualLogSource logSource, int times = 1, string comment = "", LogLevel level = LogLevel.Info, bool trimNewLine = true) {
            string output = value.ToString();
            if (trimNewLine) output = output.TrimNewLine();
            if (times == 1) LogOnce(logSource, output, comment, level);
            else
                for (int i = 0; i < times; i++)
                    LogOnce(logSource, output, comment, level);

            return value;
        }

        private static void LogOnce(ManualLogSource logSource, string value, string comment, LogLevel level) {
            logSource.Log(level, $"{comment} {value}".TrimStart(' '));
        }

        public static string TrimNewLine(this string value) => value.TrimEnd('\n');
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string Hog(this object _) {
            return "Hog";
        }

        public static byte[] Log(this byte[] value, int times = 1, string comment = "", LogLevel level = LogLevel.Info) {
            value.Hex().Log(times, comment, level);

            return value;
        }

        public static Il2CppStructArray<byte> Log(this Il2CppStructArray<byte> value, int times = 1,
            string comment = "", LogLevel level = LogLevel.Info) =>
            value.ToArray().Log(times, comment, level);
    }
}