using System.IO;

namespace apcurium.MK.Booking.ConfigTool
{
    public static class PathConverter
    {
		public static string Convert(string path )
        {
			return path.Replace(@"\",Path.DirectorySeparatorChar.ToString());
        }
    }

}

