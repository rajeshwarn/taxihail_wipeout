using System;
using Infrastructure.Messaging;

namespace apcurium.MK.Booking.Commands
{
    public class SaveTemporaryOrderPaymentInfo : ICommand
    {
        public SaveTemporaryOrderPaymentInfo()
        {
            Id = Guid.NewGuid();
        }

        public Guid OrderId { get; set; }
        public string Cvv { get; set; }

        public Guid Id { get; set; }
    }
}