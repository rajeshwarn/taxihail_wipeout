using System;
using apcurium.MK.Common.Enumeration;
using Infrastructure.Messaging;

namespace apcurium.MK.Booking.Commands
{
    public class CaptureCreditCardPayment : ICommand
    {
        public CaptureCreditCardPayment()
        {
            Id = Guid.NewGuid();
        }

        public Guid Id { get; private set; }

        public Guid PaymentId { get; set; }

        public string AuthorizationCode { get; set; }

        public PaymentProvider Provider { get; set; }
    }
}