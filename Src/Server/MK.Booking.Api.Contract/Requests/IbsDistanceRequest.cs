using System;
using System.Collections.Generic;
using apcurium.MK.Booking.Api.Contract.Resources;
using ServiceStack.ServiceHost;

namespace apcurium.MK.Booking.Api.Contract.Requests
{
    [Route("/ibsdistance/", "GET")]
    public class IbsDistanceRequest : IReturn<IbsDistanceResponse>
    {
        public double Distance { get; set; }
        public int? WaitTime { get; set; }
        public int? StopCount { get; set; }
        public int? PassengerCount { get; set; }
        public int? AccountNumber { get; set; }
        public string CustomerNumber { get; set; }
        public int? VehicleType { get; set; }

    }

    public class IbsDistanceResponse : DirectionInfo
    {
    }
}
