using System;
using apcurium.MK.Common.Http;

namespace apcurium.MK.Booking.Api.Contract.Requests
{
    [RouteDescription("/directions/eta", "GET")]
    public class AssignedEtaRequest : BaseDto
    {
        public Guid OrderId { get; set; }

        public double VehicleLat { get; set; }
        public double VehicleLng { get; set; }
    }
}