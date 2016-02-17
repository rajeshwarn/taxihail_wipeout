using System;
using apcurium.MK.Common.Entity;
using Infrastructure.Messaging;

namespace apcurium.MK.Booking.Commands
{
    public class ChangeOrderStatusForManualRideLinq : ICommand
    {
        public ChangeOrderStatusForManualRideLinq()
        {
            Id = Guid.NewGuid();
        }

        public Guid Id { get; set; }

        public Guid OrderId { get; set; }

        public OrderStatus Status { get; set; }

        public DateTime LastTripPollingDateInUtc { get; set; }
    }
}