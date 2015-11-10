using System;
using Infrastructure.Messaging;

namespace apcurium.MK.Booking.Commands
{
    public class AddVehicleIdMapping : ICommand
    {
        public AddVehicleIdMapping()
        {
            Id = Guid.NewGuid();
        }

        public Guid OrderId { get; set; }

        public string LegacyDispatchId { get; set; }

        public string DeviceName { get; set; }

        public Guid Id { get; private set; }
    }
}
