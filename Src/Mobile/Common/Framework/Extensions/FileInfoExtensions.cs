using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
