﻿using System.Diagnostics;
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
            if (times == 1) LogOnce(value.ToString(), comment);
            else
                for (int i = 0; i < times; i++)
                    LogOnce(value.ToString(), comment);

            return value;
        }

        private static void LogOnce(string value, string comment, LogLevel level = LogLevel.Info) {
            if (value.Equals("0")) LogOnce(new StackTrace().ToString(), "");
            PogusPlugin.Logger.LogInfo($"{value} {comment}");
        }

        public static byte[] Log(this byte[] value, int times = 1, string comment = "", LogLevel level = LogLevel.Info) {
            value.Select(x => x.ToString("X2")).Join(delimiter: "").Log(times, comment);

            return value;
        }

        public static Il2CppStructArray<byte> Log(this Il2CppStructArray<byte> value, int times = 1,
            string comment = "", LogLevel level = LogLevel.Info) {
            value.Select(x => x.ToString("X2")).Join(delimiter: "").Log(times, comment);

            return value;
        }
#endif
    }
}