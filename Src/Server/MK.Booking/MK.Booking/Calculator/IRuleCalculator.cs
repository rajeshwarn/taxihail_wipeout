#region

using System;
using apcurium.MK.Booking.ReadModel;

#endregion

namespace apcurium.MK.Booking.Calculator
{
    public interface IRuleCalculator
    {
        RuleDetail GetActiveWarningFor(bool isFutureBooking, DateTime pickupDate, Func<string> pickupZoneGetterFunc, Func<string> dropOffZoneGetterFunc);
        RuleDetail GetActiveDisableFor(bool isFutureBooking, DateTime pickupDate, Func<string> pickupZoneGetterFunc, Func<string> dropOffZoneGetterFunc);
    }
}