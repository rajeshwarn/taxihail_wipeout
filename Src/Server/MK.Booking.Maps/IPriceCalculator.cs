using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace apcurium.MK.Booking.Maps
{
    public interface IPriceCalculator
    {
        double? GetPrice(int? distance, DateTime pickupDate);
    }
}
