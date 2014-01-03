using System.IO;

namespace apcurium.MK.Booking.Mobile.Framework.Extensions
{
    public static class FileInfoExtensions
    {
        public static byte[] ReadBytes(this FileInfo fileInfo)
        {
            using (FileStream stream = fileInfo.OpenRead())
            {
                return stream.ReadBytes();
            }
        }
    }
}