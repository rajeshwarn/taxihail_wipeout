using System.Collections.Generic;
using ServiceStack.ServiceHost;
using apcurium.MK.Booking.Api.Contract.Resources;

namespace apcurium.MK.Booking.Api.Contract.Requests
{
    [RestService("/vehicles/", "GET")]
    public class AvailableVehicles : IReturn<AvailableVehiclesResponse>
    {
        public double Latitude { get; set; }
        public double Longitude { get; set; }
    }

    public class AvailableVehiclesResponse : List<AvailableVehicle>
    {

    }
}
