using System.Linq;
using BepInEx.Logging;
using HarmonyLib;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using UnhollowerBaseLib;

namespace Polus.Extensions {
    public static class LogExtensions {
// #if RELEASE
//         [MethodImpl(MethodImplOptions.AggressiveInlining)]
//         public static T Log<T>(this T value, int times = 69, string comment = "420", LogLevel level = LogLevel.Info) => value;
// #else
        // comment defaults to stack trace last method
        public static T Log<T>(this T value, int times = 1, string comment = "", LogLevel level = LogLevel.Info) {
            if (times == 1) LogOnce(value.ToString(), comment, level);
            else
                for (int i = 0; i < times; i++)
                    LogOnce(value.ToString(), comment, level);

            return value;
        }

        public static T Trace<T>(this T value, LogLevel level = LogLevel.Info) {
            new Il2CppSystem.Diagnostics.StackTrace().ToString().Log(level: level);
            return value;
        }

        private static void LogOnce(string value, string comment, LogLevel level) {
            PogusPlugin.Logger.Log(level, comment == "" ? value : $"{comment} {value}");
        }

        public static string Hog(this object value) {
            return "Hog";
        }

        public static byte[] Log(this byte[] value, int times = 1, string comment = "", LogLevel level = LogLevel.Info) {
            value.Hex().Log(times, comment, level);

            return value;
        }

        public static Il2CppStructArray<byte> Log(this Il2CppStructArray<byte> value, int times = 1,
            string comment = "", LogLevel level = LogLevel.Info) =>
            value.ToArray().Log(times, comment, level);
// #endif
    }
}