﻿#region

using System.Collections.Generic;
using apcurium.MK.Booking.Api.Contract.Resources;
using ServiceStack.ServiceHost;

#endregion

namespace apcurium.MK.Booking.Api.Contract.Requests
{
    [Route("/vehicles/", "POST")]
    public class AvailableVehicles : IReturn<AvailableVehiclesResponse>
    {
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public int? VehicleTypeId { get; set; }
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