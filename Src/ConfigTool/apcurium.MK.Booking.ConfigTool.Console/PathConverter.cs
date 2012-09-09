using System;

namespace apcurium.MK.Booking.ConfigTool
{
    public static class PathConverter
    {



        public static string Convert(string path )
        {
      
#if OSX
            return path.Replace(@"\",@"/");
#else
            return path.Replace(@"/",@"\");
#endif
        }

    }

}

