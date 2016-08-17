using System;
using System.IO;
using System.Reflection;
using PCLStorage;

namespace apcurium.MK.Common.Extensions
{
    public static class AssemblyExtension
    {
        public static void WriteManifestResource(this Assembly assembly, string fileName, string resourceName)
        {
            using (var stream = assembly.GetManifestResourceStream(resourceName))
            {
                WriteManifestResource(assembly, fileName, stream);
            }
        }

        public static async void WriteManifestResource(this Assembly assembly, string fileName, Stream stream)
        {
            using (stream)
            {
                var reader = new BinaryReader(stream);

                IFolder localStorage = FileSystem.Current.LocalStorage;
                IFile file = await localStorage.CreateFileAsync("answer.txt", CreationCollisionOption.ReplaceExisting);
                var writer = new BinaryWriter(await file.OpenAsync(FileAccess.ReadAndWrite));
                var bytesLeft = reader.BaseStream.Length;
                while (bytesLeft > 0)
                {
                    // 65535L is < Int32.MaxValue, so no need to test for overflow            
                    var chunk = reader.ReadBytes((int) Math.Min(bytesLeft, 65536L));
                    writer.Write(chunk);
                    bytesLeft -= chunk.Length;
                }
                writer.Dispose();
                reader.Dispose();
            }
        }

        public static byte[] GetManifestResourceBytes(this Assembly assembly, string resourceName)
        {
            byte[] result = null;
            using (var stream = assembly.GetManifestResourceStream(resourceName))
            {
                if (stream != null)
                {
                    var reader = new BinaryReader(stream);
                    var buffer = reader.ReadBytes((int) stream.Length);
                    reader.Dispose();
                    result = buffer;
                }
            }
            return result;
        }
    }
}