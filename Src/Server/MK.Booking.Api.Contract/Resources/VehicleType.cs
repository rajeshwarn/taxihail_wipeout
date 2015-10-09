using System;

namespace apcurium.MK.Booking.Api.Contract.Resources
{
    public class VehicleType
    {
        public Guid Id { get; set; }

        public ServiceType ServiceType { get; set; }

        public string Name { get; set; }

        public string LogoName { get; set; }

        public int ReferenceDataVehicleId { get; set; }

        public int MaxNumberPassengers { get; set; }
    }

    public enum ServiceType
    {
        Taxi = 0,
        Luxury = 1
    }
}