using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using apcurium.MK.Booking.Api.Contract.Resources;

namespace apcurium.MK.Booking.Mobile.Models
{
    public class OrderWithStatusModel
    {
        public Order Order { get; set; }
        public OrderStatusDetail OrderStatusDetail { get; set; }
    }
}