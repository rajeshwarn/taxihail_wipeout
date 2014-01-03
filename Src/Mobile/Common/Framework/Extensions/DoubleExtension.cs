using System;

namespace apcurium.MK.Booking.Mobile.Framework.Extensions
{
    public static class DoubleExtension
    {
        public static bool AlmostEquals(this double double1, double double2, double precision)
        {
            return (Math.Abs(double1 - double2) <= precision);
        }
    }
}