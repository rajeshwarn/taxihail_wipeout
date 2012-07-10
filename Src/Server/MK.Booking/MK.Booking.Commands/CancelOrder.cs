using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Infrastructure.Messaging;
using apcurium.MK.Common.Enumeration;

namespace apcurium.MK.Booking.Commands
{
    public class CancelOrder : ICommand
    {
        public CancelOrder()
        {
            Id = Guid.NewGuid();
            Status = OrderStatus.Cancelled.ToString();
        }

        public Guid Id { get; set; }
        public Guid OrderId { get; set; }
        public string Status { get; set; }
    }
}
