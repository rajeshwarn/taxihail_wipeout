using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Infrastructure.EventSourcing;

namespace apcurium.MK.Booking.Domain
{
    public class Order : EventSourced
    {
        protected Order(Guid id)
            : base(id)
        {
        
    }
    }
}
