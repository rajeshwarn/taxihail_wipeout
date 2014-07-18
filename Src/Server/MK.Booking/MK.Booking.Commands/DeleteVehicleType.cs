using System;
using Infrastructure.Messaging;

namespace apcurium.MK.Booking.Commands
{
    public class DeleteVehicleType : ICommand
    {
        public DeleteVehicleType()
        {
            Id = Guid.NewGuid();
        }

        public Guid Id { get; set; }
        public Guid VehicleTypeId { get; set; }
        public Guid CompanyId { get; set; }
    }
}