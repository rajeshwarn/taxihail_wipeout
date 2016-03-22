#region

using System.Collections.Generic;
using apcurium.MK.Booking.Api.Contract.Resources;
using ServiceStack.ServiceHost;

#endregion

namespace apcurium.MK.Booking.Api.Contract.Requests
{
    [Route("/vehicles/", "POST")]
    public class AvailableVehicles : IReturn<AvailableVehiclesResponse>
    {
        /// <summary>
        /// Current user latitude
        /// </summary>
        public double Latitude { get; set; }

        /// <summary>
        /// Current user longitude
        /// </summary>
        public double Longitude { get; set; }

        /// <summary>
        /// If specified, will filter available vehicles by vehicle type
        /// </summary>
        public int? VehicleTypeId { get; set; }

        /// <summary>
        /// Radius, in meters, used to search for available vehicle
        /// </summary>
        public int? SearchRadius { get; set; }

        /// <summary>
        /// If specified, will return only available vehicles from those fleets
        /// </summary>
        public int[] FleetIds { get; set; }
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