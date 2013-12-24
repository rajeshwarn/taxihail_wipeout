#region

using System;
using apcurium.MK.Common.Enumeration;
using Infrastructure.Messaging;

#endregion

namespace apcurium.MK.Booking.Commands
{
    public class CaptureCreditCardPayment : ICommand
    {
        public CaptureCreditCardPayment()
        {
            Id = Guid.NewGuid();
        }

        public Guid PaymentId { get; set; }

        public string AuthorizationCode { get; set; }

        public PaymentProvider Provider { get; set; }
        public Guid Id { get; private set; }
    }
}