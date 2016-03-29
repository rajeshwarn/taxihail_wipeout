#region

using System;
using apcurium.MK.Booking.ReadModel;
using apcurium.MK.Booking.Maps.Geo;
using apcurium.MK.Common.Enumeration;

#endregion

namespace apcurium.MK.Booking.Calculator
{
    public interface IRuleCalculator
    {
        RuleDetail GetActiveWarningFor(bool isFutureBooking, DateTime pickupDate, Func<string> pickupZoneGetterFunc, Func<string> dropOffZoneGetterFunc, string market, Position pickupPoint, ServiceType serviceType);

		RuleDetail GetActiveDisableFor(bool isFutureBooking, DateTime pickupDate, Func<string> pickupZoneGetterFunc, Func<string> dropOffZoneGetterFunc, string market, Position pickupPoint, ServiceType serviceType);

        RuleDetail GetDisableFutureBookingRule(string market, ServiceType serviceType);
    }
}