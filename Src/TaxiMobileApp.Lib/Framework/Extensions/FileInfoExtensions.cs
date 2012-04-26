using System.IO;

namespace apcurium.Framework.Extensions
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
