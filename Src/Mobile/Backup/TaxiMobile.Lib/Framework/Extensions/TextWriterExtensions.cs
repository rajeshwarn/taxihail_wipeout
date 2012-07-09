using System.IO;

namespace TaxiMobile.Lib.Framework.Extensions
{
    public static class TextWriterExtensions
    {
        public static void Write(this TextWriter writer, string format, params object[] args)
        {
            writer.Write(string.Format(format, args));
        }

        public static void WriteLine(this TextWriter writer, string format, params object[] args)
        {
            writer.Write(string.Format(format, args));
        }
    }
}