#region

using System;
using apcurium.MK.Booking.ReadModel;
using apcurium.MK.Booking.Maps.Geo;

#endregion

namespace apcurium.MK.Booking.Calculator
{
    public interface IRuleCalculator
    {
		RuleDetail GetActiveWarningFor(bool isFutureBooking, DateTime pickupDate, Func<string> pickupZoneGetterFunc, Func<string> dropOffZoneGetterFunc, string market, Position pickupPoint);

		RuleDetail GetActiveDisableFor(bool isFutureBooking, DateTime pickupDate, Func<string> pickupZoneGetterFunc, Func<string> dropOffZoneGetterFunc, string market, Position pickupPoint);

        RuleDetail GetDisableFutureBookingRule(string market);
    }
}