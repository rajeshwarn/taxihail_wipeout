using System;

namespace TaxiMobile.Lib.Services.Mapper
{
    public static class DateTimeExtension
    {
        public static TWEBTimeStamp ToWSDateTime(this DateTime dateTime)
        {
            return new TWEBTimeStamp
            {
                Fractions = dateTime.Millisecond,
                Second = dateTime.Second,
                Minute = dateTime.Minute,
                Hour = dateTime.Hour,
                Day = dateTime.Day,
                Month = dateTime.Month,
                Year = dateTime.Year
            };
        }

        public static DateTime ToDateTime(this TWEBTimeStamp timeStamp)
        {
            return new DateTime(timeStamp.Year, timeStamp.Month, timeStamp.Day, timeStamp.Hour, timeStamp.Minute, timeStamp.Second, timeStamp.Fractions);
        } 
    }
}