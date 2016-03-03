using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using apcurium.MK.Common.Entity;

namespace apcurium.MK.Booking.Api.Contract.Resources
{
    public class ActivateOrderResponse
    {
        public Order Order { get; set; }
        public OrderStatusDetail OrderStatusDetail { get; set; }
    }
}
