using System;
using Infrastructure.Messaging;

namespace apcurium.MK.Booking.Commands
{
    public class SwitchOrderToNextDispatchCompany : ICommand
    {
        public SwitchOrderToNextDispatchCompany()
        {
            Id = Guid.NewGuid();
        }

        public Guid Id { get; private set; }

        public Guid OrderId { get; set; }

        public int IBSOrderId { get; set; }

        public string CompanyKey { get; set; }

        public string CompanyName { get; set; }
    }
}
