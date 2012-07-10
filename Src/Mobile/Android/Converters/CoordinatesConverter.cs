using System;
using System.Collections.Generic;
using System.Text;

namespace apcurium.MK.Booking.Mobile.Client.Converters
{
   public static  class CoordinatesConverter
    {

       public static int ConvertToE6(this double coordinate)
       {
           int result = (int)(coordinate * 1e6);
           return result;
       }

       public static double ConvertFromE6(this int coordinate)
       {
           double result = (double)(coordinate / 1e6);
           return result;
       }
    }
}
