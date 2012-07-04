using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.IO;


namespace apcurium.MK.Common.Extensions
{
    public static class AssemblyExtension
    {

        public static void WriteManifestResource(this Assembly assembly, string fileName, string resourceName)
        {
            using (var stream = assembly.GetManifestResourceStream(resourceName))
            {
				WriteManifestResource( assembly, fileName, stream );
            }
        }
		
		public static void WriteManifestResource(this Assembly assembly, string fileName, Stream stream)
        {
            using ( stream )
            {
                var reader = new BinaryReader(stream);

                var writer = new BinaryWriter(new FileStream(fileName, FileMode.Create));
                long bytesLeft = reader.BaseStream.Length;
                while (bytesLeft > 0)
                {
                    // 65535L is < Int32.MaxValue, so no need to test for overflow            
                    byte[] chunk = reader.ReadBytes((int)Math.Min(bytesLeft, 65536L));
                    writer.Write(chunk);
                    bytesLeft -= chunk.Length;
                }
                writer.Close();
                reader.Close();
            }
        }
		
        public static byte[] GetManifestResourceBytes(this Assembly assembly, string resourceName)
        {
            byte[] result = null;
            using (var stream = assembly.GetManifestResourceStream(resourceName))
            {
                var reader = new BinaryReader(stream);
                var buffer = reader.ReadBytes((int)stream.Length);
                reader.Close();
                result = buffer;
            }
            return result;
        }

    }
}

