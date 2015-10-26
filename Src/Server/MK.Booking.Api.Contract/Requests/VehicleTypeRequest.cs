using System;
using apcurium.MK.Booking.Api.Contract.Security;
using apcurium.MK.Booking.Security;
using ServiceStack.ServiceHost;
using ServiceStack.ServiceInterface;
using apcurium.MK.Common.Enumeration;

namespace apcurium.MK.Booking.Api.Contract.Requests
{
    [Authenticate]
#if !CLIENT
    [AuthorizationRequired(ApplyTo.Post | ApplyTo.Put | ApplyTo.Delete, RoleName.Admin)]
#endif
    [Route("/admin/vehicletypes", "GET, POST,PUT")]
    [Route("/admin/vehicletypes/{Id}", "GET,DELETE")]
    public class VehicleTypeRequest
    {
        public Guid Id { get; set; }

        public string Name { get; set; }

        public string LogoName { get; set; }

        public int ReferenceDataVehicleId { get; set; }

        public int? ReferenceNetworkVehicleTypeId { get; set; }

        public ServiceType ServiceType { get; set; }

        public int MaxNumberPassengers { get; set; }

        public bool IsWheelchairAccessible { get; set; }
    }
}