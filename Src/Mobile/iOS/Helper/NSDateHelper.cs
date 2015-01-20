using apcurium.MK.Booking.Mobile.Framework.Extensions;
using Foundation;
using System;

namespace apcurium.MK.Booking.Mobile.Client.Helper
{
    public static class NSDateHelper
    {
        public static DateTime NSDateToDateTime(this NSDate date)
        {
            DateTime reference = TimeZone.CurrentTimeZone.ToLocalTime(new DateTime(2001, 1, 1, 0, 0, 0));
            return reference.AddSeconds(date.SecondsSinceReferenceDate);
        }

        public static NSDate DateTimeToNSDate(this DateTime date)
        {
            DateTime reference = TimeZone.CurrentTimeZone.ToLocalTime(new DateTime(2001, 1, 1, 0, 0, 0) );
            return NSDate.FromTimeIntervalSinceReferenceDate((date - reference).TotalSeconds);
        }
    }
}
