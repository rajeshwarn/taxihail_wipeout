#region

using System;
using Infrastructure.Messaging;

#endregion

namespace apcurium.MK.Booking.Commands
{
    public class CancelPayPalExpressCheckoutPayment : ICommand
    {
        public CancelPayPalExpressCheckoutPayment()
        {
            Id = Guid.NewGuid();
        }

        public Guid PaymentId { get; set; }
        public Guid Id { get; private set; }
    }
}