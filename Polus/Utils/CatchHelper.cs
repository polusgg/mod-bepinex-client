using System;
using System.Collections.Generic;
using System.Linq;
using BepInEx.Logging;
using Polus.Extensions;
using Polus.Patches.Permanent;
using Sentry;
using Sentry.Extensibility;
using Sentry.Infrastructure;

namespace Polus.Utils {
    public static class CatchHelper {
        // trollface emote goes here

        public static void TryCatch(Action action, bool logs = true, Action catchAction = null) {
            try {
                action();
            } catch (Exception e) {
                if (logs) {
                    e.Log(level: LogLevel.Error);
                    e.ReportException();
                }
                catchAction?.Invoke();
            }
        }

        public static T TryCatch<T>(Func<T> action, bool logs = true, Action catchAction = null) {
            try {
                return action();
            } catch (Exception e) {
                if (logs) {
                    e.Log(level: LogLevel.Error);
                    e.ReportException();
                }
                catchAction?.Invoke();
            }

            return default;
        }

        public static void Init() {
            SentrySdk.Init(o => {
                o.Dsn = "https://1d7e56371934416e84bd9ddc1596b1ba@o1016669.ingest.sentry.io/5982062";
                // When configuring for the first time, to see what the SDK is doing:
                o.Debug = false;
                o.DiagnosticLogger = new PolusDiagnosticLogger();
                (DateTime date, int? packageVersion) = StereotypicalClientModderVersionShowerPatch.Ver;
                o.Release = $"v{date.Year}.{date.Month}.{date.Day}:{(packageVersion.HasValue ? packageVersion.Value : "?")}";
                // Set traces_sample_rate to 1.0 to capture 100% of transactions for performance monitoring.
                // We recommend adjusting this value in production.
                o.TracesSampleRate = 1.0;
            });
            SentrySdk.StartSession();
            // SentrySdk.AddBreadcrumb("Lmaoooo");
            // SentrySdk.CaptureMessage("so true!1");
        }

        public static void AddBreadcrumb(this string value, Dictionary<string, object> data) => SentrySdk.AddBreadcrumb(value, data: data.ToDictionary(kvp => kvp.Key, kvp => kvp.Value.ToString()));

        public static SentryId ReportException(this Exception ex) => SentrySdk.CaptureException(ex);

        public static SentryId ReportMessage(this string message, SentryLevel level = SentryLevel.Error) {
            return SentrySdk.CaptureEvent(new SentryEvent {
                Message = message,
                
                Level = level
            });
        }

        private class PolusDiagnosticLogger : IDiagnosticLogger {
            private readonly ManualLogSource logger = Logger.CreateLogSource("SentryDebug");
            public bool IsEnabled(SentryLevel level) {
                return true;
            }

            public void Log(SentryLevel logLevel, string message, Exception exception = null, params object[] args) {
                $"{message} {exception}".Log(logger, level: logLevel switch {
                    SentryLevel.Debug => LogLevel.Debug,
                    SentryLevel.Info => LogLevel.Info,
                    SentryLevel.Warning => LogLevel.Warning,
                    SentryLevel.Error => LogLevel.Error,
                    SentryLevel.Fatal => LogLevel.Fatal,
                    _ => throw new ArgumentOutOfRangeException(nameof(logLevel), logLevel, null)
                }, comment: string.Join(' ', args));
            }
        }
    }
}