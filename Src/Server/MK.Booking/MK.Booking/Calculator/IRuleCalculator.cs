using System;
using System.Collections.Generic;
using apcurium.MK.Booking.ReadModel;

namespace apcurium.MK.Booking.Calculator
{
    public interface IRuleCalculator
    {
        RuleDetail GetActiveWarningFor(bool isFutureBooking, DateTime pickupDate, string zone);
        RuleDetail GetActiveDisableFor(bool isFutureBooking, DateTime pickupDate, string zone);
    }
}