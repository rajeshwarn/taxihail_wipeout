using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace apcurium.MK.Booking.Api.Contract.Resources
{
    public enum OrderStatus
    {
        Unknown = 0,
        Pending = 1,
        Created = 2,
        Canceled = 3,
        Completed = 4
    }
}
