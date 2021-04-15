using System.IO;

namespace PolusggSlim.Utils.Extensions
{
    public static class StreamExtensions
    {
        public static byte[] ReadAll(this Stream stream)
        {
            using var memoryStream = new MemoryStream();
            stream.CopyTo(memoryStream);
            return memoryStream.ToArray();
        }
    }
}