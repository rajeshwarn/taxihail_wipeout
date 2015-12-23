using System;
using Infrastructure.Messaging;

namespace apcurium.MK.Booking.Commands
{
    public class CancelOrderBecauseOfError : ICommand
    {
        public CancelOrderBecauseOfError()
        {
            Id = Guid.NewGuid();
        }

        public Guid OrderId { get; set; }

        public string ErrorCode { get; set; }

        public string ErrorDescription { get; set; }

        public Guid Id { get; private set; }
    }
}