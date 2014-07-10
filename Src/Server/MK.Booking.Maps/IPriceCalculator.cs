#region

using System;

#endregion

namespace apcurium.MK.Booking.Maps
{
    public interface IPriceCalculator
    {
        double? GetPrice(int? distance, DateTime pickupDate, int? durationWithoutAnyDelayInSeconds, int? trafficDelayInSeconds);
    }
}