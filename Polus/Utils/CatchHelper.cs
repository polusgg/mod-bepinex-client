using System;
using BepInEx.Logging;
using Polus.Extensions;

namespace Polus.Utils {
    public static class CatchHelper {
        // trollface emote goes here

        public static void TryCatch(Action action, bool logs = true) {
            try {
                action();
            } catch (Exception e) {
                if (logs) e.Log(level: LogLevel.Error);
            }
        }

        public static T TryCatch<T>(Func<T> action, bool logs = true) {
            try {
                return action();
            } catch (Exception e) {
                if (logs) e.Log(level: LogLevel.Error);
            }

            return default;
        }
    }
}