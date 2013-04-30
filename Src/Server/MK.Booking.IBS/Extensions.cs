using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace apcurium.MK.Booking.IBS
{
    public static class Extensions
    {
        public static DateTime? ToDateTime(this TWEBTimeStamp ETATime)
        {
            return  ETATime == null || ETATime.Year < DateTime.Now.Year
               ? (DateTime?)null
               : new DateTime(
                   ETATime.Year,
                   ETATime.Month,
                   ETATime.Day,
                   ETATime.Hour,
                   ETATime.Minute,
                   ETATime.Second
                   );
        }

    }
}
