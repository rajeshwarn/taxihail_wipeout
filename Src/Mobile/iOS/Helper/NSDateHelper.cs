using Foundation;
using System;

namespace apcurium.MK.Booking.Mobile.Client.Helper
{
    public static class NSDateHelper
    {
        public static DateTime NSDateToDateTimeUtc(this NSDate date)
        {
            DateTime reference = new DateTime(2001, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            return DateTime.SpecifyKind(reference.AddSeconds(date.SecondsSinceReferenceDate), DateTimeKind.Utc);
        }

        public static DateTime NSDateToLocalDateTime(this NSDate date)
        {
            return date.NSDateToDateTimeUtc().ToLocalTime();
        }

        public static NSDate LocalDateTimeToNSDate(this DateTime localDate)
        {
            DateTime reference = new DateTime(2001, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            return NSDate.FromTimeIntervalSinceReferenceDate((localDate.ToUniversalTime() - reference).TotalSeconds);
        }
    }
}
