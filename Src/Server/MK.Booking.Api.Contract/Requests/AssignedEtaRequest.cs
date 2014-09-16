using System;
using ServiceStack.ServiceHost;

namespace apcurium.MK.Booking.Api.Contract.Requests
{
    [Route("/directions/eta", "GET")]
    public class AssignedEtaRequest : BaseDto
    {
        public Guid OrderId { get; set; }

        public double VehicleLat { get; set; }
        public double VehicleLng { get; set; }
    }
}