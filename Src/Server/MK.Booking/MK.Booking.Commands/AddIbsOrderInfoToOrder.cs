using System;
using Infrastructure.Messaging;

namespace apcurium.MK.Booking.Commands
{
    public class AddIbsOrderInfoToOrder : ICommand
    {
        public AddIbsOrderInfoToOrder()
        {
            Id = Guid.NewGuid();
        }

        public Guid OrderId { get; set; }

        public int IBSOrderId { get; set; }

        public string CompanyKey { get; set; }

        public Guid Id { get; private set; }
    }
}