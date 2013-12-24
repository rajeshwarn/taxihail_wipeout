#region

using System;

#endregion

namespace apcurium.MK.Booking.IBS
{
    public static class Extensions
    {
        public static DateTime? ToDateTime(this TWEBTimeStamp etaTime)
        {
            return etaTime == null || etaTime.Year < DateTime.Now.Year
                ? (DateTime?) null
                : new DateTime(
                    etaTime.Year,
                    etaTime.Month,
                    etaTime.Day,
                    etaTime.Hour,
                    etaTime.Minute,
                    etaTime.Second);
        }
    }
}