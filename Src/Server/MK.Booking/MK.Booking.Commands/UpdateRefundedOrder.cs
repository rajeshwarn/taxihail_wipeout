using System;
using Infrastructure.Messaging;

namespace apcurium.MK.Booking.Commands
{
    public class UpdateRefundedOrder : ICommand
    {
        public UpdateRefundedOrder()
        {
            Id = Guid.NewGuid();
        }

        public Guid Id { get; set; }

        public Guid OrderId { get; set; }

        public bool IsSuccessful { get; set; }

        public string Message { get; set; }
    }
}
