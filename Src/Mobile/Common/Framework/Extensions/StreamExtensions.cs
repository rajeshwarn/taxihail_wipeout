using System.IO;

namespace apcurium.MK.Booking.Mobile.Framework.Extensions
{
    public static class StreamExtensions
    {
        public static byte[] ReadBytes(this Stream stream)
        {
            var bytes = new byte[stream.Length];

            stream.Read(bytes, 0, bytes.Length);

            return bytes;
        }
    }
}