using System;

namespace apcurium.MK.Booking.Api.Contract.Resources
{
    public class VehicleType
    {
        public Guid Id { get; set; }

        public string Name { get; set; }

        public string LogoName { get; set; }

        public int ReferenceDataVehicleId { get; set; }
    }
}