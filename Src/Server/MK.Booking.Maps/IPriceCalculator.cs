#region

using System;
using apcurium.MK.Common.Enumeration;

#endregion

namespace apcurium.MK.Booking.Maps
{
    public interface IPriceCalculator
    {
        double? GetPrice(int? distance, DateTime pickupDate, int? durationInSeconds, ServiceType serviceType, int? vehicleTypeId = null);
    }
}