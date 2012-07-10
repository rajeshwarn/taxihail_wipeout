using System.IO;

namespace TaxiMobile.Lib.Framework.Extensions
{
    public static class FileInfoExtensions
    {
        public static byte[] ReadBytes(this FileInfo fileInfo)
        {
            using (var stream = fileInfo.OpenRead())
            {
                return stream.ReadBytes();
            }
        }
    }
}
