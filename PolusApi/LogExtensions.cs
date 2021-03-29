using System.Linq;
using HarmonyLib;
using UnhollowerBaseLib;

namespace PolusApi {
    public static class LogExtensions {
        public static T Log<T>(this T value, int times = 1, string comment = "") {
            #if DEBUG
            if (times == 1) System.Console.WriteLine($"{value} {comment}");
            else {
                for (int i = 0; i < times; i++) {
                    System.Console.WriteLine($"{value} {comment}");
                }
            }
            #endif

            return value;
        } 
        public static byte[] Log(this byte[] value, int times = 1, string comment = "") {
            #if DEBUG
            value.Select(x => x.ToString("X2")).Join(delimiter: "").Log(times, comment);
            #endif

            return value;
        } 
        public static Il2CppStructArray<byte> Log(this Il2CppStructArray<byte> value, int times = 1, string comment = "") {
            #if DEBUG
            value.Select(x => x.ToString("X2")).Join(delimiter: "").Log(times, comment);
            #endif
            
            return value;
        } 
    }
}