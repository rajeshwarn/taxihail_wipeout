#region

using System.Collections.Generic;
using apcurium.MK.Booking.Api.Contract.Resources;

#endregion

namespace apcurium.MK.Booking.Api.Contract.Requests
{
    [Route("/vehicles/", "GET")]
    public class AvailableVehicles : IReturn<AvailableVehiclesResponse>
    {
        public double Latitude { get; set; }
        public double Longitude { get; set; }
    }

    public class AvailableVehiclesResponse : List<AvailableVehicle>
    {
        public AvailableVehiclesResponse()
        {
        }

        public AvailableVehiclesResponse(IEnumerable<AvailableVehicle> list)
            : base(list)
        {
        }
    }
}