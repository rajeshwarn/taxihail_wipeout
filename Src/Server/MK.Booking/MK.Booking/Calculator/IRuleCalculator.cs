﻿using System;
using apcurium.MK.Booking.ReadModel;

namespace apcurium.MK.Booking.Calculator
{
    public interface IRuleCalculator
    {
        RuleDetail GetActiveWarningFor(bool isFutureBooking, DateTime pickupDate, Func<string> zoneGetterFunc);
        RuleDetail GetActiveDisableFor(bool isFutureBooking, DateTime pickupDate, Func<string> zoneGetterFunc);
    }
}