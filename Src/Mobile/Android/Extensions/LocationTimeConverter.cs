using System;

namespace apcurium.MK.Booking.Mobile.Client.Extensions
{
    public static class LocationTimeConverter
    {
        public static DateTime ToDateTime(this long time)
        {
            //UTC time of this fix, in milliseconds since January 1, 1970.
            return new DateTime(1970, 1, 1).AddMilliseconds(Convert.ToDouble(time));
        }
    }
}