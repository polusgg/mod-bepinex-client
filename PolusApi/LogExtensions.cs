using System.Linq;
using HarmonyLib;
using PolusGGMod;

namespace PolusGGMod {
    public static class LogExtensions {
        public static T Log<T>(this T value, int times = 1, string comment = "") {
            if (times == 1) System.Console.WriteLine($"{value} {comment}");
            else {
                for (int i = 0; i < times; i++) {
                    System.Console.WriteLine($"{value} {comment}");
                }
            }

            return value;
        } 
        public static byte[] Log(this byte[] value, int times = 1, string comment = "") {
            value.Select(x => x.ToString("X2")).Join(delimiter: "").Log(times, comment);

            return value;
        } 
    }
}