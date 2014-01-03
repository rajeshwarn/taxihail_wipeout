using System.IO;

namespace apcurium.MK.Booking.Mobile.Framework.Extensions
{
    public static class TextWriterExtensions
    {
        public static void Write(this TextWriter writer, string format, params object[] args)
        {
            writer.Write(format, args);
        }

        public static void WriteLine(this TextWriter writer, string format, params object[] args)
        {
            writer.Write(format, args);
        }
    }
}