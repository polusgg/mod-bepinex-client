using System;
using System.Linq;

namespace PolusggSlim.Utils.Extensions
{
    public static class StringExtensions
    {
        public static byte[] HexStringToByteArray(this string hex)
        {
            return Enumerable.Range(0, hex.Length / 2)
                .Select(x => Convert.ToByte(hex.Substring(x * 2, 2), 16))
                .ToArray();
        }
        
        public static string ByteArrayToHexString(this byte[] bytes)
        {
            return BitConverter.ToString(bytes).Replace("-", "");
        }
    }
}