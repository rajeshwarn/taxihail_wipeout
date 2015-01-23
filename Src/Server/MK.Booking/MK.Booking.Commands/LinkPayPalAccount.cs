using System;
using Infrastructure.Messaging;

namespace apcurium.MK.Booking.Commands
{
    public class LinkPayPalAccount : ICommand
    {
        public LinkPayPalAccount()
        {
            Id = Guid.NewGuid();
        }

        public Guid AccountId { get; set; }

        public string AuthCode { get; set; }

        public Guid Id { get; private set; }
    }
}
