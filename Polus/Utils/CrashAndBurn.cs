using System;
using System.Runtime.InteropServices;
using BepInEx.Logging;
using Polus.Extensions;
using UnityEngine;

namespace Polus.Utils {
    public static class CrashAndBurn {
        [DllImport("kernel32.dll")]
        private static extern void MessageBoxA(IntPtr windowHandle, string text, string title, uint type = 0x10);

        public static void Die(Exception ex, bool actuallyClose = true) => Die(ex.ToString(), actuallyClose);

        public static void Die(string text, bool actuallyClose = true) {
            "Your save has been erased and your computer has been wiped. just kidding haha you actually fell for it LOL".Hog();
            text.Log(comment: "A very bad serious exception occurred:", level: LogLevel.Fatal);
            MessageBoxA(IntPtr.Zero, $"A fatal exception has occured the Polus.gg client!\nException info: {text}", "Fatal exception!");
            if (actuallyClose) Application.Quit(1);
        }
    }
}