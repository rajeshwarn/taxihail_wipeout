using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;



namespace apcurium.MK.Booking.Mobile.Client.Lib.MD.Framework.Extensions
{
    public static class DoubleExtension
    {
        public static bool AlmostEquals(this double double1, double double2, double precision)
        {
            return (Math.Abs(double1 - double2) <= precision);
        }
    }
}