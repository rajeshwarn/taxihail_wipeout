using System;
using Infrastructure.Messaging;

namespace apcurium.MK.Booking.Commands
{

    public class IgnoreDispatchCompanySwitch : ICommand
    {
        public IgnoreDispatchCompanySwitch()
        {
            Id = Guid.NewGuid();
        }

        public Guid Id { get; private set; }

        public Guid OrderId { get; set; }
    }
}