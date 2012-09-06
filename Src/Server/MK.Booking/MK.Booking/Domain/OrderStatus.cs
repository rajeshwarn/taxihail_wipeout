using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace apcurium.MK.Booking.Domain
{
    public enum OrderStatus
    {
        Pending = 1,
        Created = 2,
        Canceled = 3,
        Completed = 4,
        Removed = 5 // when user choose to remove it from the history
    }
}
