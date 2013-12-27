using System;

namespace apcurium.MK.Booking.Mobile.Framework.Extensions
{
    public static class DateTimeExtensions
    {
        public static bool IsWeekEnd(this DateTime instance)
        {
            return instance.DayOfWeek == DayOfWeek.Saturday ||
                   instance.DayOfWeek == DayOfWeek.Sunday;
        }

        public static bool IsWeekDay(this DateTime instance)
        {
            return !IsWeekEnd(instance);
        }

        public static DateTime AddWeekDays(this DateTime instance, int days)
        {
            int sign = Math.Sign(days);
            int unsignedDays = Math.Abs(days);
            for (int i = 0; i < unsignedDays; i++)
            {
                do
                {
                    instance = instance.AddDays(sign);
                } while (instance.IsWeekEnd());
            }
            return instance;
        }

        public static DateTime AddWeekDays(this DateTime instance, TimeSpan timeSpan)
        {
            return AddWeekDays(instance, Convert.ToInt32(timeSpan.TotalDays));
        }


        public static DateTime AddWeeks(this DateTime dateTime, int weeks)
        {
            return dateTime.AddDays(weeks*7);
        }
    }
}