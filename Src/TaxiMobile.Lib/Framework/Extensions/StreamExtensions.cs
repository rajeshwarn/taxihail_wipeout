using System.IO;

namespace apcurium.Framework.Extensions
{
    public static class StreamExtensions
    {
        public static byte[] ReadBytes(this Stream stream)
        {
            byte[] bytes = new byte[stream.Length];

            stream.Read(bytes, 0, bytes.Length);

            return bytes;
        }
    }
}
