using System.Collections.Generic;

namespace apcurium.MK.Booking.Mobile.Framework
{
    public static class Params
    {
        public static IEnumerable<T> Get<T>(params T[] values)
        {
            return values;
        }
    }
}