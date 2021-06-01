using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using BepInEx.Logging;
using HarmonyLib;
using UnhollowerBaseLib;

namespace PolusGG.Extensions {
    public static class LogExtensions {
#if RELEASE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T Log<T>(this T value, int times = 69, string comment = "420", LogLevel level = LogLevel.Info) => value;
#else
        // comment defaults to stack trace last method
        public static T Log<T>(this T value, int times = 1, string comment = "", LogLevel level = LogLevel.Info) {
            if (times == 1) LogOnce(value.ToString(), comment, level);
            else
                for (int i = 0; i < times; i++)
                    LogOnce(value.ToString(), comment, level);

            return value;
        }

        private static void LogOnce(string value, string comment, LogLevel level) {
            PogusPlugin.Logger.Log(level, $"{value} {comment}");
        }

        public static byte[] Log(this byte[] value, int times = 1, string comment = "", LogLevel level = LogLevel.Info) {
            value.Select(x => x.ToString("X2")).Join(delimiter: "").Log(times, comment, level);

            return value;
        }

        public static Il2CppStructArray<byte> Log(this Il2CppStructArray<byte> value, int times = 1,
            string comment = "", LogLevel level = LogLevel.Info) {
            value.Select(x => x.ToString("X2")).Join(delimiter: "").Log(times, comment, level);

            return value;
        }
#endif
    }
}