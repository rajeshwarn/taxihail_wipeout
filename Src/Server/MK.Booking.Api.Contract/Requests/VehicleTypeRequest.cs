using System;
using ServiceStack.ServiceHost;

namespace apcurium.MK.Booking.Api.Contract.Requests
{
    [Route("/admin/vehicletypes", "GET, POST,PUT")]
    [Route("/admin/vehicletypes/{Id}", "GET,DELETE")]
    public class VehicleTypeRequest
    {
        public Guid Id { get; set; }

        public string Name { get; set; }

        public string LogoName { get; set; }

        public int ReferenceDataVehicleId { get; set; }

        public int? ReferenceNetworkVehicleTypeId { get; set; }

        public int MaxNumberPassengers { get; set; }

        public bool IsWheelchairAccessible { get; set; }
    }
}