using System;
using Infrastructure.Messaging;

namespace apcurium.MK.Booking.Commands
{
    public class AddUpdateVehicleType : ICommand
    {
        public AddUpdateVehicleType()
        {
            Id = Guid.NewGuid();
        }

        public Guid Id { get; set; }
        public Guid VehicleTypeId { get; set; }
        public string Name { get; set; }
        public string LogoName { get; set; }
        public int ReferenceDataVehicleId { get; set; }
        public Guid CompanyId { get; set; }
    }
}