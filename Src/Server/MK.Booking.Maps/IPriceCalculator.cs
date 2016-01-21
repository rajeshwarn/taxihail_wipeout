#region

using System;
using apcurium.MK.Common.Entity;

#endregion

namespace apcurium.MK.Booking.Maps
{
    public interface IPriceCalculator
    {
        double? GetPrice(int? distance, DateTime pickupDate, int? durationInSeconds, int? vehicleTypeId = null, Tariff overriddenTariff = null);
    }
}