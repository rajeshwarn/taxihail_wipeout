

using System;
using Infrastructure.Messaging;

namespace apcurium.MK.Booking.Commands
{
    public class AddBraintreeAccountId : ICommand
    {
        public AddBraintreeAccountId()
        {
            Id = new Guid();
        }

        public Guid Id { get; set; }
        public Guid AccountId { get; set; }
        public string BraintreeAccountId { get; set; }
    }
}
