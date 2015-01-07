using System;
using Infrastructure.Messaging;

namespace apcurium.MK.Booking.Commands
{
    public class CancelOrderBecauseOfIbsError : ICommand
    {
        public CancelOrderBecauseOfIbsError()
        {
            Id = Guid.NewGuid();
        }

        public Guid OrderId { get; set; }

        public string ErrorCode { get; set; }

        public string ErrorDescription { get; set; }

        public Guid Id { get; private set; }
    }
}